// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.TickStreams
{
  using System.Linq;
  using FFT.Market.Sessions.TradingHoursSessions;
  using FFT.Market.Ticks;

  /// <summary>
  /// Reads ticks from an uninterrupted 24x7 tick stream and yields only the
  /// ticks that fall within sessions of the given session iterator.
  /// </summary>
  public class SessionRespectingTickStreamReader : ITickStreamReader
  {
    private readonly TradingSessions _tradingSessions;
    private readonly ITickStreamReader _tickStreamReader;
    private readonly TradingSessionIterator _iterator;

    public SessionRespectingTickStreamReader(TradingSessions tradingSessions, ITickStreamReader tickStreamReader)
    {
      _tradingSessions = tradingSessions;
      _tickStreamReader = tickStreamReader;
      var timeOfFirstTick = tickStreamReader.PeekNext()!.TimeStamp;
      var sessionDateOfFirstTick = _tradingSessions.GetActualSessionAt(timeOfFirstTick).SessionDate;
      _iterator = new TradingSessionIterator(_tradingSessions, sessionDateOfFirstTick);
      Info = new TickStreamInfo(tickStreamReader.Info.Instrument, tradingSessions);
    }

    public TickStreamInfo Info { get; }

    public long BytesRemaining => _tickStreamReader.BytesRemaining;

    public Tick? PeekNext()
    {
      var tick = _tickStreamReader.PeekNext();
      if (tick is null) return null;
      _iterator.MoveUntil(tick.TimeStamp);
      while (!_iterator.IsInSession)
      {
        _tickStreamReader.ReadNext();
        tick = _tickStreamReader.PeekNext();
        if (tick is null) return null;
        _iterator.MoveUntil(tick.TimeStamp);
      }

      return tick;
    }

    public Tick? ReadNext()
    {
      var tick = _tickStreamReader.ReadNext();
      if (tick is null) return null;
      _iterator.MoveUntil(tick.TimeStamp);
      while (!_iterator.IsInSession)
      {
        tick = _tickStreamReader.ReadNext();
        if (tick is null) return null;
        _iterator.MoveUntil(tick.TimeStamp);
      }

      return tick;
    }
  }
}
