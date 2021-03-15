// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Sessions.SettlementSessions
{
  using System;
  using System.Linq;
  using FFT.Market.Instruments;
  using FFT.TimeStamps;

  public sealed class SettlementSession : ISession
  {
    public SettlementSession(IInstrument instrument, DateStamp sessionDate)
    {
      if (instrument is null)
        throw new ArgumentNullException(nameof(instrument));

      if (instrument.SettlementTime.TimeOfDay.TotalHours <= 0)
        throw new ArgumentException("Settlement time of day with zero or negative time is not supported.");

      if (instrument.SettlementTime.TimeOfDay.TotalHours > 24)
        throw new ArgumentException("Settlement time of day in early morning of following day is not yet supported.");

      if (!instrument.IsTradingDay(sessionDate))
        throw new ArgumentException($"Unexpected sessionDate '{sessionDate}'. It should not be a valid trading day.");

      Instrument = instrument;
      SessionDate = sessionDate;

      SessionEnd = new TimeStamp(SessionDate, instrument.SettlementTime.TimeOfDay, TimeZone);

      var sessionStartDate = Instrument.ThisOrPreviousTradingDay(SessionDate.AddDays(-1));
      SessionStart = new TimeStamp(sessionStartDate, instrument.SettlementTime.TimeOfDay, TimeZone);
    }

    public IInstrument Instrument { get; }

    public TimeZoneInfo TimeZone => Instrument.SettlementTime.TimeZone;

    public DateStamp SessionDate { get; }

    public TimeStamp SessionStart { get; }

    public TimeStamp SessionEnd { get; }

    public static SettlementSession GetLastCompletedAt(IInstrument instrument, TimeStamp at)
    {
      return Create(instrument, at).GetPrevious();
    }

    /// <summary>
    /// Creates the settlement session active at the given moment in time,
    /// <paramref name="at"/>. If <paramref name="at"/> is at the exact session
    /// switchover time, the session returned is the session that is ending, NOT
    /// the session that is starting. Therefore, if you have the session start
    /// time and you want to get the session that's about to start, it would be
    /// best to call this method using "Create(sessionStartTime.AddTicks(1))".
    /// </summary>
    public static SettlementSession Create(IInstrument instrument, TimeStamp at)
    {
      return new SettlementSession(instrument, GetSessionDate(instrument, at));
    }

    /// <summary>
    /// Returns the date of the session in progress at the given moment in time
    /// <paramref name="at"/>.
    /// </summary>
    public static DateStamp GetSessionDate(IInstrument instrument, TimeStamp at)
    {
      if (instrument.SettlementTime.TimeOfDay.TotalHours <= 0)
        throw new ArgumentException("Settlement time of day with zero or negative time is not supported.");

      if (instrument.SettlementTime.TimeOfDay.TotalHours > 24)
        throw new ArgumentException("Settlement time of day in early morning of following day is not yet supported.");

      var atTimeOfDay = at.ToTimeOfDay(instrument.SettlementTime.TimeZone);
      var nextSettlement = at.GetNext(instrument.SettlementTime.TimeOfDay, instrument.SettlementTime.TimeZone);
      var nextSettlementDate = nextSettlement.GetDate(instrument.SettlementTime.TimeZone);

      if (atTimeOfDay <= instrument.SettlementTime.TimeOfDay)
        return instrument.ThisOrNextTradingDay(nextSettlementDate);

      return instrument.ThisOrNextTradingDay(nextSettlementDate.AddDays(1));
    }

    public SettlementSession GetNext()
    {
      return new SettlementSession(Instrument, Instrument.ThisOrNextTradingDay(SessionDate.AddDays(1)));
    }

    public SettlementSession GetPrevious()
    {
      return new SettlementSession(Instrument, Instrument.ThisOrPreviousTradingDay(SessionDate.AddDays(-1)));
    }
  }
}
