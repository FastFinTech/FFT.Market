// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Tests
{
  using FFT.Market.Instruments;
  using FFT.TimeStamps;

  public partial class ShortTickStreamTests
  {
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
