// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Bars.Periods
{
  using static System.Math;

  public sealed record TickPeriod : IPeriod
  {
    public static TickPeriod Default { get; } = new TickPeriod { TicksPerBar = 100 };
    public string Name => "Tick";
    public bool IsEvenTimeSpacingBars => false;
    public int TicksPerBar { get; init; }
    public override string ToString() => $"{TicksPerBar}-Tick";
    public IPeriod Multiply(double value) => this with
    {
      TicksPerBar = Max(1, (int)(TicksPerBar * value))
    };
  }
}
