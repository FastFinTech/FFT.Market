// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.BarBuilders
{
  using System;
  using FFT.Market.Bars;
  using FFT.Market.Bars.Periods;
  using FFT.Market.Instruments;
  using FFT.Market.Ticks;
  using static System.Math;

  public sealed class PriceActionBarBuilder : BarBuilder
  {
    private readonly double _initialTrendBarSizeInPoints;
    private readonly double _initialReversalBarSizeInPoints;

    public PriceActionBarBuilder(BarsInfo barsInfo)
      : base(barsInfo)
    {
      Period = (barsInfo.Period as PriceActionPeriod) ?? throw new ArgumentException("period");
      _initialTrendBarSizeInPoints = TrendBarSizeInPoints = barsInfo.Instrument.IncrementsToPoints(Period.TrendBarSizeInTicks);
      _initialReversalBarSizeInPoints = ReversalBarSizeInPoints = barsInfo.Instrument.IncrementsToPoints(Period.ReversalBarSizeInTicks);
    }

    public PriceActionPeriod Period { get; }
    public IBar BarInProgress { get; private set; }
    public double TrendBarSizeInPoints { get; }
    public double ReversalBarSizeInPoints { get; }
    public double CurrentBarMaxHigh { get; private set; }
    public double CurrentBarMinLow { get; private set; }
    public double NextOpenUp { get; private set; }
    public double NextOpenDown { get; private set; }
    public double NextBarMaxHigh { get; private set; }
    public double NextBarMinLow { get; private set; }
    public Direction Trend { get; private set; } = Direction.Up;

    protected override void BarBuilderOnTick(Tick tick)
    {
      if (BarInProgress is null || SessionIterator.IsFirstTickOfSession)
      {
        // It's important to reset ALL calculation variables on a new session,
        // to prevent data sync issues when bars series are built from different
        // start dates.
        Trend = Direction.Up;
        StartNewBar(tick, tick.Price);
      }
      else if (tick.Price > CurrentBarMaxHigh)
      {
        CloseBarAtMaxHigh();
        Trend = Direction.Up;

        if (tick.Price > NextBarMaxHigh)
        {
          StartNewBar(tick, tick.Price);
        }
        else
        {
          StartNewBar(tick, NextOpenUp);
        }
      }
      else if (tick.Price < CurrentBarMinLow)
      {
        CloseBarAtMinLow();
        Trend = Direction.Down;

        if (tick.Price < NextBarMinLow)
        {
          StartNewBar(tick, tick.Price);
        }
        else
        {
          StartNewBar(tick, NextOpenDown);
        }
      }
      else
      {
        UpdateBar(tick);
      }
    }

    private void StartNewBar(Tick tick, double openPrice)
    {
      if (Trend.IsUp)
      {
        CurrentBarMaxHigh = BarsInfo.Instrument.RoundPrice(openPrice + TrendBarSizeInPoints);
        CurrentBarMinLow = BarsInfo.Instrument.RoundPrice(openPrice - ReversalBarSizeInPoints);
      }
      else
      {
        CurrentBarMaxHigh = BarsInfo.Instrument.RoundPrice(openPrice + ReversalBarSizeInPoints);
        CurrentBarMinLow = BarsInfo.Instrument.RoundPrice(openPrice - TrendBarSizeInPoints);
      }

      NextOpenUp = BarsInfo.Instrument.AddIncrements(CurrentBarMaxHigh, 1);
      NextOpenDown = BarsInfo.Instrument.AddIncrements(CurrentBarMinLow, -1);
      NextBarMaxHigh = BarsInfo.Instrument.RoundPrice(NextOpenUp + TrendBarSizeInPoints);
      NextBarMinLow = BarsInfo.Instrument.RoundPrice(NextOpenDown - TrendBarSizeInPoints);

      BarInProgress = new Bar();
      BarInProgress.Open = openPrice;
      BarInProgress.High = Max(openPrice, tick.Price);
      BarInProgress.Low = Min(openPrice, tick.Price);
      BarInProgress.Close = tick.Price;
      BarInProgress.Volume = tick.Volume;
      BarInProgress.TimeStamp = tick.TimeStamp;
      Bars.AddNewBar(BarInProgress);
    }

    private void UpdateBar(Tick tick)
    {
      BarInProgress.High = Max(BarInProgress.High, tick.Price);
      BarInProgress.Low = Min(BarInProgress.Low, tick.Price);
      BarInProgress.Close = tick.Price;
      BarInProgress.Volume += tick.Volume;
      BarInProgress.TimeStamp = tick.TimeStamp;
      BarInProgress.TickCount++;
    }

    private void CloseBarAtMaxHigh()
      => BarInProgress.High = BarInProgress.Close = CurrentBarMaxHigh;

    private void CloseBarAtMinLow()
      => BarInProgress.Low = BarInProgress.Close = CurrentBarMinLow;
  }
}
