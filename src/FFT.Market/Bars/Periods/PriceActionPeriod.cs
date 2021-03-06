// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Bars.Periods
{
  public sealed record PriceActionPeriod : IPeriod
  {
    public static PriceActionPeriod Default { get; } = new PriceActionPeriod
    {
      TrendBarSizeInTicks = 12,
      ReversalBarSizeInTicks = 12,
    };

    public string Name => "PriceAction";
    public bool IsEvenTimeSpacingBars => false;
    public int TrendBarSizeInTicks { get; init; }
    public int ReversalBarSizeInTicks { get; init; }

    public override string ToString()
      => $"{TrendBarSizeInTicks}/{ReversalBarSizeInTicks}-PriceAction";

    public IPeriod Multiply(double value)
      => this with
      {
        TrendBarSizeInTicks = (int)(TrendBarSizeInTicks * value),
        ReversalBarSizeInTicks = (int)(ReversalBarSizeInTicks * value),
      };
  }
}
