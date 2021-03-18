// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.BarBuilders
{
  using System.Diagnostics;
  using FFT.Market.TickStreams;
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

      // Get an identifier for the tick stream that contains ticks for this bar series.
      TickStreamInfoValue = new TickStreamInfo(BarsInfo.Instrument, BarsInfo.TradingHours).Value;
    }

    public BarsInfo BarsInfo { get; }
    public IBars Bars { get; }
    public int TickStreamInfoValue { get; }

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
      if (tick.Info.Value != TickStreamInfoValue)
        return;

      SessionIterator.MoveUntil(tick.TimeStamp);

#if DEBUG
      // Check for idiot situations and break so the coder can see it happen.
      if (!SessionIterator.IsInSession)
      {
        // This should never happen, because the tick stream is supposed to be restricted only to in-session ticks.
        // If you ever see this you should have a look at the tick providers and find the bug.
        Debugger.Break();
      }
#endif

      BarBuilderOnTick(tick);
    }

    /// <summary>
    /// Actually processes a tick.
    /// Method assumes that session iterator is initialized correctly and that the timestamp of the
    /// tick is inside an ActualTradingSession.
    /// </summary>
    protected abstract void BarBuilderOnTick(Tick tick);

    protected double ToTick(double price)
      => price.RoundToTick(BarsInfo.Instrument.TickSize);
  }
}
