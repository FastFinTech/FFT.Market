// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.ProcessingContexts
{
  public enum ProcessingContextState
  {
    /// <summary>
    /// Processing context has been created and is being initialized.
    /// (EngineProviders and BarProviders are being setup).
    /// </summary>
    Initializing = 0,

    /// <summary>
    /// Processing context is waiting for all tick data supplies to finish
    /// loading.
    /// </summary>
    Loading = 1,

    /// <summary>
    /// Processing context is processing historical ticks.
    /// </summary>
    ProcessingHistorical = 2,

    /// <summary>
    /// Processing context is processing live ticks.
    /// </summary>
    ProcessingLive = 3,

    /// <summary>
    /// Processing context has experienced an error and has stopped processing
    /// ticks.
    /// </summary>
    Error = 4,
  }
}
