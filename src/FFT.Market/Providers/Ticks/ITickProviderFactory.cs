// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Providers.Ticks
{
  public interface ITickProviderFactory
  {
    ITickProvider GetTickProvider(TickProviderInfo info);
  }
}
