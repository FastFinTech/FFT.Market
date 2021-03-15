// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Instruments
{
  using FFT.TimeStamps;

  public interface IInstrument
  {
    Asset BaseAsset { get; }

    Asset QuoteAsset { get; }

    SettlementTime SettlementTime { get; }

    double TickSize { get; }

    DateStamp ThisOrNextTradingDay(DateStamp date);

    DateStamp ThisOrPreviousTradingDay(DateStamp date);

    bool IsTradingDay(DateStamp date);

    double TicksToPoints(int ticks)
      => (double)(ticks * (decimal)TickSize);

    double Round2Tick(double value)
      => value.RoundToTick(TickSize);

    double AddTicks(double value, int numTicks)
      => value.AddTicks(TickSize, numTicks);
  }
}
