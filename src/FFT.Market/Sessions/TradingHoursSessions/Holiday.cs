// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Sessions.TradingHoursSessions
{
  using FFT.TimeStamps;

  /// <summary>
  /// Represents an exchange holiday. All trading sessions for the given
  /// ExchangeDate are closed.
  /// </summary>
  public sealed record Holiday
  {
    /// <summary>
    /// The date of the exchange holiday, eg, 25 December 2016.
    /// </summary>
    public DateStamp SessionDate { get; init; }

    /// <summary>
    /// The name of the exchange holiday, eg, "Christmas Day".
    /// </summary>
    public string Name { get; init; }
  }
}
