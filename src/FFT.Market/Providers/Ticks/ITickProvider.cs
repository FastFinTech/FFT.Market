// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Providers.Ticks
{
  using FFT.Market.TickStreams;

  /// <summary>
  /// Implement this interface to provide tick data.
  /// </summary>
  public interface ITickProvider : IProvider
  {
    /// <summary>
    /// Gets the information describing the ticks provided by this instance.
    /// </summary>
    TickProviderInfo Info { get; }

    /// <summary>
    /// Creates an <see cref="ITickStreamReader"/> that you can use to read the
    /// ticks provided by this instance.
    /// </summary>
    ITickStreamReader CreateReader();
  }
}
