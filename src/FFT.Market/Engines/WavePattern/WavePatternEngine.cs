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

    private ImmutableList<WaveLogic> _waves = ImmutableList<WaveLogic>.Empty;
    private WaveLogic _trendWave;
    private WaveLogic _reversalWave;
    private WaveLogic _lastCompletedWave;

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

    public ImmutableList<IWave> Waves { get; private set; } = ImmutableList<IWave>.Empty;
    public override string Name => "Wave Pattern Engine";
    public IWave LastCompletedWave => _lastCompletedWave;
    public IWave CurrentTrendWave => _trendWave;
    public IWave CurrentReversalWave => _reversalWave;

    public double CurrentPowerlineValue
      => _lastCompletedWave is null
        ? _trendWave.P!.Value
        : _lastCompletedWave.P!.Value;

    public double CurrentPowerlineValuePlusReversalOffset
      => _trendWave.Direction.IsUp
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
        if (_reversalWave is null)
          return null;
        if (_reversalWave.State < WaveStates.FormedP)
          return null;
        if (!IsReversalSignal())
          return null;
        return _reversalWave.XTriggerValue;
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
        if (_trendWave is null)
          return null;
        if (_trendWave.State != WaveStates.FormedP)
          return null;
        return _trendWave.ETriggerValue;
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
      // initialization needs to be done if the current trend wave is null
      if (_trendWave is null)
      {
        if (_initializer.TryInitialize(_barIndex, out var direction))
        {
          SetupNewTrendWave(direction);
        }

        // that's all we need to do ... exit the method
        return;
      }

      // lets start by updating the current trend wave
      _flags = _trendWave.Process(_barIndex);

      // if the trend wave completed successfully, then we need to setup a new
      // trend wave using the "continue trend" method
      if (_flags.HasFlag(WavePatternFlags.FormedX))
      {
        ContinueTrend();
        XTriggered?.Invoke(this);
      }

      if (_flags.IsAnyFlagSet(WavePatternFlags.FormedE))
      {
        ETriggered?.Invoke(this);
      }

      // now it's time to figure out what to do with the reversal wave half of
      // the time there's no reversal wave actually active in the system.
      if (_reversalWave is null)
      {
        // let's go ahead and setup a new reversal wave if one should be created
        // now
        if (ShouldCreateReversalWave())
        {
          SetupNewReversalWave(CurrentTrendWave.Direction.Opposite);
        }
      }
      else
      {
        // a reversal wave is already active in the system. let's start by
        // having it process this bar
        _reversalFlags = _reversalWave.Process(_barIndex);

        // did the reversal wave complete itself?
        if (_reversalFlags.HasFlag(WavePatternFlags.FormedX))
        {
          // If the reversal wave satisfies the conditions to signal a reversal,
          // we'll perform the reversal
          if (IsReversalSignal())
          {
            Reverse();
            XTriggered?.Invoke(this);
            ReversalTriggered?.Invoke(this);
          }
          else
          {
            // otherwise we need to setup a new reversal wave
            SetupNewReversalWave(_reversalWave.Direction);
          }
        }
        else
        {
          // The reversal wave did not complete itself. Let's check if it needs
          // to be cleared and do so if necessary.
          if (ShouldClearReversalWave())
          {
            ClearReversalWave();
          }
        }
      }

      // the last thing to do is a litte bit of sugar for the UI. Just adjust
      // the bar index to which each wave's powerline should be drawn
      _trendWave.LastIndexOfPowerline = _barIndex;
      if (_lastCompletedWave is not null)
        _lastCompletedWave.LastIndexOfPowerline = _barIndex;
    }

    /// <summary>
    /// Returns true if a reversal wave should be formed. Method assumes that a
    /// reversal wave does not already exist.
    /// </summary>
    private bool ShouldCreateReversalWave()
    {
      // If the current trend wave's state is still in "FormedA", then there are
      // no bars that can possibly be used as A's for a reversal wave. There
      // needs to be price movement in the opposite direction first.
      if (_trendWave.State < WaveStates.FormedP)
        return false;

      // Now we wait for price to creep to the correct side of the powerline
      // http://screencast.com/t/iDsMeD1ln90s Note: When null ==
      // LastCompletedWave, this expression ends up returning true whenever
      // CurrentTrendWave (the very first wave of this bar series) forms a new
      // P, because the "CurrentPowerlineValue" property is setup to return the
      // PValue of the CurrentTrendWave when null == LastCompletedWave.
      return _trendWave.Direction.IsUp
          ? _bars.GetLow(_barIndex) <= CurrentPowerlineValue
          : _bars.GetHigh(_barIndex) >= CurrentPowerlineValue;
    }

    /// <summary>
    /// Returns true if the reversal wave should be cleared. Method assumes that
    /// null != CurrentReversalWave.
    /// </summary>
    private bool ShouldClearReversalWave()
    {
      // if the reversal wave has crept onto the wrong side of the powerline
      // then we clear it this lets us reset the reversal wave when price creeps
      // onto the correct side of the powerline
      // http://screencast.com/t/iDsMeD1ln90s

      // First run a little test to fix this edge case,
      // http://screencast.com/t/glHoY9NMCH which was caused by continually
      // incorrectly clearing a valid reversal wave when the LastCompletedWave
      // was null. Reason it happened: When LastCompletedWave is null, the
      // CurrentPowerLineValue is set to the P of the trend wave in progress,
      // which resulted in the final expression always incorrectly returning
      // true
      if (_lastCompletedWave is null)
        return false;

      return _reversalWave.Direction.IsUp
          ? _reversalWave.MinHigh < CurrentPowerlineValue
          : _reversalWave.MaxLow > CurrentPowerlineValue;
    }

    /// <summary>
    /// This method is called when conditions are no longer valid for a reversal
    /// wave to exist in the system. It simply removes whatever reversal wave
    /// exists by setting them null. The cleared reversal waves where never
    /// added to the allWaves list ... they simply disappear as though they
    /// never existed.
    /// </summary>
    private void ClearReversalWave()
    {
      _reversalWave = null!;
    }

    /// <summary>
    /// This method is called when a trend wave completes. At this stage, a new
    /// wave needs to be instantiated in the same direction.
    /// </summary>
    private void ContinueTrend()
    {
      // the current trend wave becomes the last completed wave
      _lastCompletedWave = _trendWave;

      // setup a new wave trending in the same direction
      SetupNewTrendWave(_trendWave.Direction);
    }

    /// <summary>
    /// This method is called when a reversal wave has completed and conditions
    /// are correct for it to signal a reversal.
    /// </summary>
    private void Reverse()
    {
      // set the completed reversal wave as the "last completed wave" so it can
      // be used for the powerline value
      _lastCompletedWave = _reversalWave;

      // and add it to the allWaves list so it becomes a "system wave",
      // displayed on charts. It now becomes the first wave of the new trend.
      _waves = _waves.Add(_reversalWave);
      Waves = Waves.Add(_reversalWave);

      // setup the reversal event flags
      _flags |= _reversalWave.Direction.IsUp ? WavePatternFlags.SwitchedDirectionUp : WavePatternFlags.SwitchedDirectionDown;

      // as well as the "FormedX" flag (since the reversal wave has just become
      // an official wave, included in the system and it has just formed an X
      _flags |= WavePatternFlags.FormedX;

      // since the reversal wave has completed, we need to setup a new "in
      // progress" wave in the direction of the new trend. This method call also
      // sets the NewA flag.
      SetupNewTrendWave(_reversalWave.Direction);

      // and of course, the reversal wave needs to be cleared because it has
      // completed. We'll have to wait for conditions to be right before we
      // setup a new reversal wave.
      ClearReversalWave();
    }

    /// <summary>
    /// Creates a new wave in the direction of the trend. This is a utility
    /// method, used by the following: 1. Initialization uses it to setup the
    /// first wave. 2. ContinueTrend() uses it to add the next wave when a trend
    /// wave completes 3. Reverse() uses it to add the next wave when a reversal
    /// wave completes and triggers a reversal.
    /// </summary>
    private void SetupNewTrendWave(Direction direction)
    {
      // create the new trend wave, saving it in the appropriate variables
      _trendWave = new WaveLogic(direction, _bars, _barIndex, _eDistanceInPoints, _xDistanceInPoints);

      // don't forget to add it to the "_waveList" list so the UI can display it
      _waves = _waves.Add(_trendWave);
      Waves = Waves.Add(_trendWave);

      // and finally make sure the appropriate flags are set
      _flags.SetFlags(WavePatternFlags.NewA);
    }

    /// <summary>
    /// Creates a new wave against the direction of the trend. This is a utility
    /// method, used by the following: 1. When no reversal wave exists, and
    /// conditions are right to create one 2. When a reversal wave completed but
    /// could not trigger a trend reversal, a new reversal wave needs to be
    /// setup.
    /// </summary>
    private void SetupNewReversalWave(Direction direction)
    {
      // create the new reversal wave and set the variables for it.
      _reversalWave = new WaveLogic(direction, _bars, _barIndex, _eDistanceInPoints, _xDistanceInPoints);
    }

    /// <summary>
    /// This method is called when the reversal wave has just been completed.
    /// Returns true if the just-completed reversal wave can also trigger a
    /// reversal signal.
    /// </summary>
    private bool IsReversalSignal()
    {
      // First fix this edge case, http://screencast.com/t/glHoY9NMCH which was
      // ALSO caused by not accepting the reversal signal of the reversal wave
      // at the beginning of the chart
      if (_lastCompletedWave is null)
        return true;

      // a reversal wave can trigger a reversal in the following situations:
      // Direction.IsUp: All bar highs from its A to X are >= powerline value
      // Direction.IsDown: All bar lows from its A to X are <= powerline value
      return _reversalWave.Direction.IsUp
          ? _reversalWave.MinHigh >= CurrentPowerlineValuePlusReversalOffset
          : _reversalWave.MaxLow <= CurrentPowerlineValuePlusReversalOffset;
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
