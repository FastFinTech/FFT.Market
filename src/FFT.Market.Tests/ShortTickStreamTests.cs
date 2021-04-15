// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Tests
{
  using System;
  using System.Buffers;
  using System.Linq;
  using System.Text;
  using FFT.Market.Instruments;
  using FFT.Market.Ticks;
  using FFT.Market.TickStreams;
  using FFT.TimeStamps;
  using FFT.TimeZoneList;
  using Microsoft.VisualStudio.TestTools.UnitTesting;
  using Nerdbank.Streams;

  [TestClass]
  public class ShortTickStreamTests
  {
    private static readonly IInstrument _bitcoin = new Instrument
    {
      Name = "BTCUSDT",
      BaseAsset = KnownAssets.Crypto_Bitcoin,
      QuoteAsset = KnownAssets.Crypto_Tether,
      Exchange = KnownExchanges.Binance,
      MinPriceIncrement = 0.00000001,
      MinQtyIncrement = 0.00000001,
      SettlementTime = new SettlementTime { TimeZone = TimeZones.America_New_York, TimeOfDay = TimeSpan.FromHours(16) },
    };

    [TestMethod]
    public void ShortTickStream_AsReadOnlySequence()
    {
      var sequence = new Sequence<byte>(ArrayPool<byte>.Shared);
      var tickStream = new ShortTickStream(_bitcoin, sequence);
      tickStream.WriteTick(new Tick
      {
        Instrument = _bitcoin,
        Price = 21,
        Bid = 21,
        Ask = 21.01,
        SequenceNumber = 0,
        TimeStamp = TimeStamp.Now,
        Volume = 5,
      });
      tickStream.WriteTick(new Tick
      {
        Instrument = _bitcoin,
        Price = 21,
        Bid = 21,
        Ask = 21.01,
        SequenceNumber = 0,
        TimeStamp = TimeStamp.Now,
        Volume = 5,
      });
      var allBytes = tickStream.AsReadOnlySequence();
      Assert.IsTrue(allBytes.Length > 0);
    }

    internal sealed record Instrument : IInstrument
    {
      public string Name { get; init; }
      public Asset BaseAsset { get; init; }
      public Asset QuoteAsset { get; init; }
      public Exchange Exchange { get; init; }
      public SettlementTime SettlementTime { get; init; }
      public double MinPriceIncrement { get; init; }
      public double MinQtyIncrement { get; init; }

      public bool IsTradingDay(DateStamp date) => true;

      public DateStamp ThisOrNextTradingDay(DateStamp date) => date;

      public DateStamp ThisOrPreviousTradingDay(DateStamp date) => date;
    }
  }
}
