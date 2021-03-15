// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.TickStreams
{
  using System;
  using FFT.Market.Ticks;
  using FFT.TimeStamps;

  public interface ITickStream : IDisposable
  {
    TickStreamInfo Info { get; }
    long DataLength { get; }
    int Count { get; }
    void WriteTick(Tick tick);
    ITickStreamReader CreateReader();
    ITickStreamReader CreateReaderFrom(TimeStamp timeStamp);
  }
}
