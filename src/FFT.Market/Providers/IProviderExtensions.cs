// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Providers
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;

  public static class IProviderExtensions
  {
    /// <summary>
    /// Throws an exception if the provider is in error state.
    /// </summary>
    [DebuggerStepThrough]
    public static void ThrowIfInError(this IProvider provider)
    {
      if (provider.State == ProviderStates.Error)
        throw new Exception("Error in " + provider.Name, provider.Exception);
    }

    /// <summary>
    /// Throws an exception if any of the providers are in error state.
    /// </summary>
    [DebuggerStepThrough]
    public static void ThrowIfAnyHasError(this IEnumerable<IProvider> providers)
    {
      foreach (var provider in providers)
        provider.ThrowIfInError();
    }
  }
}
