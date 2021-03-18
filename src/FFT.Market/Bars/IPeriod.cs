// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Bars
{
  using static System.Math;

  public interface IPeriod
  {
    string Name { get; }

    bool IsEvenTimeSpacingBars { get; }

    bool IsUnevenTimeSpacingBars => !IsEvenTimeSpacingBars;

    IPeriod Multiply(double value);
  }

  public sealed record SecondPeriod : IPeriod
  {
    public static SecondPeriod Default { get; } = new SecondPeriod { PeriodInSeconds = 30 };

    public string Name => "Second";
    public bool IsEvenTimeSpacingBars => true;
    public int PeriodInSeconds { get; init; }
    public override string ToString() => $"{PeriodInSeconds}-Second";
    public IPeriod Multiply(double value) => new SecondPeriod { PeriodInSeconds = Max(1, (int)(PeriodInSeconds * value)) };
  }

  public sealed record MinutePeriod : IPeriod
  {
    public static MinutePeriod Default { get; } = new MinutePeriod { PeriodInMinutes = 10 };
    public string Name => "Minute";
    public bool IsEvenTimeSpacingBars => true;
    public int PeriodInMinutes { get; init; }
    public override string ToString() => $"{PeriodInMinutes}-Minute";
    public IPeriod Multiply(double value) => new MinutePeriod { PeriodInMinutes = Max(1, (int)(PeriodInMinutes * value)) };
  }

  public sealed record TickPeriod : IPeriod
  {
    public static TickPeriod Default { get; } = new TickPeriod { TicksPerBar = 100 };
    public string Name => "Tick";
    public bool IsEvenTimeSpacingBars => false;
    public int TicksPerBar { get; init; }
    public override string ToString() => $"{TicksPerBar}-Tick";
    public IPeriod Multiply(double value) => new TickPeriod { TicksPerBar = Max(1, (int)(TicksPerBar * value)) };
  }

  public sealed record RangePeriod : IPeriod
  {
    public static RangePeriod Default { get; } = new RangePeriod { TicksPerBar = 4 };
    public string Name => "Range";
    public bool IsEvenTimeSpacingBars => false;
    public int TicksPerBar { get; init; }
    public override string ToString() => $"{TicksPerBar}-Range";
    public IPeriod Multiply(double value) => new RangePeriod { TicksPerBar = Max(1, (int)(TicksPerBar * value)) };
  }

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
      => new PriceActionPeriod
      {
        TrendBarSizeInTicks = (int)(TrendBarSizeInTicks * value),
        ReversalBarSizeInTicks = (int)(ReversalBarSizeInTicks * value),
      };
  }
}
