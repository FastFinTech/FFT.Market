// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.BarBuilders
{
  using System;
  using FFT.Market.Bars;
  using FFT.Market.Ticks;

  public class RangeBarBuilder : BarBuilder
  {
    private readonly RangePeriod _period;
    private readonly double _tickSize;
    private readonly double _rangeInPoints;

    private IBar _barInProgress;

    public RangeBarBuilder(BarsInfo info)
      : base(info)
    {
      _period = (info.Period as RangePeriod) ?? throw new ArgumentException("period");
      _tickSize = info.Instrument.TickSize;
      _rangeInPoints = info.Instrument.TicksToPoints(_period.TicksPerBar);
    }

    private double MaxHigh => ToTick(_barInProgress.Low + _rangeInPoints);

    private double MinLow => ToTick(_barInProgress.High - _rangeInPoints);

    private double OpenOfNextBarUp => ToTick(_barInProgress.Low + _rangeInPoints + _tickSize);

    private double MaxHighOfNextBarUp => ToTick(_barInProgress.Low + _rangeInPoints + _rangeInPoints + _tickSize);

    private double OpenOfNextBarDown => ToTick(_barInProgress.High - _rangeInPoints - _tickSize);

    private double MinLowOfNextBarDown => ToTick(_barInProgress.High - _rangeInPoints - _rangeInPoints - _tickSize);

    protected override void BarBuilderOnTick(Tick tick)
    {
      if (_barInProgress is null || SessionIterator.IsFirstTickOfSession)
      {
        StartNewBar(tick, tick.Price);
      }
      else if (tick.Price > MaxHigh)
      {
        CloseAtMaxHigh();
        if (tick.Price > MaxHighOfNextBarUp)
        {
          StartNewBar(tick, tick.Price);
        }
        else
        {
          StartNewBar(tick, OpenOfNextBarUp);
        }
      }
      else if (tick.Price < MinLow)
      {
        CloseAtMinLow();
        if (tick.Price < MinLowOfNextBarDown)
        {
          StartNewBar(tick, tick.Price);
        }
        else
        {
          StartNewBar(tick, OpenOfNextBarDown);
        }
      }
      else
      {
        UpdateBar(tick);
      }
    }

    private void StartNewBar(Tick tick, double open)
    {
      _barInProgress = new Bar();
      _barInProgress.Open = open;
      _barInProgress.Close = tick.Price;
      _barInProgress.High = Math.Max(open, tick.Price);
      _barInProgress.Low = Math.Min(open, tick.Price);
      _barInProgress.Volume = tick.Volume;
      _barInProgress.TimeStamp = tick.TimeStamp;
      Bars.AddNewBar(_barInProgress);
    }

    private void UpdateBar(Tick tick)
    {
      _barInProgress.High = Math.Max(_barInProgress.High, tick.Price);
      _barInProgress.Low = Math.Min(_barInProgress.Low, tick.Price);
      _barInProgress.Close = tick.Price;
      _barInProgress.Volume += tick.Volume;
      _barInProgress.TimeStamp = tick.TimeStamp;
      _barInProgress.TickCount++;
    }

    private void CloseAtMaxHigh()
    {
      _barInProgress.High = _barInProgress.Close = MaxHigh;
    }

    private void CloseAtMinLow()
    {
      _barInProgress.Low = _barInProgress.Close = MinLow;
    }
  }
}
