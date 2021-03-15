// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.BarBuilders
{
  using System;
  using FFT.Market.Bars;
  using FFT.Market.Ticks;
  using static System.Math;

  public class TickBarBuilder : BarBuilder
  {
    private readonly TickPeriod _period;

    private IBar _barInProgress;

    public TickBarBuilder(BarsInfo info)
      : base(info)
    {
      _period = (TickPeriod)info.Period;
    }

    protected override void BarBuilderOnTick(Tick tick)
    {
      if (_barInProgress is null || _barInProgress.TickCount == _period.TicksPerBar || SessionIterator.IsFirstTickOfSession)
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
      _barInProgress = new Bar();
      _barInProgress.Open = _barInProgress.High = _barInProgress.Low = _barInProgress.Close = tick.Price;
      _barInProgress.Volume = tick.Volume;
      _barInProgress.TimeStamp = tick.TimeStamp.AddTicks(-1);
      Bars.AddNewBar(_barInProgress);
    }

    private void UpdateBar(Tick tick)
    {
      _barInProgress.High = Max(_barInProgress.High, tick.Price);
      _barInProgress.Low = Min(_barInProgress.Low, tick.Price);
      _barInProgress.Close = tick.Price;
      _barInProgress.Volume += tick.Volume;
      _barInProgress.TimeStamp = tick.TimeStamp.AddTicks(-1);
      _barInProgress.TickCount++;
    }
  }
}
