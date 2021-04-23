// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Bars.Periods
{
  using static System.Math;

  public sealed record SecondPeriod : IPeriod
  {
    public static SecondPeriod Default { get; } = new SecondPeriod { PeriodInSeconds = 30 };

    public string Name => "Second";
    public bool IsEvenTimeSpacingBars => true;
    public int PeriodInSeconds { get; init; }
    public override string ToString() => $"{PeriodInSeconds}-Second";
    public IPeriod Multiply(double value) => this with
    {
      PeriodInSeconds = Max(1, (int)(PeriodInSeconds * value))
    };
  }
}
