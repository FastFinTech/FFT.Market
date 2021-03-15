// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Ticks
{
  using FFT.Market.TickStreams;
  using FFT.TimeStamps;

  /// <summary>
  /// Represents a single transaction in the market.
  /// </summary>
  public record Tick
  {
    public TickStreamInfo Info { get; init; }
    public long SequenceNumber { get; init; }
    public double Price { get; init; }
    public double Volume { get; init; }
    public double Bid { get; init; }
    public double Ask { get; init; }
    public TimeStamp TimeStamp { get; init; }

    public bool ValuesEqual(Tick other)
      => Price == other.Price
          && Volume == other.Volume
          && Bid == other.Bid
          && Ask == other.Ask
          && TimeStamp == other.TimeStamp;
  }
}
