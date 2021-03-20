// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Engines.SimpleArb
{
  using System;
  using System.Collections.Generic;
  using System.Collections.Immutable;
  using FFT.Market.Instruments;
  using FFT.Market.ProcessingContexts;
  using FFT.Market.Sessions.TradingHoursSessions;
  using FFT.Market.Ticks;
  using FFT.Market.TickStreams;
  using FFT.TimeStamps;

  public sealed class SimpleArbEngine : EngineBase
  {
    private readonly IInstrument _instrument1;
    private readonly IInstrument _instrument2;

    private Tick _lastTick1;
    private Tick _lastTick2;
    private ArbEvent? _currentArb = null;

    private SimpleArbEngine(ProcessingContext processingContext, IInstrument instrument1, IInstrument instrument2)
      : base(processingContext)
    {
      instrument1.EnsureNotNull(nameof(instrument1));
      instrument2.EnsureNotNull(nameof(instrument2));

      if (!_instrument1!.BaseAsset.Equals(_instrument2!.BaseAsset))
        throw new ArgumentException("BaseAsset");
      if (!_instrument1.QuoteAsset.Equals(_instrument2.QuoteAsset))
        throw new ArgumentException("BaseAsset");

      (_instrument1, _instrument2) = (instrument1, instrument2);
    }

    public event Action<SimpleArbEngine, IArbEvent> NewArbCreated;

    public override string Name { get; }

    public IArbEvent? CurrentArg => _currentArb;

    public ImmutableList<IArbEvent> ArbEvents { get; private set; } = ImmutableList<IArbEvent>.Empty;

    public static SimpleArbEngine Get(ProcessingContext processingContext, IInstrument instrument1, IInstrument instrument2)
    {
      return processingContext.GetEngine(
          search: engine => Match(engine, instrument1, instrument2),
          create: processingContext => new SimpleArbEngine(processingContext, instrument1, instrument2));

      static bool Match(SimpleArbEngine engine, IInstrument instrument1, IInstrument instrument2)
        => (engine._instrument1 == instrument1 && engine._instrument2 == instrument2)
        || (engine._instrument1 == instrument2 && engine._instrument2 == instrument1);
    }

    public override IEnumerable<object> GetDependencies()
    {
      yield return _instrument1;
      yield return _instrument2;
    }

    public override void OnTick(Tick tick)
    {
      if (tick.Instrument == _instrument1)
      {
        if (_lastTick2 is not null)
        {
          CheckArb(tick, _lastTick2, tick.TimeStamp);
        }

        _lastTick1 = tick;
      }
      else if (tick.Instrument == _instrument2)
      {
        if (_lastTick1 is not null)
        {
          CheckArb(_lastTick1, tick, tick.TimeStamp);
        }

        _lastTick2 = tick;
      }
    }

    private void CheckArb(Tick tick1, Tick tick2, TimeStamp currentTime)
    {
      if (tick1.Bid > tick2.Ask)
      {
        OnArbExists(buy: _instrument2, sell: _instrument1);
      }
      else if (tick2.Bid > tick1.Ask)
      {
        OnArbExists(buy: _instrument1, sell: _instrument2);
      }
      else
      {
        CloseCurrentArb();
      }

      void OnArbExists(IInstrument buy, IInstrument sell)
      {
        if (_currentArb is not null && _currentArb.Buy != buy)
          CloseCurrentArb();

        if (_currentArb is null)
        {
          _currentArb = new ArbEvent
          {
            At = currentTime,
            Buy = buy,
            Sell = sell,
          };
          ArbEvents = ArbEvents.Add(_currentArb);
          NewArbCreated?.Invoke(this, _currentArb);
        }
      }

      void CloseCurrentArb()
      {
        if (_currentArb is not null)
        {
          _currentArb.Until = currentTime;
          _currentArb = null;
        }
      }
    }

    private sealed class ArbEvent : IArbEvent
    {
      public TimeStamp At { get; set; }
      public TimeStamp? Until { get; set; }
      public IInstrument Buy { get; set; }
      public IInstrument Sell { get; set; }
    }
  }
}
