// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Services
{
  using System;
  using FFT.TimeStamps;

  public interface ITradingPlatformTime
  {
    /// <summary>
    /// Gets the "now" time of the trading platform. If the trading platform is
    /// is "market replay" mode, then this value will NOT be the same as the
    /// current clock time.
    /// </summary>
    TimeStamp Now { get; }

    /// <summary>
    /// Gets the timezone that the trading platform is configured to display
    /// dates and times in.
    /// </summary>
    TimeZoneInfo TimeZone { get; }
  }
}
