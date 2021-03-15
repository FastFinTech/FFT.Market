// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Bars
{
  using FFT.TimeStamps;

  /// <summary>
  /// Contains information that can be used to find a particular bar in a bar
  /// series. This is of particular use when a bar series can contain multiple
  /// bars with identical timestamps, and you are identifying the bar not only
  /// by its timestamp, but also by its close price and a sequence number.
  /// </summary>
  public class BarPositionDescriptor
  {
    /// <summary>
    /// The timestamp of the bar.
    /// </summary>
    public TimeStamp TimeStamp { get; set; }

    /// <summary>
    /// The close price of the bar.
    /// </summary>
    public double Close { get; set; }

    /// <summary>
    /// If the bar series contains several bars with the same timestamp and
    /// close price, this value will contain the sequence number of the
    /// target bar.
    /// </summary>
    public int SequenceNumber { get; set; }
  }
}
