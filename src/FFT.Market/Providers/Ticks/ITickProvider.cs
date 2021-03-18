// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Providers.Ticks
{
  using FFT.Market.TickStreams;

  public interface ITickProvider : IProvider
  {
    TickProviderInfo Info { get; }
    ITickStreamReader CreateReader();
  }
}
