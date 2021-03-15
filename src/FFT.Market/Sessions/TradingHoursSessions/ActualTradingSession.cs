// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Sessions.TradingHoursSessions
{
  using System;
  using FFT.TimeStamps;

  /// <summary>
  /// Represents an actual trading session, with exact start, end, and
  /// settlement times, in the same timezone as the instrument's exchange
  /// timezone.
  /// </summary>
  public sealed record ActualTradingSession : ISession
  {
    /// <summary>
    /// The original session template that was used to generate this
    /// ActualTradingSession.
    /// </summary>
    public TradingSessionTemplate SessionTemplate { get; init; }

    /// <summary>
    /// The timezone that is used to generate the trading hours. It can be
    /// different from the instrument's exchange timezone.
    /// </summary>
    public TimeZoneInfo TimeZone { get; init; }

    /// <summary>
    /// This is the trading day to which the session belongs. Multiple trading
    /// sessions, eg, morning and evening, can be present on the same exchange
    /// day, though only one of them can have a non-null settlement time.
    /// </summary>
    public DateStamp SessionDate { get; init; }

    /// <summary>
    /// The start of the trading session.
    /// </summary>
    public TimeStamp SessionStart { get; init; }

    /// <summary>
    /// The end of the trading session.
    /// </summary>
    public TimeStamp SessionEnd { get; init; }

    /// <summary>
    /// Returns true if this actual trading session is in a different week than
    /// the 'previous' trading session.
    /// </summary>
    public bool IsNewWeek(ActualTradingSession previous)
    {
      return SessionDate.ToWeekFloor() != previous.SessionDate.ToWeekFloor();
    }

    /// <summary>
    /// Returns true if this actual trading session is in a different exchange
    /// day than the 'previous' trading session.
    /// </summary>
    public bool IsNewExchangeDay(ActualTradingSession previous)
    {
      return SessionDate != previous.SessionDate;
    }
  }
}
