// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Sessions.TradingHoursSessions
{
  using System;
  using FFT.TimeStamps;

  /// <summary>
  /// Represents the template used to describe trading sessions. Used by the
  /// TradingHours component to create "ActualSession" objects which have exact
  /// start and end timestamps. All times are expressed according to the
  /// TimeZone of the instrument's exchange.
  /// </summary>
  public sealed record TradingSessionTemplate
  {
    /// <summary>
    /// This is the trading day to which the session belongs. Multiple sessions,
    /// eg, morning and evening, can be present on the same exchange day, though
    /// only one of them can have a non-null settlement time.
    /// </summary>
    public DayOfWeek ExchangeDay { get; init; }

    /// <summary>
    /// The start of the session with respect to the start of the week, Sunday
    /// 00:00.
    /// </summary>
    public TimeOfWeek Start { get; init; }

    /// <summary>
    /// The end of the session with respect to the start of the week, Sunday
    /// 00:00.
    /// </summary>
    public TimeOfWeek End { get; init; }

    /// <summary>
    /// Use this method to throw a ValidationException if the properties have
    /// been setup in an illogical way.
    /// </summary>
    public void Validate()
    {
      if (End <= Start)
      {
        throw new ValidationException("Session end must be zero (end of week) or after session start.");
      }

      /// / TODO: Look at putting this back in, but making it work with the
      /// chinese session templates that trigger this error
      // var rangeMin = new TimeOfWeek(ExchangeDay, TimeSpan.Zero); var rangeMax
      // = rangeMin.Add(TimeSpan.FromDays(1)); if (rangeMax.TicksSinceWeekFloor
      // == 0) rangeMax = TimeOfWeek.EndOfWeek; if (End < rangeMin) throw new
      // ValidationException("At least part of the session must be during the
      // given 'ExchangeDay'"); if (Start > rangeMax) throw new
      // ValidationException("At least part of the session must be during the
      // given 'ExchangeDay'");
    }

    /// <summary>
    /// Use this method to throw a ValidationException if the properties of this
    /// object are not valid for a session after the "previous" session.
    /// </summary>
    public void Validate(TradingSessionTemplate previous)
    {
      if (Start < previous.End)
        throw new ValidationException("Sessions must begin at or after the previous session has ended.");

      if (ExchangeDay < previous.ExchangeDay)
        throw new ValidationException("Sessions 'ExchangeDay' property must be in ascending order.");
    }
  }
}
