// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Providers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Nito.AsyncEx;

  /// <summary>
  /// Stores providers of type <see cref="TProvider"/> and indexes them using
  /// <see cref="TInfo"/> as the key. Providers that have an error are disposed
  /// and removed from the store. <see cref="TInfo"/> MUST BE a
  /// memberwise-equality object for the cache-keying to work as expected.
  /// </summary>
  public class ProviderStore<TInfo, TProvider>
    where TProvider : class, IProvider
    where TInfo : notnull
  {
    private readonly object _sync = new();
    private readonly Dictionary<TInfo, TProvider> _store = new();
    private readonly Func<TInfo, TProvider> _constructor;

    public ProviderStore(Func<TInfo, TProvider> constructor)
      => _constructor = constructor;

    /// <summary>
    /// Gets the provider with the given info from the store if it exists, or creates a new one.
    /// The provider is started automatically, and will be removed from the store automatically if it errors.
    /// </summary>
    public TProvider GetCreate(TInfo info)
    {
      lock (_sync)
      {
        if (!_store.TryGetValue(info, out var provider))
        {
          provider = _constructor(info);
          _store[info] = provider;
          provider.ErrorTask.ContinueWith(
            t =>
            {
              lock (_sync)
              {
                _store.Remove(info);
              }
            },
            TaskScheduler.Default).Ignore();
          provider.Start();
        }

        return provider;
      }
    }

    /// <summary>
    /// Retrieves the first provider existing in the store that satisfies the given predicate.
    /// Returns null if none exists.
    /// </summary>
    public TProvider? GetFirstOrNull(Func<TProvider, bool> predicate)
    {
      lock (_sync)
      {
        foreach (var kv in _store)
        {
          if (predicate(kv.Value))
            return kv.Value;
        }

        return null;
      }
    }
  }
}
