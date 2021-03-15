// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Sessions.TradingHoursSessions
{
  public enum PartialHolidayType
  {
    /// <summary>
    /// The exchange opens later than usual, cancelling or modifying the regular
    /// trading sessions.
    /// </summary>
    LateStart,

    /// <summary>
    /// The exchange closes earlier than usual, cancelling or modifying the
    /// regular trading sessions.
    /// </summary>
    EarlyEnd,
  }
}
