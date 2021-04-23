// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Bars.Periods
{
  using static System.Math;

  public sealed record RangePeriod : IPeriod
  {
    public static RangePeriod Default { get; } = new RangePeriod { TicksPerBar = 4 };
    public string Name => "Range";
    public bool IsEvenTimeSpacingBars => false;
    public int TicksPerBar { get; init; }
    public override string ToString() => $"{TicksPerBar}-Range";
    public IPeriod Multiply(double value) => this with
    {
      TicksPerBar = Max(1, (int)(TicksPerBar * value))
    };
  }
}
