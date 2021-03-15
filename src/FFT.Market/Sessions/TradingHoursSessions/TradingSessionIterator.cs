// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Sessions.TradingHoursSessions
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using FFT.TimeStamps;

  /// <summary>
  /// Iterates through trading sessions. IMPORTANT!! Read the spec commented in
  /// <see cref="ISessionIterator"/> so you know exactly how this class is
  /// expected to work.
  /// </summary>
  public sealed class TradingSessionIterator : ISessionIterator
  {
    private readonly TradingSessions _tradingHours;

    /// <param name="approximateFirstMoveUntilDate">
    /// Provide the approximate date of the first <see
    /// cref="MoveUntil(TimeStamp)"/> method call so that we can initialize the
    /// iterator so that the first <see cref="MoveUntil(TimeStamp)"/> call will
    /// result in the conditions specified in the commenting for <see
    /// cref="ISessionIterator"/>.
    /// </param>
    public TradingSessionIterator(TradingSessions tradingHours, DateStamp approximateFirstMoveUntilDate)
    {
      _tradingHours = tradingHours;

      var at = new TimeStamp(approximateFirstMoveUntilDate.AddDays(-12).DateTime.Ticks);
      Current = _tradingHours.GetActualSessionAt(at);
      Next = _tradingHours.GetActualSessionAt(Current.SessionEnd.AddTicks(1));
      MoveNext();
      MoveNext();
      CurrentTime = Current.SessionStart.AddTicks(1);
      IsInSession = true;
      IsFirstTickOfSession = true;
      IsNewSession = true;
    }

    public TimeZoneInfo TimeZone => _tradingHours.TimeZone;

    public ActualTradingSession Previous { get; private set; }

    public ActualTradingSession Current { get; private set; }

    public ActualTradingSession Next { get; private set; }

    ISession ISessionIterator.Previous => Previous;

    ISession ISessionIterator.Current => Current;

    ISession ISessionIterator.Next => Next;

    public bool IsNewSession { get; private set; }

    public bool IsInSession { get; private set; }

    public bool IsFirstTickOfSession { get; private set; }

    public TimeStamp CurrentTime { get; private set; }

    /// <summary>
    /// Increments sessions until Current.SessionEndExchange >= atExchange.
    /// </summary>
    public void MoveUntil(TimeStamp at)
    {
      if (at < CurrentTime) Debugger.Break();
      CurrentTime = at;
      if (at <= Current.SessionEnd)
      {
        IsNewSession = false;
        if (at > Current.SessionStart)
        {
          IsFirstTickOfSession = !IsInSession;
          IsInSession = true;
        }
        else
        {
          IsInSession = false;
          IsFirstTickOfSession = false;
        }
      }
      else
      {
        do MoveNext(); while (at > Current.SessionEnd);
        IsNewSession = true;
        IsInSession = at > Current.SessionStart;
        IsFirstTickOfSession = IsInSession;
      }
    }

    private void MoveNext()
    {
      Previous = Current;
      Current = Next;
      Next = _tradingHours.GetActualSessionAt(Current.SessionEnd.AddTicks(1));
    }
  }
}
