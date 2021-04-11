// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Bars
{
  using System;
  using FFT.TimeStamps;

  public class Bar : IBar
  {
    public Bar()
    {
      TickCount = 1;
    }

    public Bar(Bar other)
    {
      Open = other.Open;
      High = other.High;
      Low = other.Low;
      Close = other.Close;
      Volume = other.Volume;
      TimeStamp = other.TimeStamp;
      TickCount = other.TickCount;
    }

    public double Open { get; set; }
    public double High { get; set; }
    public double Low { get; set; }
    public double Close { get; set; }
    public double Volume { get; set; }
    public TimeStamp TimeStamp { get; set; }
    public int TickCount { get; set; }

    public double GetValue(BarInputType inputType)
    {
      switch (inputType)
      {
        case BarInputType.Close: return Close;
        case BarInputType.High: return High;
        case BarInputType.Low: return Low;
        case BarInputType.Open: return Open;
        case BarInputType.Volume: return Volume;
        default: throw new NotImplementedException();
      }
    }

    public void CumulateNextBar(Bar bar)
    {
      High = Math.Max(High, bar.High);
      Low = Math.Min(Low, bar.Low);
      Close = bar.Close;
      Volume += bar.Volume;
      TickCount += bar.TickCount;
      TimeStamp = bar.TimeStamp;
    }
  }
}
