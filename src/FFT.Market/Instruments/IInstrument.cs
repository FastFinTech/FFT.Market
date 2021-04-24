// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Instruments
{
  using System.Runtime.CompilerServices;
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

    bool IsTradingDay(DateStamp date);

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    double IncrementsToPoints(int numIncrements)
      => (double)(numIncrements * (decimal)MinPriceIncrement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int PointsToIncrements(double points)
      => points.ToIncrements(MinPriceIncrement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    double RoundPrice(double value)
      => value.RoundToIncrement(MinPriceIncrement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    double AddIncrements(double value, int numIncrements)
      => value.AddIncrements(MinPriceIncrement, numIncrements);
  }
}
