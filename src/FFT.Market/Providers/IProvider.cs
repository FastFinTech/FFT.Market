// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Providers
{
  using System;
  using FFT.Market.DependencyTracking;
  using FFT.Market.UsageTracking;

  public interface IProvider : IHaveUserCountToken, IHaveDependencies, IHaveReadyTask, IHaveErrorTask, IDisposable
  {
    string Name { get; }
    ProviderStates State { get; }
    Exception? Exception { get; }
    ProviderStatus GetStatus();
    void Start();
  }
}
