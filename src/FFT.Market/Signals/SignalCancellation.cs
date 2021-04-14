﻿// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Signals
{
  using FFT.TimeStamps;

  public sealed record SignalCancellation
  {
    public TimeStamp At { get; init; }
    public string Reason { get; init; }
  }
}
