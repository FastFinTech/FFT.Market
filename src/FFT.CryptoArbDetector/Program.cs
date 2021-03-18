// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.CryptoArbDetector
{
  using System;
  using System.Threading.Tasks;
  using FFT.Market;
  using FFT.Market.Engines.SimpleArb;
  using FFT.Market.ProcessingContexts;
  using FFT.TimeStamps;
  using Nito.AsyncEx;

  internal class Program
  {
    private static async Task Main(string[] args)
    {
      MonitoringPair[] pairs = Array.Empty<MonitoringPair>();
      while (true)
      {
        using var context = new ProcessingContext(TimeStamp.Now);
        foreach (var pair in pairs)
        {
          var engine = SimpleArbEngine.Get(context, pair.Instrument1, pair.Instrument2);
          engine.NewArbCreated += Engine_NewArbCreated;
        }

        context.ReadyTask.ContinueWith(t => { }, TaskContinuationOptions.OnlyOnRanToCompletion).Ignore();
        context.Start();
        await context.WaitForErrorAsync(default);
        await Task.Delay(1000);
      }
    }

    private static void Engine_NewArbCreated(SimpleArbEngine arg1, IArbEvent arg2)
    {
      throw new NotImplementedException();
    }
  }
}
