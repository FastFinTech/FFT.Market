// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.TickStreams
{
  using System;
  using FFT.Market.Instruments;
  using FFT.Market.Ticks;
  using FFT.TimeStamps;

  public interface ITickStream : IDisposable
  {
    IInstrument Instrument { get; }

    /// <summary>
    /// Number of bytes of data used to store the ticks.
    /// </summary>
    long DataLength { get; }

    void WriteTick(Tick tick);

    /// <summary>
    /// Creates a reader from the beginning of the stream.
    /// </summary>
    ITickStreamReader CreateReader();

    /// <summary>
    /// Creates a reader that starts just AFTER the given <paramref
    /// name="timeStamp"/>. Ticks at exactly <paramref name="timeStamp"/> will
    /// not be included in the reader's output.
    /// </summary>
    ITickStreamReader CreateReaderFrom(TimeStamp timeStamp);
  }
}
