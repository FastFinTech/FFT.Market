// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Services
{
  using FFT.Market.Providers.Ticks;

  public static class ServiceProvider
  {
#pragma warning disable SA1201 // Elements should appear in the correct order

    public static ITradingPlatformTime TradingPlatformTime { get; private set; }
    public static void SetTradingPlatformTime(ITradingPlatformTime time) => TradingPlatformTime = time;

    public static ILiveTickProviderFactory LiveTickProviderFactory { get; private set; }
    public static void SetLiveTickProviderFactory(ILiveTickProviderFactory factory) => LiveTickProviderFactory = factory;
  }
}
