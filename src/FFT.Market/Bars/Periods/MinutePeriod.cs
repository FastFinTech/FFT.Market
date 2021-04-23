// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Bars.Periods
{
  using static System.Math;

  public sealed record MinutePeriod : IPeriod
  {
    public static MinutePeriod Default { get; } = new MinutePeriod { PeriodInMinutes = 10 };
    public string Name => "Minute";
    public bool IsEvenTimeSpacingBars => true;
    public int PeriodInMinutes { get; init; }
    public override string ToString() => $"{PeriodInMinutes}-Minute";
    public IPeriod Multiply(double value) => this with
    {
      PeriodInMinutes = Max(1, (int)(PeriodInMinutes * value))
    };
  }
}
