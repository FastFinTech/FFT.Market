// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Sessions.TradingHoursSessions
{
  using System;
  using FFT.TimeStamps;

  /// <summary>
  /// Represents a partial exchange holiday, whereby the exchange opens late or
  /// closes early. When this happens, some trading sessions are modified or
  /// canceled.
  /// </summary>
  public sealed record PartialHoliday
  {

    /// <summary>
    /// The date of the partial holiday.
    /// </summary>
    public DateStamp SessionDate { get; init; }

    /// <summary>
    /// Whether the partial holiday is a late open or an early close.
    /// </summary>
    public PartialHolidayType Type { get; init; }

    /// <summary>
    /// The time of the late open or early close, expressed as minutes since
    /// 00:00 Sunday, in the same timezone as the instrument's exchange.
    /// </summary>
    public TimeOfWeek TimeOfWeek { get; init; }

    /// <summary>
    /// The name of the exchange partial holiday, eg, "Christmas Day".
    /// </summary>
    public string Name { get; init; }
  }
}
