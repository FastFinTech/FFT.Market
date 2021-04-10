// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.BarBuilders
{
  using System.Diagnostics;
  using FFT.Market.Bars;
  using FFT.Market.Sessions.TradingHoursSessions;
  using FFT.Market.Ticks;

  /// <summary>
  /// Inherit this class for building bars from ticks.
  /// </summary>
  public abstract class BarBuilder
  {
    protected BarBuilder(BarsInfo barsInfo)
    {
      BarsInfo = barsInfo;
      Bars = new Bars(barsInfo);
      SessionIterator = new TradingSessionIterator(BarsInfo.TradingHours, BarsInfo.FirstSessionDate);
    }

    public BarsInfo BarsInfo { get; }
    public IBars Bars { get; }

    protected TradingSessionIterator SessionIterator { get; }

    /// <summary>
    /// A utility method to create a bar builder from a BarsInfo object, so that you don't have to write 
    /// this switch statement all over your own code.
    /// </summary>
    public static BarBuilder Create(BarsInfo barsInfo)
    {
      return barsInfo.Period switch
      {
        SecondPeriod => new SecondBarBuilder(barsInfo),
        MinutePeriod => new MinuteBarBuilder(barsInfo),
        RangePeriod => new RangeBarBuilder(barsInfo),
        TickPeriod => new TickBarBuilder(barsInfo),
        PriceActionPeriod => new PriceActionBarBuilder(barsInfo),
        _ => throw barsInfo.Period.GetType().UnknownTypeException(),
      };
    }

    public void OnTick(Tick tick)
    {
      // Only process the tick if it belongs to the tick stream for this bar series.
      if (tick.Instrument != BarsInfo.Instrument)
        return;

      SessionIterator.MoveUntil(tick.TimeStamp);

      if (!SessionIterator.IsInSession)
        return;

      BarBuilderOnTick(tick);
    }

    /// <summary>
    /// Actually processes a tick.
    /// Method assumes that session iterator is initialized correctly and that the timestamp of the
    /// tick is inside an ActualTradingSession.
    /// </summary>
    protected abstract void BarBuilderOnTick(Tick tick);

    protected double ToTick(double price)
      => price.RoundToIncrement(BarsInfo.Instrument.MinPriceIncrement);
  }
}
