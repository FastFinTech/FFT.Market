// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.CryptoArbDetector
{
  using FFT.Market.Instruments;

  internal sealed record MonitoringPair
  {
    public IInstrument Instrument1 { get; init; }
    public IInstrument Instrument2 { get; init; }
  }
}
