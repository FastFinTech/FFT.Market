// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Engines.ElliotWave
{
  using System.Collections.Generic;
  using FFT.Market.ProcessingContexts;
  using FFT.Market.Ticks;

  public sealed class ElliotWaveEngine : EngineBase
  {
    public ElliotWaveEngine(ProcessingContext processingContext, ElliotWaveEngineSettings settings)
      : base(processingContext)
    {
      Settings = settings;
      Name = $"{nameof(ElliotWaveEngine)}";
    }

    public override string Name { get; }

    public ElliotWaveEngineSettings Settings { get; }

    public override IEnumerable<object> GetDependencies()
    {
      throw new System.NotImplementedException();
    }

    public override void OnTick(Tick tick)
    {
      throw new System.NotImplementedException();
    }
  }
}
