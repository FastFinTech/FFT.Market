// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.BarBuilders
{
  using System;
  using FFT.Market.Bars;
  using FFT.Market.Ticks;
  using FFT.TimeStamps;
  using static System.Math;

  public class SecondBarBuilder : BarBuilder
  {
    private readonly SecondPeriod _period;

    private IBar _barInProgress;
    private TimeStamp _barEndTime;

    public SecondBarBuilder(BarsInfo barsInfo)
      : base(barsInfo)
    {
      _period = (barsInfo.Period as SecondPeriod) ?? throw new ArgumentException("period");
    }

    protected override void BarBuilderOnTick(Tick tick)
    {
      if (_barInProgress is null || tick.TimeStamp > _barEndTime)
      {
        StartNewBar(tick);
      }
      else
      {
        UpdateBar(tick);
      }
    }

    private void StartNewBar(Tick tick)
    {
      _barEndTime = tick.TimeStamp.ToIntervalCeiling(SessionIterator.Current.SessionStart, TimeSpan.FromSeconds(_period.PeriodInSeconds));
      _barEndTime = TimeStamp.Min(_barEndTime, SessionIterator.Current.SessionEnd);

      _barInProgress = new Bar();
      _barInProgress.Open = _barInProgress.High = _barInProgress.Low = _barInProgress.Close = tick.Price;
      _barInProgress.Volume = tick.Volume;
      _barInProgress.TimeStamp = _barEndTime;
      Bars.AddNewBar(_barInProgress);
    }

    private void UpdateBar(Tick tick)
    {
      _barInProgress.High = Max(_barInProgress.High, tick.Price);
      _barInProgress.Low = Min(_barInProgress.Low, tick.Price);
      _barInProgress.Close = tick.Price;
      _barInProgress.Volume += tick.Volume;
      _barInProgress.TickCount++;
    }
  }
}
