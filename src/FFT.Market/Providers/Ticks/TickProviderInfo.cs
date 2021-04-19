// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Providers.Ticks
{
  using FFT.Market.Instruments;
  using FFT.TimeStamps;

  /// <summary>
  /// Uniquely-describes the tick data information provided by a <see
  /// cref="ITickProvider"/>.
  /// </summary>
  public sealed record TickProviderInfo
  {
    /// <summary>
    /// The instrument for which tick data will be provided.
    /// </summary>
    public IInstrument Instrument { get; init; }

    /// <summary>
    /// The timestamp of the first tick will be GREATER than this value. This
    /// value is NOT inclusive.
    /// </summary>
    public TimeStamp From { get; init; }

    /// <summary>
    /// The timestamp of the last tick will be less than or equal to this value.
    /// When this value is null, live market data ticks will be provided as they
    /// become available.
    /// </summary>
    public TimeStamp? Until { get; init; }
  }
}
