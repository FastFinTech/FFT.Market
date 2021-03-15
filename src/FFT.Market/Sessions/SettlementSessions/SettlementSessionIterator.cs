// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Sessions.SettlementSessions
{
  using System;
  using FFT.Market.Instruments;
  using FFT.TimeStamps;

  /// <summary>
  /// Iterates through sessions generated from the settlement time of an instrument.
  /// IMPORTANT!! Read the spec commented in <see cref="ISessionIterator"/> so you know
  /// exactly how this class is expected to work.
  /// </summary>
  public sealed class SettlementSessionIterator : ISessionIterator
  {
    /// <param name="approximateFirstMoveUntilDate">
    /// Provide the approximate date of the first <see cref="MoveUntil(TimeStamp)"/> method call
    /// so that we can initialize the iterator so that the first <see cref="MoveUntil(TimeStamp)"/> call will
    /// result in the conditions specified in the commenting for <see cref="ISessionIterator"/>.
    /// </param>
    public SettlementSessionIterator(IInstrument instrument, DateStamp approximateFirstMoveUntilDate)
    {
      TimeZone = instrument.SettlementTime.TimeZone;
      Current = new SettlementSession(instrument, instrument.ThisOrPreviousTradingDay(approximateFirstMoveUntilDate.AddDays(-10)));
      Previous = Current.GetPrevious();
      Next = Current.GetNext();
      MoveUntil(Current.SessionStart.AddTicks(1));
    }

    public TimeZoneInfo TimeZone { get; }

    public SettlementSession Previous { get; private set; }

    public SettlementSession Current { get; private set; }

    public SettlementSession Next { get; private set; }

    ISession ISessionIterator.Previous => Previous;

    ISession ISessionIterator.Current => Current;

    ISession ISessionIterator.Next => Next;

    public bool IsNewSession { get; private set; }

    public bool IsInSession => true;

    public bool IsFirstTickOfSession => IsNewSession;

    public TimeStamp CurrentTime { get; private set; }

    /// <summary>
    /// Increments sessions until Current.SessionEnd >= until.
    /// </summary>
    public void MoveUntil(TimeStamp until)
    {
      CurrentTime = until;
      if (until <= Current.SessionEnd)
      {
        IsNewSession = false;
      }
      else
      {
        do MoveNext(); while (until > Current.SessionEnd);
        IsNewSession = true;
      }
    }

    private void MoveNext()
    {
      Previous = Current;
      Current = Next;
      Next = Current.GetNext();
    }
  }
}
