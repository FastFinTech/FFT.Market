// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Engines.WavePattern
{
  using System;
  using System.Collections.Generic;
  using System.Collections.Immutable;
  using FFT.Market.Bars;
  using FFT.Market.ProcessingContexts;
  using FFT.Market.Ticks;
  using static System.Math;

  public sealed class WavePatternEngine : EngineBase<WavePatternEngineSettings>
  {
    private readonly IBars _bars;
    private readonly double _eDistanceInPoints;
    private readonly double _xDistanceInPoints;
    private readonly double _reversalOffsetInPoints;
    private readonly Initializer _initializer;

    private ImmutableList<WaveLogic> _apexList = ImmutableList<WaveLogic>.Empty;
    private WaveLogic _trendApex;
    private WaveLogic _reversalApex;
    private WaveLogic _lastCompletedApex;

    private int _barIndex;
    private int _previousBarIndex = -1;
    private double _previousTickPrice = -1;
    private uint _flags;
    private uint _reversalFlags;

    private WavePatternEngine(ProcessingContext processingContext, WavePatternEngineSettings settings, BarsInfo barsInfo)
        : base(processingContext, settings)
    {
      _bars = ProcessingContext.GetBars(barsInfo);
      _eDistanceInPoints = _bars.BarsInfo.Instrument.IncrementsToPoints(settings.ETicks);
      _xDistanceInPoints = _bars.BarsInfo.Instrument.IncrementsToPoints(settings.XTicks);
      _reversalOffsetInPoints = _bars.BarsInfo.Instrument.IncrementsToPoints(settings.ReversalOffsetTicks);
      _initializer = new Initializer(_bars);
    }

    public event Action<WavePatternEngine> ETriggered;
    public event Action<WavePatternEngine> XTriggered;
    public event Action<WavePatternEngine> ReversalTriggered;

    public override string Name => "Apex Pattern Engine";
    public IEnumerable<IWave> AllApexes => _apexList;
    public IWave LastCompletedApex => _lastCompletedApex;
    public IWave CurrentTrendApex => _trendApex;
    public IWave CurrentReversalApex => _reversalApex;
    public uint Flags => _flags;
    public uint ReversalFlags => _reversalFlags;

    public double CurrentPowerlineValue
      => _lastCompletedApex is null
        ? _trendApex.P!.Value
        : _lastCompletedApex.P!.Value;

    public double CurrentPowerlineValuePlusReversalOffset
      => _trendApex.Direction.IsUp
        ? CurrentPowerlineValue - _reversalOffsetInPoints
        : CurrentPowerlineValue + _reversalOffsetInPoints;

    /// <summary>
    /// Convenience property, created so the UI can display the price at which
    /// the next trend reversal may occur.
    /// </summary>
    public double? PriceOfNextReversal
    {
      get
      {
        if (_reversalApex is null)
          return null;
        if (_reversalApex.State < WaveStates.FormedP)
          return null;
        if (!IsReversalSignal())
          return null;
        return _reversalApex.XTriggerValue;
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
        if (_trendApex is null)
          return null;
        if (_trendApex.State != WaveStates.FormedP)
          return null;
        return _trendApex.ETriggerValue;
      }
    }

    public static WavePatternEngine Get(ProcessingContext processingContext, WavePatternEngineSettings settings, BarsInfo barsInfo)
      => processingContext.GetEngine(
          search: engine => engine.Settings.Equals(settings) && engine._bars.BarsInfo.Equals(barsInfo),
          create: processingContext => new WavePatternEngine(processingContext, settings, barsInfo));

    public override IEnumerable<object> GetDependencies()
    {
      yield return _bars;
    }

    public override void OnTick(Tick tick)
    {
      _flags = 0;

      if (tick.Instrument != _bars.BarsInfo.Instrument)
        return;

      for (_barIndex = Max(0, _previousBarIndex); _barIndex < _bars.Count; _barIndex++)
      {
        if (_barIndex != _previousBarIndex)
        {
          Process();
          _previousTickPrice = tick.Price;
          _previousBarIndex = _barIndex;
        }
        else if (tick.Price != _previousTickPrice)
        {
          Process();
          _previousTickPrice = tick.Price;
        }
      }
    }

    private void Process()
    {
      // initialization needs to be done if the current trend apex is null
      if (_trendApex is null)
      {
        if (_initializer.TryInitialize(_barIndex, out var direction))
        {
          SetupNewTrendApex(direction);
        }

        // that's all we need to do ... exit the method
        return;
      }

      // lets start by updating the current trend apex
      _flags = _trendApex.Process(_barIndex);

      // if the trend apex completed successfully, then we need to setup a new
      // trend apex using the "continue trend" method
      if (_flags.IsAnyFlagSet(WavePatternFlags.FormedX))
      {
        ContinueTrend();
        XTriggered?.Invoke(this);
      }

      if (_flags.IsAnyFlagSet(WavePatternFlags.FormedE))
      {
        ETriggered?.Invoke(this);
      }

      // now it's time to figure out what to do with the reversal apex half of
      // the time there's no reversal apex actually active in the system.
      if (_reversalApex is null)
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
        _reversalFlags = _reversalApex.Process(_barIndex);

        // did the reversal apex complete itself?
        if (_reversalFlags.IsAnyFlagSet(WavePatternFlags.FormedX))
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
            SetupNewReversalApex(_reversalApex.Direction);
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
      _trendApex.LastIndexOfPowerline = _barIndex;
      if (_lastCompletedApex is not null)
        _lastCompletedApex.LastIndexOfPowerline = _barIndex;
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
      if (_trendApex.State < WaveStates.FormedP)
        return false;

      // Now we wait for price to creep to the correct side of the powerline
      // http://screencast.com/t/iDsMeD1ln90s Note: When null ==
      // LastCompletedApex, this expression ends up returning true whenever
      // CurrentTrendApex (the very first apex of this bar series) forms a new
      // P, because the "CurrentPowerlineValue" property is setup to return the
      // PValue of the CurrentTrendApex when null == LastCompletedApex.
      return _trendApex.Direction.IsUp
          ? _bars.GetLow(_barIndex) <= CurrentPowerlineValue
          : _bars.GetHigh(_barIndex) >= CurrentPowerlineValue;
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
      if (_lastCompletedApex is null)
        return false;

      return _reversalApex.Direction.IsUp
          ? _reversalApex.MinHigh < CurrentPowerlineValue
          : _reversalApex.MaxLow > CurrentPowerlineValue;
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
      _reversalApex = null!;
    }

    /// <summary>
    /// This method is called when a trend apex completes. At this stage, a new
    /// apex needs to be instantiated in the same direction.
    /// </summary>
    private void ContinueTrend()
    {
      // the current trend apex becomes the last completed apex
      _lastCompletedApex = _trendApex;

      // setup a new apex trending in the same direction
      SetupNewTrendApex(_trendApex.Direction);
    }

    /// <summary>
    /// This method is called when a reversal apex has completed and conditions
    /// are correct for it to signal a reversal.
    /// </summary>
    private void Reverse()
    {
      // set the completed reversal apex as the "last completed apex" so it can
      // be used for the powerline value
      _lastCompletedApex = _reversalApex;

      // and add it to the allApexes list so it becomes a "system apex",
      // displayed on charts. It now becomes the first apex of the new trend.
      _apexList = _apexList.Add(_reversalApex);

      // setup the reversal event flags
      _flags.SetFlags(_reversalApex.Direction.IsUp ? WavePatternFlags.SwitchedDirectionUp : WavePatternFlags.SwitchedDirectionDown);

      // as well as the "FormedX" flag (since the reversal apex has just become
      // an official apex, included in the system and it has just formed an X
      _flags.SetFlags(WavePatternFlags.FormedX);

      // since the reversal apex has completed, we need to setup a new "in
      // progress" apex in the direction of the new trend. This method call also
      // sets the NewA flag.
      SetupNewTrendApex(_reversalApex.Direction);

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
      _trendApex = new WaveLogic(direction, _bars, _barIndex, _eDistanceInPoints, _xDistanceInPoints);

      // don't forget to add it to the "_apexList" list so the UI can display it
      _apexList = _apexList.Add(_trendApex);

      // and finally make sure the appropriate flags are set
      _flags.SetFlags(WavePatternFlags.NewA);
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
      _reversalApex = new WaveLogic(direction, _bars, _barIndex, _eDistanceInPoints, _xDistanceInPoints);
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
      if (_lastCompletedApex is null)
        return true;

      // a reversal apex can trigger a reversal in the following situations:
      // Direction.IsUp: All bar highs from its A to X are >= powerline value
      // Direction.IsDown: All bar lows from its A to X are <= powerline value
      return _reversalApex.Direction.IsUp
          ? _reversalApex.MinHigh >= CurrentPowerlineValuePlusReversalOffset
          : _reversalApex.MaxLow <= CurrentPowerlineValuePlusReversalOffset;
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
        _targetDistance = _bars.BarsInfo.Instrument.IncrementsToPoints(50);
      }

      /// <summary>
      /// Returns a known direction if initialization is possible. Otherwise,
      /// returns Direction.Unknown. Initialization is possible when the total
      /// range from low to high of all bars processed so far is fifty ticks or
      /// more.
      /// </summary>
      public bool TryInitialize(int barIndex, out Direction direction)
      {
        var currentHigh = _bars.GetHigh(barIndex);
        var currentLow = _bars.GetLow(barIndex);
        _maxHigh = Max(_maxHigh, currentHigh);
        _minLow = Min(_minLow, currentLow);

        if ((_maxHigh - _minLow) >= _targetDistance)
        {
          direction = _maxHigh == currentHigh ? Direction.Up : Direction.Down;
          return true;
        }

        direction = Direction.Unknown;
        return false;
      }
    }
  }
}
