// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Bars
{
  using FFT.Market.BarBuilders;
  using FFT.Market.Bars.Periods;
  using FFT.Market.Instruments;
  using FFT.Market.Sessions.TradingHoursSessions;
  using FFT.TimeStamps;

  /// <summary>
  /// Use this object to completely define all the properties of a bars object.
  /// When we encapsulate all the properties in one object, we can use it as a
  /// key to retrieve bars from a repository
  /// </summary>
  /// <remarks>Inheriting from MemberwiseEqualityObject means that we don't need
  /// to explicitly type out the Equals and GetHashCode methods for each of the
  /// different settings objects, so long as all important properties are
  /// actually public properties</remarks>
  public sealed record BarsInfo
  {
    /// <summary>
    /// The instrument that the bars represent.
    /// </summary>
    public IInstrument Instrument { get; init; }

    /// <summary>
    /// The bars period.
    /// </summary>
    public IPeriod Period { get; init; }

    /// <summary>
    /// The trading hours template that the bars are being built from. Ticks
    /// outside the trading hours template are excluded by the bars builder.
    /// </summary>
    /// <remarks>Use of this property is NOT implemented by most bar builders,
    /// as we use Default 24/7 for all scenarios so far</remarks>
    public TradingSessions TradingHours { get; init; }

    /// <summary>
    /// The exact starting timestamp of the bars series.
    /// </summary>
    public DateStamp FirstSessionDate { get; init; }

    /// <summary>
    /// A utility method to create a bar builder for this bars info object.
    /// </summary>
    public BarBuilder CreateBuilder()
      => BarBuilder.Create(this);
  }
}
