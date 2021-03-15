// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Engines.ApexPattern
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using FFT.Market.Bars;
  using FFT.Market.DependencyTracking;
  using FFT.Market.ProcessingContexts;
  using FFT.Market.Ticks;
  using static System.Math;

  public class ApexPatternEngine : EngineBase<ApexPatternEngineSettings>
  {
    public event Action<ApexPatternEngine> ETriggered, XTriggered, ReversalTriggered;

    public override string Name => "Apex Pattern Engine";

    readonly IBars Bars;
    readonly double EDistanceInPoints;
    readonly double XDistanceInPoints;
    readonly double ReversalOffsetInPoints;
    readonly Initializer initializer;
    readonly int TickStreamIdValue;

    List<ApexLogic> allApexes = new();
    ApexLogic currentTrendApexLogic;
    ApexLogic currentReversalApexLogic;

    private ApexPatternEngine(ProcessingContext processingContext, ApexPatternEngineSettings settings, BarsInfo barsInfo)
        : base(processingContext, settings)
    {
      Bars = ProcessingContext.GetBars(barsInfo);
      EDistanceInPoints = Bars.BarsInfo.Instrument.TicksToPoints(settings.ETicks);
      XDistanceInPoints = Bars.BarsInfo.Instrument.TicksToPoints(settings.XTicks);
      ReversalOffsetInPoints = Bars.BarsInfo.Instrument.TicksToPoints(settings.ReversalOffsetTicks);
      initializer = new Initializer(Bars);
      TickStreamIdValue = this.GetNonProviderTickStreamDependenciesRecursive().Single().Value;
    }

    public IEnumerable<IAPEX> AllApexes => allApexes;
    public IAPEX LastCompletedApex { get; private set; }
    public IAPEX CurrentTrendApex => currentTrendApexLogic;
    public IAPEX CurrentReversalApex => currentReversalApexLogic;

    public double CurrentPowerlineValue
      => LastCompletedApex is null
        ? CurrentTrendApex.P!.Value
        : LastCompletedApex.P!.Value;

    public double CurrentPowerlineValuePlusReversalOffset
      => CurrentTrendApex.Direction.IsUp
        ? CurrentPowerlineValue - ReversalOffsetInPoints
        : CurrentPowerlineValue + ReversalOffsetInPoints;

    /// <summary>
    /// Convenience property, created so the UI can display the price at which
    /// the next trend reversal may occur.
    /// </summary>
    public double? PriceOfNextReversal
    {
      get
      {
        if (CurrentReversalApex is null)
          return null;
        if (CurrentReversalApex.State < ApexStates.FormedP)
          return null;
        if (!IsReversalSignal())
          return null;
        return CurrentReversalApex.XTriggerValue;
      }
    }

    /// <summary>
    /// Convenience property, created so the UI can display the price at which
    /// the next E may be triggered.
    /// </summary>
    public double? PriceOfNextE
    {
      get
      {
        if (CurrentTrendApex is null)
          return null;
        if (CurrentTrendApex.State != ApexStates.FormedP)
          return null;
        return CurrentTrendApex.ETriggerValue;
      }
    }

    public static ApexPatternEngine Get(ProcessingContext processingContext, ApexPatternEngineSettings settings, BarsInfo barsInfo)
      => processingContext.GetEngine(
          search: engine => engine.Settings.Equals(settings) && engine.Bars.BarsInfo.Equals(barsInfo),
          create: processingContext => new ApexPatternEngine(processingContext, settings, barsInfo));

    public override IEnumerable<object> GetDependencies()
    {
      yield return Bars;
    }

    // these variables essentially belong inside the Process method,
    // but there are so many methods to be called from there, and I didn't want to 
    // explicitly pass them into each method that was called (all the typing would be horrendous)
    // so I just stuck them here. Obviously, this isn't setup to be used in a multi-threaded way.
    int barIndex, previousBarIndex = -1;
    double previousTickPrice = -1;
    public ApexPatternFlags Flags;
    ApexPatternFlags reversalFlags;

    public override void OnTick(Tick tick)
    {
      if (tick.Info.Value != TickStreamIdValue) return;

      for (barIndex = Max(0, previousBarIndex); barIndex < Bars.Count; barIndex++)
      {
        if (barIndex != previousBarIndex)
        {
          Process();
          previousTickPrice = tick.Price;
          previousBarIndex = barIndex;
        }
        else if (tick.Price != previousTickPrice)
        {
          Process();
          previousTickPrice = tick.Price;
        }
      }
    }

    private void Process()
    {
      // initialization needs to be done if the current trend apex is null
      if (CurrentTrendApex is null)
      {
        Flags = 0;
        var initializationDirection = initializer.TryInitialize(barIndex);

        // initialization can be performed if a known direction has been returned
        if (!initializationDirection.IsUnknown)
        {
          // initialize by setting up the first trend apex
          // this method also sets the flags.NewA flag
          SetupNewTrendApex(initializationDirection);
        }

        // that's all we need to do ... exit the method
        return;
      }

      // lets start by updating the current trend apex
      Flags = currentTrendApexLogic.Process(barIndex);

      // if the trend apex completed successfully, then we need to setup a new
      // trend apex using the "continue trend" method
      if (Flags.HasFlag(ApexPatternFlags.FormedX))
      {
        ContinueTrend();
        XTriggered?.Invoke(this);
      }

      if (Flags.HasFlag(ApexPatternFlags.FormedE))
      {
        ETriggered?.Invoke(this);
      }

      // now it's time to figure out what to do with the reversal apex half of
      // the time there's no reversal apex actually active in the system.
      if (CurrentReversalApex is null)
      {
        // let's go ahead and setup a new reversal apex if one should be created
        // now
        if (ShouldCreateReversalApex())
        {
          SetupNewReversalApex(CurrentTrendApex.Direction.Opposite);
        }
      }
      else
      {
        // a reversal apex is already active in the system. let's start by
        // having it process this bar
        reversalFlags = currentReversalApexLogic.Process(barIndex);

        // did the reversal apex complete itself?
        if (reversalFlags.HasFlag(ApexPatternFlags.FormedX))
        {
          // If the reversal apex satisfies the conditions to signal a reversal,
          // we'll perform the reversal
          if (IsReversalSignal())
          {
            Reverse();
            XTriggered?.Invoke(this);
            ReversalTriggered?.Invoke(this);
          }
          else
          {
            // otherwise we need to setup a new reversal apex
            SetupNewReversalApex(CurrentReversalApex.Direction);
          }
        }
        else
        {
          // The reversal apex did not complete itself. Let's check if it needs
          // to be cleared and do so if necessary.
          if (ShouldClearReversalApex())
          {
            ClearReversalApex();
          }
        }
      }

      // the last thing to do is a litte bit of sugar for the UI. Just adjust
      // the bar index to which each apex's powerline should be drawn
      if (CurrentTrendApex is not null)
        CurrentTrendApex.LastIndexOfPowerline = barIndex;
      if (LastCompletedApex is not null)
        LastCompletedApex.LastIndexOfPowerline = barIndex;
    }

    /// <summary>
    /// Returns true if a reversal apex should be formed. Method assumes that a
    /// reversal apex does not already exist.
    /// </summary>
    private bool ShouldCreateReversalApex()
    {
      // If the current trend apex's state is still in "FormedA", then there are
      // no bars that can possibly be used as A's for a reversal apex. There
      // needs to be price movement in the opposite direction first.
      if (CurrentTrendApex.State < ApexStates.FormedP)
        return false;

      // Now we wait for price to creep to the correct side of the powerline
      // http://screencast.com/t/iDsMeD1ln90s Note: When null ==
      // LastCompletedApex, this expression ends up returning true whenever
      // CurrentTrendApex (the very first apex of this bar series) forms a new
      // P, because the "CurrentPowerlineValue" property is setup to return the
      // PValue of the CurrentTrendApex when null == LastCompletedApex.
      return CurrentTrendApex.Direction.IsUp
          ? Bars.GetLow(barIndex) <= CurrentPowerlineValue
          : Bars.GetHigh(barIndex) >= CurrentPowerlineValue;
    }

    /// <summary>
    /// Returns true if the reversal apex should be cleared. Method assumes that
    /// null != CurrentReversalApex.
    /// </summary>
    private bool ShouldClearReversalApex()
    {
      // if the reversal apex has crept onto the wrong side of the powerline
      // then we clear it this lets us reset the reversal apex when price creeps
      // onto the correct side of the powerline
      // http://screencast.com/t/iDsMeD1ln90s

      // First run a little test to fix this edge case,
      // http://screencast.com/t/glHoY9NMCH which was caused by continually
      // incorrectly clearing a valid reversal apex when the LastCompletedApex
      // was null. Reason it happened: When LastCompletedApex is null, the
      // CurrentPowerLineValue is set to the P of the trend apex in progress,
      // which resulted in the final expression always incorrectly returning
      // true
      if (LastCompletedApex is null)
        return false;

      return CurrentReversalApex.Direction.IsUp
          ? CurrentReversalApex.MinHigh < CurrentPowerlineValue
          : CurrentReversalApex.MaxLow > CurrentPowerlineValue;
    }

    /// <summary>
    /// This method is called when conditions are no longer valid for a reversal
    /// apex to exist in the system. It simply removes whatever reversal apex
    /// exists by setting them null. The cleared reversal apexes where never
    /// added to the allApexes list ... they simply disappear as though they
    /// never existed.
    /// </summary>
    private void ClearReversalApex()
    {
      CurrentReversalApex = null!;
      currentReversalApexLogic = null!;
    }

    /// <summary>
    /// This method is called when a trend apex completes. At this stage, a new
    /// apex needs to be instantiated in the same direction.
    /// </summary>
    private void ContinueTrend()
    {
      // the current trend apex becomes the last completed apex
      LastCompletedApex = CurrentTrendApex;

      // setup a new apex trending in the same direction
      SetupNewTrendApex(CurrentTrendApex.Direction);
    }

    /// <summary>
    /// This method is called when a reversal apex has completed and conditions
    /// are correct for it to signal a reversal.
    /// </summary>
    private void Reverse()
    {
      // set the completed reversal apex as the "last completed apex" so it can
      // be used for the powerline value
      LastCompletedApex = CurrentReversalApex;

      // and add it to the allApexes list so it becomes a "system apex",
      // displayed on charts. It now becomes the first apex of the new trend.
      allApexes.Add(CurrentReversalApex);

      // setup the reversal event flags
      Flags |= CurrentReversalApex.Direction.IsUp ? ApexPatternFlags.SwitchedDirectionUp : ApexPatternFlags.SwitchedDirectionDown;

      // as well as the "FormedX" flag (since the reversal apex has just become
      // an official apex, included in the system and it has just formed an X
      Flags |= ApexPatternFlags.FormedX;

      // since the reversal apex has completed, we need to setup a new "in
      // progress" apex in the direction of the new trend. This method call also
      // sets the NewA flag.
      SetupNewTrendApex(CurrentReversalApex.Direction);

      // and of course, the reversal apex needs to be cleared because it has
      // completed. We'll have to wait for conditions to be right before we
      // setup a new reversal apex.
      ClearReversalApex();
    }

    /// <summary>
    /// Creates a new apex in the direction of the trend. This is a utility
    /// method, used by the following: 1. Initialization uses it to setup the
    /// first apex. 2. ContinueTrend() uses it to add the next apex when a trend
    /// apex completes 3. Reverse() uses it to add the next apex when a reversal
    /// apex completes and triggers a reversal.
    /// </summary>
    private void SetupNewTrendApex(Direction direction)
    {
      // create the new trend apex, saving it in the appropriate variables
      currentTrendApexLogic = ApexLogic.Create(direction, Bars, barIndex, EDistanceInPoints, XDistanceInPoints);
      CurrentTrendApex = currentTrendApexLogic.Apex;

      // don't forget to add it to the "allApexes" list so the UI can display it
      allApexes.Add(CurrentTrendApex);

      // and finally make sure the appropriate flags are set
      Flags |= ApexPatternFlags.NewA;
    }

    /// <summary>
    /// Creates a new apex against the direction of the trend. This is a utility
    /// method, used by the following: 1. When no reversal apex exists, and
    /// conditions are right to create one 2. When a reversal apex completed but
    /// could not trigger a trend reversal, a new reversal apex needs to be
    /// setup.
    /// </summary>
    private void SetupNewReversalApex(Direction direction)
    {
      // create the new reversal apex and set the variables for it.
      currentReversalApexLogic = ApexLogic.Create(direction, Bars, barIndex, EDistanceInPoints, XDistanceInPoints);
      CurrentReversalApex = currentReversalApexLogic.Apex;
    }

    /// <summary>
    /// This method is called when the reversal apex has just been completed.
    /// Returns true if the just-completed reversal apex can also trigger a
    /// reversal signal.
    /// </summary>
    private bool IsReversalSignal()
    {
      // First fix this edge case, http://screencast.com/t/glHoY9NMCH which was
      // ALSO caused by not accepting the reversal signal of the reversal apex
      // at the beginning of the chart
      if (LastCompletedApex is null)
        return true;

      // a reversal apex can trigger a reversal in the following situations:
      // Direction.IsUp: All bar highs from its A to X are >= powerline value
      // Direction.IsDown: All bar lows from its A to X are <= powerline value
      return CurrentReversalApex.Direction.IsUp
          ? CurrentReversalApex.MinHigh >= CurrentPowerlineValuePlusReversalOffset
          : CurrentReversalApex.MaxLow <= CurrentPowerlineValuePlusReversalOffset;
    }

    private class Initializer
    {
      private readonly IBars _bars;
      private readonly double _targetDistance;

      private double _maxHigh = double.MinValue;
      private double _minLow = double.MaxValue;

      public Initializer(IBars bars)
      {
        _bars = bars;
        _targetDistance = _bars.BarsInfo.Instrument.TicksToPoints(50);
      }

      /// <summary>
      /// Returns a known direction if initialization is possible. Otherwise,
      /// returns Direction.Unknown. Initialization is possible when the total
      /// range from low to high of all bars processed so far is fifty ticks or
      /// more.
      /// </summary>
      public Direction TryInitialize(int barIndex)
      {
        var currentHigh = _bars.GetHigh(barIndex);
        var currentLow = _bars.GetLow(barIndex);
        _maxHigh = Max(_maxHigh, currentHigh);
        _minLow = Min(_minLow, currentLow);

        if ((_maxHigh - _minLow) >= _targetDistance)
        {
          return _maxHigh == currentHigh
              ? Direction.Up
              : Direction.Down;
        }

        return Direction.Unknown;
      }
    }
  }
}
