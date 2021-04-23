// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Engines.WavePatternSignals_001
{
  using System;
  using System.Collections.Generic;
  using System.Collections.Immutable;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using FFT.Market.Bars;
  using FFT.Market.Engines.WavePattern;
  using FFT.Market.ProcessingContexts;
  using FFT.Market.Signals;
  using FFT.Market.Ticks;
  using FFT.TimeStamps;
  using Settings = FFT.Market.Engines.WavePatternSignals_001.WavePatternSignalsEngine_001Settings;

  public sealed partial class WavePatternSignalsEngine_001 : EngineBase
  {
    /**************************************************************
     * 1. E-X distance must be >= 10 ticks.
     * 2. Size the position so that the E-X distance gain is equal to the
     *    cumulative drawdown.
     * 3. Initial stop is 1 tick below P
     * 4. At X, set stop at bottom of X bar. OR, if X is far away, when
     *    unrealized profit is double the E-X distance, bring stop to breakeven
     *    + 1
     * 5. Trail stop at low of 2 bars behind the current bar.
    **************************************************************/

    private readonly IBars _bars;
    private readonly WavePatternEngine _waveEngine;

    private Tick _tick;
    private Signal? _activeSignal = null;

    public WavePatternSignalsEngine_001(ProcessingContext processingContext, Settings settings, BarsInfo barsInfo)
      : base(processingContext)
    {
      Settings = settings;
      Name = $"{nameof(WavePatternSignalsEngine_001)}_{barsInfo}";
      _bars = processingContext.GetBars(barsInfo);
      _waveEngine = WavePatternEngine.Get(processingContext, new WavePatternEngineSettings(), barsInfo);
    }

    public Settings Settings { get; }

    public override string Name { get; }

    public ImmutableList<Signal> Signals { get; private set; } = ImmutableList<Signal>.Empty;

    private IWave ActiveWave => _waveEngine.CurrentTrendApex;

    public static WavePatternSignalsEngine_001 Get(ProcessingContext processingContext, Settings settings, BarsInfo barsInfo)
      => processingContext.GetEngine(
          search: engine => engine.Settings.Equals(settings) && engine._bars.BarsInfo.Equals(barsInfo),
          create: processingContext => new WavePatternSignalsEngine_001(processingContext, settings, barsInfo));

    public override IEnumerable<object> GetDependencies()
    {
      yield return _waveEngine;
    }

    public override void OnTick(Tick tick)
    {
      if (tick.Instrument != _bars.BarsInfo.Instrument)
        return;

      _tick = tick;

      if (_activeSignal is not null)
      {
        if (ActiveWave.Direction != _activeSignal.Entry!.Direction)
        {
          // cancel this signal due to direction reversal
        }

        if (_activeSignal.EntryFill is not null)
        {
          if (_tick.Price.CompareTo((double)_activeSignal.StopLoss!.Price) * _activeSignal.Entry!.Direction <= 0)
          {
            FillExit(_activeSignal, _tick.TimeStamp, (decimal)_tick.Price, "Stop loss was triggered.");
            _activeSignal = null;
          }
          else
          {
            // adjust sl
          }
        }
        else
        {
          if (_tick.Price.CompareTo((double)_activeSignal.Entry!.Price) * _activeSignal.Entry!.Direction >= 0)
          {
            FillEntry(_activeSignal, _tick.TimeStamp, (decimal)_tick.Price);
          }
          else
          {
            // adjust entry and sl
          }
        }
      }

      if (TryStopOutActiveSignal())
      {
        _activeSignal = null;
      }
      else if (TryTriggerEntryOfActiveSignal())
      {
      }
      else
      {

      }

      if (_waveEngine.Flags.IsAnyFlagSet(WavePatternFlags.SetOrAdjustedETriggervalue))
      {
        if (_activeSignal is null)
        {
          _activeSignal = Create();
          Signals = Signals.Add(_activeSignal);
        }
        else if (_activeSignal.Entry!.Direction != _waveEngine.CurrentTrendApex.Direction)
        {
          Cancel(_activeSignal, tick.TimeStamp, "Apex direction has flipped.");
          _activeSignal = Create();
          Signals = Signals.Add(_activeSignal);
        }
        else
        {

        }
      }
    }

    private bool TryStopOutActiveSignal()
    {
      if (_activeSignal is not null && _activeSignal.EntryFill is not null)
      {

      }
      if (_activeSignal?.EntryFill is not null)
      {
        if (_tick.Price.CompareTo((double)_activeSignal.StopLoss!.Price) * _activeSignal.Entry!.Direction <= 0)
        {
          // Fill at the worst price -- which is always the tick price.
          FillExit(_activeSignal, _tick.TimeStamp, (decimal)_tick.Price, "Stop loss was triggered.");
          return true;
        }
      }

      return false;
    }

    private bool TryTriggerEntryOfActiveSignal()
    {
      if (_activeSignal is not null && _activeSignal.EntryFill is null)
      {
        // Signal entry type is always a stop entry
        if (_tick.Price.CompareTo((double)_activeSignal.Entry!.Price) * _activeSignal.Entry!.Direction >= 0)
        {
          // Fill at the worst price -- which is always the tick price
          FillEntry(_activeSignal, _tick.TimeStamp, (decimal)_tick.Price);
          return true;
        }
      }

      return false;
    }

    private Signal Create()
    {
      var time = _bars.GetTimeStamp(_waveEngine.CurrentTrendApex.A.Index);
      var direction = _waveEngine.CurrentTrendApex.Direction;
      var signal = new Signal(Guid.NewGuid());
      signal.Handle(new CreateSignal
      {
        AggregateId = signal.Id,
        ExpectedVersion = 0,
        At = _tick.TimeStamp,
        Instrument = _bars.BarsInfo.Instrument.Name,
        Exchange = _bars.BarsInfo.Instrument.Exchange.LongName,
        StrategyName = nameof(WavePatternSignalsEngine_001),
        SignalName = $"{time}_{direction}",
      });

      var entryPrice = (decimal)_waveEngine.CurrentTrendApex.ETriggerValue!.Value;
      SetEntry(signal, _tick.TimeStamp, SignalEntryType.Stop, direction, entryPrice, "Initial entry value.");

      var stopPrice = (decimal)_bars.BarsInfo.Instrument.AddIncrements(_waveEngine.CurrentTrendApex.P!.Value, direction * -1);
      SetStop(signal, _tick.TimeStamp, stopPrice, "Initial stop value.");

      return signal;
    }

    private void SetEntry(Signal signal, TimeStamp at, SignalEntryType entryType, Direction direction, decimal price, string tag)
    {
      signal.Handle(new SetEntry
      {
        AggregateId = signal.Id,
        ExpectedVersion = signal.Version,
        At = at,
        Direction = direction,
        EntryType = entryType,
        Price = price,
        Tag = tag,
      });
    }

    private void SetStop(Signal signal, TimeStamp at, decimal price, string tag)
    {
      signal.Handle(new SetStopLoss
      {
        AggregateId = signal.Id,
        ExpectedVersion = signal.Version,
        At = at,
        Price = price,
        Tag = tag,
      });
    }

    private void Cancel(Signal signal, TimeStamp at, string reason)
    {
      signal.Handle(new CancelSignal
      {
        AggregateId = signal.Id,
        At = at,
        ExpectedVersion = signal.Version,
        Reason = reason,
      });
    }

    private void FillEntry(Signal signal, TimeStamp at, decimal price)
    {
      signal.Handle(new FillEntry
      {
        AggregateId = signal.Id,
        At = at,
        ExpectedVersion = signal.Version,
        Price = price,
      });
    }

    private void FillExit(Signal signal, TimeStamp at, decimal price, string reason)
    {
      signal.Handle(new FillExit
      {
        AggregateId = signal.Id,
        At = at,
        ExpectedVersion = signal.Version,
        Price = price,
        Reason = reason,
      });
    }
  }
}
