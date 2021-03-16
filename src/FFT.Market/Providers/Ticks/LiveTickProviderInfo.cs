// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Providers.Ticks
{
  using FFT.Market.Instruments;
  using FFT.Market.Sessions.TradingHoursSessions;
  using FFT.TimeStamps;

  public sealed record LiveTickProviderInfo
  {
    public IInstrument Instrument { get; init; }
    public DateStamp FirstSessionDate { get; init; }
    public TradingSessions TradingSessions { get; init; }
  }
}
