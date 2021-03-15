// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Sessions.TradingHoursSessions
{
  using System;
  using System.Collections.Generic;
  using System.Collections.Immutable;
  using System.Linq;
  using FFT.TimeStamps;

  /// <summary>
  /// Use this class to generate ActualTradingSession objects and figure out
  /// exactly when an instrument is open for trading, when it has settlement
  /// times, and when session breaks are. It has been decided that in the Tough
  /// framework, there will always be at least one session break per week.
  /// Therefore, it's impossible to create a "TradingSessions" object with 0
  /// SessionTemplates. If you want to represent a continual week template, you
  /// need to add a TradingSession running from 0 to 7*1440.
  /// </summary>
  public sealed record TradingSessions
  {
    public static TradingSessions Create24x7(TimeZoneInfo timeZone)
    {
      return new TradingSessions
      {
        Name = $"24 x 7 {timeZone.DisplayName}",
        Holidays = ImmutableArray<Holiday>.Empty,
        PartialHolidays = ImmutableArray<PartialHoliday>.Empty,
        TimeZone = timeZone,
        SessionTemplates = Enumerable.Range(0, 7).Select(i => new TradingSessionTemplate
        {
          Start = new TimeOfWeek(i * TimeSpan.TicksPerDay),
          End = new TimeOfWeek((i + 1) * TimeSpan.TicksPerDay),
          ExchangeDay = (DayOfWeek)i,
        }).ToImmutableArray(),
      };
    }

    public string Name { get; init; }

    /// <summary>
    /// Ordered array of session templates to describe each trading session in a
    /// week. Sessions cannot overlap, and they must be ordered correctly. There
    /// can only be one session template with a settlement time for each
    /// ExchangeDay.
    /// </summary>
    public ImmutableArray<TradingSessionTemplate> SessionTemplates { get; init; }

    /// <summary>
    /// All the full-day trading holidays for this instrument.
    /// </summary>
    public ImmutableArray<Holiday> Holidays { get; init; }

    /// <summary>
    /// All the partial-day trading holidays for this instrument.
    /// </summary>
    public ImmutableArray<PartialHoliday> PartialHolidays { get; init; }

    /// <summary>
    /// The timezone of the trading sessions.
    /// </summary>
    public TimeZoneInfo TimeZone { get; init; }

    public bool Equals(TradingSessions? other)
    {
      if (ReferenceEquals(this, other)) return true;
      if (other is null) return false;
      return EqualityComparer<string>.Default.Equals(Name, other.Name)
        && EnumerableEqualityComparer<TradingSessionTemplate>.Default.Equals(SessionTemplates, other.SessionTemplates)
        && EnumerableEqualityComparer<Holiday>.Default.Equals(Holidays, other.Holidays)
        && EnumerableEqualityComparer<PartialHoliday>.Default.Equals(PartialHolidays, other.PartialHolidays)
        && TimeZone.Id == other.TimeZone.Id;
    }

    public override int GetHashCode()
    {
      HashCode hash = default;
      hash.Add(typeof(TradingSessions));
      hash.Add(SessionTemplates, EnumerableEqualityComparer<TradingSessionTemplate>.Default);
      hash.Add(Holidays, EnumerableEqualityComparer<Holiday>.Default);
      hash.Add(PartialHolidays, EnumerableEqualityComparer<PartialHoliday>.Default);
      hash.Add(TimeZone.Id);
      return hash.ToHashCode();
    }

    /// <summary>
    /// Get the actual session that is active at the given time.
    /// If no session is active at the given time, the next session will be returned.
    /// </summary>
    public ActualTradingSession GetActualSessionAt(TimeStamp at)
    {
      var startOfWeek = at.GetDate(TimeZone).ToWeekFloor();
      var timeOfWeek = at.GetTimeOfWeek(TimeZone);
      var indexOfSession = 0;

      while (indexOfSession < SessionTemplates.Length && SessionTemplates[indexOfSession].End < timeOfWeek)
      {
        indexOfSession++;
      }

      return CreateActualSession(ref startOfWeek, ref indexOfSession);
    }

    /// <summary>
    /// Creates an ActualSession from the given week and sessionIndex. However,
    /// it's intelligent enough to check for holidays. If a holiday wipes out
    /// the session that would normally be returned, it skips ahead and uses the
    /// next available session instead. If it has to skip ahead to the next
    /// available session, it modifies the startOfWeek and sessionIndex
    /// parameters, so whoever's calling this method can keep track of where
    /// they are at in the week/session.
    /// </summary>
    private ActualTradingSession CreateActualSession(ref DateStamp startOfWeek, ref int sessionIndex)
    {
      if (startOfWeek.DayOfWeek != DayOfWeek.Sunday)
        throw new ArgumentException($"startOfWeek '{startOfWeek}' was not a Sunday.");

      // Skip ahead to the first session of the next week if required
      if (sessionIndex == SessionTemplates.Length)
      {
        sessionIndex = 0;
        startOfWeek = startOfWeek.AddDays(7);
      }

      // Get the session and create an ActualSession from it
      var session = SessionTemplates[sessionIndex];
      var sessionStart = Add(startOfWeek, session.Start);
      var sessionEnd = Add(startOfWeek, session.End);
      var sessionDate = startOfWeek.AddDays((int)session.ExchangeDay);

      // If this session falls on a holiday, skip ahead to the next session
      if (Holidays.Any(h => h.SessionDate == sessionDate))
        goto nextSession;

      // If this session falls on a partial holiday, it may be wiped out
      // completely, or modified.
      var partialHoliday = PartialHolidays.FirstOrDefault(h => h.SessionDate == sessionDate);
      if (partialHoliday is not null)
      {
        if (partialHoliday.Type == PartialHolidayType.LateStart)
        {
          // check if the day's late start completely wipes out this session
          if (partialHoliday.TimeOfWeek >= session.End)
            goto nextSession;

          // check if the day's late start simply modifies the start time of
          // this session
          if (partialHoliday.TimeOfWeek > session.Start)
            sessionStart = Add(startOfWeek, partialHoliday.TimeOfWeek);
        }
        else if (partialHoliday.Type == PartialHolidayType.EarlyEnd)
        {
          // check if the day's early end completely wipes out this session
          if (partialHoliday.TimeOfWeek <= session.Start)
            goto nextSession;

          // check if the day's early end modifieds the end time of this session
          if (partialHoliday.TimeOfWeek < session.End)
            sessionEnd = Add(startOfWeek, partialHoliday.TimeOfWeek);
        }
        else
        {
          throw new NotImplementedException();
        }
      }

      return new ActualTradingSession
      {
        SessionTemplate = session,
        SessionStart = sessionStart,
        SessionEnd = sessionEnd,
        SessionDate = sessionDate,
        TimeZone = TimeZone,
      };

nextSession:
// A holiday or partial holiday has "wiped out" the given session, So we'll skip
// ahead to the next available session.
      sessionIndex++;
      return CreateActualSession(ref startOfWeek, ref sessionIndex);
    }

    private TimeStamp Add(DateStamp startOfWeek, TimeOfWeek timeOfWeek)
        => new TimeStamp(startOfWeek.DateTime.Ticks + timeOfWeek.TicksSinceWeekFloor, TimeZone);

    /// <summary>
    /// Use this method to throw a ValidationException if the properties have
    /// been setup in an illogical way.
    /// </summary>
    public void Validate()
    {
      if (SessionTemplates.Length == 0)
        throw new ValidationException("There must be at least one Session.");

      foreach (var session in SessionTemplates)
        session.Validate();

      for (var i = 1; i < SessionTemplates.Length; i++)
        SessionTemplates[i].Validate(SessionTemplates[i - 1]);

      var partialHolidayDates = PartialHolidays.Select(p => p.SessionDate).Distinct().ToArray();
      var fullHolidayDates = Holidays.Select(h => h.SessionDate).Distinct().ToArray();

      if (partialHolidayDates.Length != PartialHolidays.Length)
        throw new ValidationException("More than one partial holiday for a given date");

      if (fullHolidayDates.Length != Holidays.Length)
        throw new ValidationException("More than one holiday for a given date");

      if (partialHolidayDates.Intersect(fullHolidayDates).Count() > 0)
        throw new ValidationException("There is a holiday and a partial holiday on the same date");
    }
  }
}
