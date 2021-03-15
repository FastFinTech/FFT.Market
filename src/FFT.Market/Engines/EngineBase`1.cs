// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Engines
{
  using FFT.Market.ProcessingContexts;

  /// <summary>
  /// Inherit this engine base class if your engine needs a settings object.
  /// </summary>
  /// <typeparam name="TSettings">The type of the settings object</typeparam>
  public abstract class EngineBase<TSettings> : EngineBase
    where TSettings : EngineSettings
  {
    protected EngineBase(ProcessingContext processingContext, TSettings settings)
        : base(processingContext)
    {
      Settings = settings;
    }

    public TSettings Settings { get; }
  }
}
