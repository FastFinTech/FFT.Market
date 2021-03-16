// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Engines.SimpleArb
{
  using FFT.Market.Instruments;
  using FFT.TimeStamps;

  public interface IArbEvent
  {
    TimeStamp At { get; }
    TimeStamp? Until { get; }
    IInstrument Buy { get; }
    IInstrument Sell { get; }
  }
}
