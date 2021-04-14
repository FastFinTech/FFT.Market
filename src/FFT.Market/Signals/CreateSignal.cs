// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Signals
{
  using System;
  using FFT.TimeStamps;

  public sealed class CreateSignal : ICommand
  {
    public Guid AggregateId { get; init; }
    public long ExpectedVersion { get; init; }
    public TimeStamp At { get; init; }
    public string StrategyName { get; init; }
    public string SignalName { get; init; }
    public string Instrument { get; init; }
    public string Exchange { get; init; }
  }
}
