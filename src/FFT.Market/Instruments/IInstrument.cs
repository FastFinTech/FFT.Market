// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Instruments
{
  using FFT.TimeStamps;

  public interface IInstrument
  {
    string Name { get; }

    Asset BaseAsset { get; }

    Asset QuoteAsset { get; }

    Exchange Exchange { get; }

    SettlementTime SettlementTime { get; }

    double MinPriceIncrement { get; }

    double MinQtyIncrement { get; }

    DateStamp ThisOrNextTradingDay(DateStamp date)
    {
      while (!IsTradingDay(date))
        date = date.AddDays(1);
      return date;
    }

    DateStamp ThisOrPreviousTradingDay(DateStamp date)
    {
      while (!IsTradingDay(date))
        date = date.AddDays(-1);
      return date;
    }

    bool IsTradingDay(DateStamp date);

    double IncrementsToPoints(int numIncrements)
      => (double)(numIncrements * (decimal)MinPriceIncrement);

    double RoundPrice(double value)
      => value.RoundToIncrement(MinPriceIncrement);

    double AddIncrements(double value, int numIncrements)
      => value.AddIncrements(MinPriceIncrement, numIncrements);
  }
}
