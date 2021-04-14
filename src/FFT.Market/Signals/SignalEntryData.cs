﻿// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Signals
{
  using FFT.TimeStamps;

  public sealed record SignalEntryData
  {
    public TimeStamp At { get; init; }
    public Direction Direction { get; init; }
    public SignalEntryType EntryType { get; init; }
    public decimal Price { get; init; }
    public string Tag { get; init; }
  }
}