// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.BarBuilders
{
  using System;
  using FFT.Market.Bars;
  using FFT.Market.Ticks;
  using static System.Math;

  public class DiagnosticBarBuilder : BarBuilder
  {
    private readonly DiagnosticPeriod _period;
    private readonly double _tickSize;
    private readonly double _trendBarSizeInPoints;
    private readonly double _reversalBarSizeInPoints;

    private IBar _barInProgress;
    private double _currentBarMaxHigh;
    private double _currentBarMinLow;
    private double _nextOpenUp;
    private double _nextOpenDown;
    private double _nextBarMaxHigh;
    private double _nextBarMinLow;
    private Direction _trend = Direction.Up;

    public DiagnosticBarBuilder(BarsInfo barsInfo)
      : base(barsInfo)
    {
      _period = (barsInfo.Period as DiagnosticPeriod) ?? throw new ArgumentException("period");
      _tickSize = barsInfo.Instrument.TickSize;
      _trendBarSizeInPoints = barsInfo.Instrument.TicksToPoints(_period.TrendBarSizeInTicks);
      _reversalBarSizeInPoints = barsInfo.Instrument.TicksToPoints(_period.ReversalBarSizeInTicks);
    }

    protected override void BarBuilderOnTick(Tick tick)
    {
      if (_barInProgress is null || SessionIterator.IsFirstTickOfSession)
      {
        // It's important to reset ALL calculation variables on a new session,
        // to prevent data sync issues when bars series are built from different start dates.
        _trend = Direction.Up;
        StartNewBar(tick, tick.Price);
      }
      else if (tick.Price > _currentBarMaxHigh)
      {
        CloseBarAtMaxHigh();
        if (tick.Price > _nextBarMaxHigh)
        {
          StartNewBar(tick, tick.Price);
        }
        else
        {
          StartNewBar(tick, _nextOpenUp);
        }

        _trend = Direction.Up;
      }
      else if (tick.Price < _currentBarMinLow)
      {
        CloseBarAtMinLow();
        if (tick.Price < _nextBarMinLow)
        {
          StartNewBar(tick, tick.Price);
        }
        else
        {
          StartNewBar(tick, _nextOpenDown);
        }

        _trend = Direction.Down;
      }
      else
      {
        UpdateBar(tick);
      }
    }

    private void StartNewBar(Tick tick, double openPrice)
    {
      _barInProgress = new Bar();
      _barInProgress.Open = openPrice;
      _barInProgress.High = Max(openPrice, tick.Price);
      _barInProgress.Low = Min(openPrice, tick.Price);
      _barInProgress.Close = tick.Price;
      _barInProgress.Volume = tick.Volume;
      _barInProgress.TimeStamp = tick.TimeStamp;
      SetCalculationVariables(openPrice);
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

    private void SetCalculationVariables(double currentBarOpenPrice)
    {
      if (_trend.IsUp)
      {
        _currentBarMaxHigh = (currentBarOpenPrice + _trendBarSizeInPoints).RoundToTick(_tickSize);
        _currentBarMinLow = (currentBarOpenPrice - _reversalBarSizeInPoints).RoundToTick(_tickSize);
      }
      else
      {
        _currentBarMaxHigh = (currentBarOpenPrice + _reversalBarSizeInPoints).RoundToTick(_tickSize);
        _currentBarMinLow = (currentBarOpenPrice - _trendBarSizeInPoints).RoundToTick(_tickSize);
      }

      _nextOpenUp = _currentBarMaxHigh.AddTicks(_tickSize, 1);
      _nextOpenDown = _currentBarMinLow.AddTicks(_tickSize, -1);
      _nextBarMaxHigh = (_nextOpenUp + _trendBarSizeInPoints).RoundToTick(_tickSize);
      _nextBarMinLow = (_nextOpenDown - _trendBarSizeInPoints).RoundToTick(_tickSize);
    }

    private void CloseBarAtMaxHigh()
      => _barInProgress.High = _barInProgress.Close = _currentBarMaxHigh;

    private void CloseBarAtMinLow()
      => _barInProgress.Low = _barInProgress.Close = _currentBarMinLow;
  }
}
