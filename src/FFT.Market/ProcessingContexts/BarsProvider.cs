//// Copyright (c) True Goodwill. All rights reserved.
//// Licensed under the MIT license. See LICENSE file in the project root for full license information.

//namespace FFT.Market.ProcessingContexts
//{
//  using System.Collections.Generic;
//  using System.Linq;
//  using FFT.Market.BarBuilders;
//  using FFT.Market.Bars;
//  using FFT.Market.Ticks;

//  public class BarsProvider
//  {
//    readonly object sync = new object();
//    public ProcessingContext ProcessingContext { get; }
//    public List<BarBuilder> BarBuilders { get; }

//    public BarsProvider(ProcessingContext processingContext)
//    {
//      ProcessingContext = processingContext;
//      BarBuilders = new List<BarBuilder>();
//    }

//    public IBars GetBars(BarsInfo barsInfo)
//    {
//      lock (sync)
//      {
//        var barBuilder = BarBuilders.SingleOrDefault(b => b.BarsInfo.Equals(barsInfo));
//        if (barBuilder is null)
//        {
//          barBuilder = BarBuilder.Create(barsInfo);
//          BarBuilders.Add(barBuilder);
//        }

//        return barBuilder.Bars;
//      }
//    }

//    public void ProcessTick(Tick tick)
//    {
//      foreach (var builder in BarBuilders)
//      {
//        builder.OnTick(tick);
//      }
//    }
//  }
//}
