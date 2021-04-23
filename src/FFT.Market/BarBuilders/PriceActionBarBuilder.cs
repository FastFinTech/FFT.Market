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

  public class PriceActionBarBuilder : BarBuilder
  {
    private readonly PriceActionPeriod _period;
    private readonly double _initialTrendBarSizeInPoints;
    private readonly double _initialReversalBarSizeInPoints;

    private IBar _barInProgress;
    private double _trendBarSizeInPoints;
    private double _reversalBarSizeInPoints;
    private double _currentBarMaxHigh;
    private double _currentBarMinLow;
    private double _nextOpenUp;
    private double _nextOpenDown;
    private double _nextBarMaxHigh;
    private double _nextBarMinLow;
    private Direction _trend = Direction.Up;

    public PriceActionBarBuilder(BarsInfo barsInfo)
      : base(barsInfo)
    {
      _period = (barsInfo.Period as PriceActionPeriod) ?? throw new ArgumentException("period");
      _initialTrendBarSizeInPoints = _trendBarSizeInPoints = barsInfo.Instrument.IncrementsToPoints(_period.TrendBarSizeInTicks);
      _initialReversalBarSizeInPoints = _reversalBarSizeInPoints = barsInfo.Instrument.IncrementsToPoints(_period.ReversalBarSizeInTicks);
    }

    protected override void BarBuilderOnTick(Tick tick)
    {
      if (_barInProgress is null || SessionIterator.IsFirstTickOfSession)
      {
        // It's important to reset ALL calculation variables on a new session,
        // to prevent data sync issues when bars series are built from different
        // start dates.
        _trend = Direction.Up;
        StartNewBar(tick, tick.Price);
      }
      else if (tick.Price > _currentBarMaxHigh)
      {
        CloseBarAtMaxHigh();
        _trend = Direction.Up;

        if (tick.Price > _nextBarMaxHigh)
        {
          StartNewBar(tick, tick.Price);
        }
        else
        {
          StartNewBar(tick, _nextOpenUp);
        }
      }
      else if (tick.Price < _currentBarMinLow)
      {
        CloseBarAtMinLow();
        _trend = Direction.Down;

        if (tick.Price < _nextBarMinLow)
        {
          StartNewBar(tick, tick.Price);
        }
        else
        {
          StartNewBar(tick, _nextOpenDown);
        }
      }
      else
      {
        UpdateBar(tick);
      }
    }

    private void StartNewBar(Tick tick, double openPrice)
    {
      if (_period.AutoAdjustSize && Bars.Count >= 3)
      {
        var increaseBarSize =
          _trendBarSizeInPoints <= _initialTrendBarSizeInPoints &&
          ((Bars[^1].IsUp && Bars[^2].IsDown && Bars[^3].IsUp) || (Bars[^1].IsDown && Bars[^2].IsUp && Bars[^3].IsDown));

        if (increaseBarSize)
        {
          _trendBarSizeInPoints = BarsInfo.Instrument.RoundPrice(_trendBarSizeInPoints * 1.5);
          _reversalBarSizeInPoints = BarsInfo.Instrument.RoundPrice(_reversalBarSizeInPoints * 1.5);
        }
        else
        {
          var decreaseBarSize =
            _trendBarSizeInPoints >= _initialTrendBarSizeInPoints &&
            Bars[^1].TimeStamp.Subtract(Bars[^2].TimeStamp).TotalMinutes > 1;

          if (decreaseBarSize)
          {
            _trendBarSizeInPoints = BarsInfo.Instrument.RoundPrice(_trendBarSizeInPoints / 1.5);
            _reversalBarSizeInPoints = BarsInfo.Instrument.RoundPrice(_reversalBarSizeInPoints / 1.5);
          }
        }
      }

      if (_trend.IsUp)
      {
        _currentBarMaxHigh = BarsInfo.Instrument.RoundPrice(openPrice + _trendBarSizeInPoints);
        _currentBarMinLow = BarsInfo.Instrument.RoundPrice(openPrice - _reversalBarSizeInPoints);
      }
      else
      {
        _currentBarMaxHigh = BarsInfo.Instrument.RoundPrice(openPrice + _reversalBarSizeInPoints);
        _currentBarMinLow = BarsInfo.Instrument.RoundPrice(openPrice - _trendBarSizeInPoints);
      }

      _nextOpenUp = BarsInfo.Instrument.AddIncrements(_currentBarMaxHigh, 1);
      _nextOpenDown = BarsInfo.Instrument.AddIncrements(_currentBarMinLow, -1);
      _nextBarMaxHigh = BarsInfo.Instrument.RoundPrice(_nextOpenUp + _trendBarSizeInPoints);
      _nextBarMinLow = BarsInfo.Instrument.RoundPrice(_nextOpenDown - _trendBarSizeInPoints);

      _barInProgress = new Bar();
      _barInProgress.Open = openPrice;
      _barInProgress.High = Max(openPrice, tick.Price);
      _barInProgress.Low = Min(openPrice, tick.Price);
      _barInProgress.Close = tick.Price;
      _barInProgress.Volume = tick.Volume;
      _barInProgress.TimeStamp = tick.TimeStamp;
      Bars.AddNewBar(_barInProgress);
    }

    private void UpdateBar(Tick tick)
    {
      _barInProgress.High = Max(_barInProgress.High, tick.Price);
      _barInProgress.Low = Min(_barInProgress.Low, tick.Price);
      _barInProgress.Close = tick.Price;
      _barInProgress.Volume += tick.Volume;
      _barInProgress.TimeStamp = tick.TimeStamp;
      _barInProgress.TickCount++;
    }

    private void CloseBarAtMaxHigh()
      => _barInProgress.High = _barInProgress.Close = _currentBarMaxHigh;

    private void CloseBarAtMinLow()
      => _barInProgress.Low = _barInProgress.Close = _currentBarMinLow;
  }
}
