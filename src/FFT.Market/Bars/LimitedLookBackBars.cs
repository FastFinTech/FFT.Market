// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Bars
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using FFT.Market.DataSeries;
  using FFT.Market.Instruments;
  using FFT.Market.TickStreams;
  using FFT.TimeStamps;

  public class LimitedLookBackBars : IBars
  {
    private readonly List<IBar> _bars = new List<IBar>();

    public LimitedLookBackBars(BarsInfo barsInfo)
    {
      BarsInfo = barsInfo;
      OpenValueSeries = new BarBasedValueSeries(this, BarInputType.Open);
      HighValueSeries = new BarBasedValueSeries(this, BarInputType.High);
      LowValueSeries = new BarBasedValueSeries(this, BarInputType.Low);
      CloseValueSeries = new BarBasedValueSeries(this, BarInputType.Close);
      VolumeValueSeries = new BarBasedValueSeries(this, BarInputType.Volume);
    }

    public int MaxLookBack { get; private set; } = 256;
    public int NumRemoved { get; private set; }

    public BarsInfo BarsInfo { get; }
    public IReadOnlyValueSeries<double> OpenValueSeries { get; }
    public IReadOnlyValueSeries<double> HighValueSeries { get; }
    public IReadOnlyValueSeries<double> LowValueSeries { get; }
    public IReadOnlyValueSeries<double> CloseValueSeries { get; }
    public IReadOnlyValueSeries<double> VolumeValueSeries { get; }

    public int Count => _bars.Count + NumRemoved;
    public IPeriod Period => BarsInfo.Period;
    public IInstrument Instrument => BarsInfo.Instrument;

    public IBar this[Index index]
      => index.IsFromEnd
        ? _bars[index]
        : _bars[index.Value - NumRemoved];

    public void IncreaseMaxLookBack(int maxLookBack)
    {
      if (maxLookBack > MaxLookBack)
      {
        MaxLookBack = maxLookBack;
      }
    }

    public IEnumerable<object> GetDependencies()
    {
      yield return BarsInfo.Instrument;
    }

    public double GetOpen(int index) => _bars[index - NumRemoved].Open;
    public double GetHigh(int index) => _bars[index - NumRemoved].High;
    public double GetLow(int index) => _bars[index - NumRemoved].Low;
    public double GetClose(int index) => _bars[index - NumRemoved].Close;
    public double GetVolume(int index) => _bars[index - NumRemoved].Volume;
    public int GetTickCount(int index) => _bars[index - NumRemoved].TickCount;
    public TimeStamp GetTimeStamp(int index) => _bars[index - NumRemoved].TimeStamp;
    public double GetValue(BarInputType inputType, int index) => _bars[index - NumRemoved].GetValue(inputType);

    public IEnumerable<double> GetValues(BarInputType inputType) => _bars.Select(b => b.GetValue(inputType));

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
    {
      _bars.Add(newBar);
      if (_bars.Count > MaxLookBack)
      {
        _bars.RemoveAt(0);
        NumRemoved++;
      }
    }

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
