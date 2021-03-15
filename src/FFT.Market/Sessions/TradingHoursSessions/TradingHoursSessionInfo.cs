// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Sessions.TradingHoursSessions
{
  using FFT.TimeStamps;

  /// <summary>
  /// See commenting on <see cref="ISessionInfo"/> base class.
  /// </summary>
  public sealed record TradingHoursSessionInfo : ISessionInfo
  {
    public TradingHoursSessionInfo(TradingSessions tradingSessions)
    {
      TradingSessions = tradingSessions;
    }

    public TradingSessions TradingSessions { get; }

    public ISessionIterator CreateIterator(TimeStamp from)
        => new TradingSessionIterator(TradingSessions, approximateFirstMoveUntilDate: from.GetDate());

    public ISession GetActiveSessionAt(TimeStamp timeStamp)
        => TradingSessions.GetActualSessionAt(timeStamp);
  }
}
