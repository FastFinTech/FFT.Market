// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Engines
{
  using System.Collections.Generic;
  using FFT.Market.DependencyTracking;
  using FFT.Market.ProcessingContexts;
  using FFT.Market.Ticks;

  /// <summary>
  /// The basic building block for processing tick data and maintaining market
  /// state information.
  /// </summary>
  public abstract class EngineBase : IHaveDependencies
  {
    protected EngineBase(ProcessingContext processingContext)
      => ProcessingContext = processingContext;

    /// <summary>
    /// A name to use to refer to the Engine in user messages, error messages,
    /// event logs etc.
    /// </summary>
    public abstract string Name { get; }

    protected ProcessingContext ProcessingContext { get; }

    public abstract IEnumerable<object> GetDependencies();

    /// <summary>
    /// Called by the processing context for each incoming tick. It is assumed
    /// that all engines returned by the InnerEngineDependencies property have
    /// already been updated for this tick.
    /// </summary>
    public abstract void OnTick(Tick tick);
  }
}
