// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Providers.Ticks
{
  using FFT.Market.Instruments;
  using FFT.TimeStamps;

  public sealed record TickProviderInfo
  {
    public IInstrument Instrument { get; init; }
    public TimeStamp From { get; init; }
    public TimeStamp? Until { get; init; }
  }
}
