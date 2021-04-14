// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Tests
{
  using System;
  using FFT.Market.Signals;
  using FFT.TimeStamps;
  using Microsoft.VisualStudio.TestTools.UnitTesting;

  [TestClass]
  public class SignalsTests
  {
    [TestMethod]
    public void TestSignals()
    {
      var signalId = Guid.NewGuid();
      var signal = new Signal(signalId);

      signal.Handle(new CreateSignal
      {
        AggregateId = signalId,
        At = TimeStamp.Now,
        ExpectedVersion = 0,
        Instrument = "Bitcoin",
        Exchange = "Binance",
        SignalName = "Signal01",
        StrategyName = "MyStrategy",
      });

      signal.Handle(new SetEntry
      {
        AggregateId = signalId,
        At = TimeStamp.Now,
        ExpectedVersion = 1,
        Direction = Direction.Up,
        EntryType = SignalEntryType.Limit,
        Price = 100,
        Tag = "The best entry there ever was",
      });
    }
  }
}
