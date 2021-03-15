// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Sessions.SettlementSessions
{
  using FFT.Market.Instruments;
  using FFT.TimeStamps;

  /// <inheritdoc/>
  public sealed record SettlementSessionInfo : ISessionInfo
  {
    public IInstrument Instrument { get; }

    public SettlementSessionInfo(IInstrument instrument)
    {
      Instrument = instrument;
    }

    public ISessionIterator CreateIterator(TimeStamp from)
        => new SettlementSessionIterator(Instrument, approximateFirstMoveUntilDate: from.GetDate());

    public ISession GetActiveSessionAt(TimeStamp at)
        => SettlementSession.Create(Instrument, at);
  }
}
