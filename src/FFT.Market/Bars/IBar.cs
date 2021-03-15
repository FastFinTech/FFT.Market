// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Bars
{
  using FFT.TimeStamps;

  public interface IBar
  {
    double Open { get; set; }
    double High { get; set; }
    double Low { get; set; }
    double Close { get; set; }
    double Volume { get; set; }
    TimeStamp TimeStamp { get; set; }
    int TickCount { get; set; }
    double GetValue(BarInputType inputType);
    IBar Clone();
  }
}
