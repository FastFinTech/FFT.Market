// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Bars
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using FFT.Market.Bars.Periods;
  using FFT.Market.DataSeries;
  using FFT.Market.Instruments;
  using FFT.Market.TickStreams;
  using FFT.TimeStamps;

  [DebuggerNonUserCode]
  public class Bars : IBars
  {
    private readonly List<IBar> _bars = new List<IBar>();

    public Bars(BarsInfo barsInfo)
    {
      BarsInfo = barsInfo;
      OpenValueSeries = new BarBasedValueSeries(this, BarInputType.Open);
      HighValueSeries = new BarBasedValueSeries(this, BarInputType.High);
      LowValueSeries = new BarBasedValueSeries(this, BarInputType.Low);
      CloseValueSeries = new BarBasedValueSeries(this, BarInputType.Close);
      VolumeValueSeries = new BarBasedValueSeries(this, BarInputType.Volume);
    }

    public BarsInfo BarsInfo { get; }
    public IReadOnlyValueSeries<double> OpenValueSeries { get; }
    public IReadOnlyValueSeries<double> HighValueSeries { get; }
    public IReadOnlyValueSeries<double> LowValueSeries { get; }
    public IReadOnlyValueSeries<double> CloseValueSeries { get; }
    public IReadOnlyValueSeries<double> VolumeValueSeries { get; }

    public int Count => _bars.Count;
    public IPeriod Period => BarsInfo.Period;
    public IInstrument Instrument => BarsInfo.Instrument;

    public IBar this[Index index] => _bars[index];

    public IEnumerable<object> GetDependencies()
    {
      yield return BarsInfo.Instrument;
    }

    public double GetOpen(Index index) => _bars[index].Open;
    public double GetHigh(Index index) => _bars[index].High;
    public double GetLow(Index index) => _bars[index].Low;
    public double GetClose(Index index) => _bars[index].Close;
    public double GetVolume(Index index) => _bars[index].Volume;
    public Index GetTickCount(Index index) => _bars[index].TickCount;
    public TimeStamp GetTimeStamp(Index index) => _bars[index].TimeStamp;
    public TimeStamp GetTimeStamp(int index) => _bars[index].TimeStamp;
    public double GetValue(BarInputType inputType, Index index) => _bars[index].GetValue(inputType);


    public IEnumerable<double> GetValues(BarInputType inputType)
      => _bars.Select(b => b.GetValue(inputType));

    public IReadOnlyValueSeries<double> GetValueSeries(BarInputType inputType)
    {
      switch (inputType)
      {
        case BarInputType.Open: return OpenValueSeries;
        case BarInputType.High: return HighValueSeries;
        case BarInputType.Low: return LowValueSeries;
        case BarInputType.Close: return CloseValueSeries;
        case BarInputType.Volume: return VolumeValueSeries;
        default: throw new NotImplementedException();
      }
    }

    public void AddNewBar(IBar newBar)
      => _bars.Add(newBar);

    public void UpdateLastBar(double open, double high, double low, double close, double volume, TimeStamp timeStamp)
    {
      var bar = _bars[_bars.Count - 1];
      bar.Open = open;
      bar.High = high;
      bar.Low = low;
      bar.Close = close;
      bar.Volume = volume;
      bar.TimeStamp = timeStamp;
    }

    public void Trim()
      => _bars.Capacity = _bars.Count;

    public IEnumerator<IBar> GetEnumerator()
      => _bars.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      => _bars.GetEnumerator();
  }
}
