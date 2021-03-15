// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Instruments
{
  using System;

  public sealed record SettlementTime
  {
    public TimeZoneInfo TimeZone { get; init; }

    public TimeSpan TimeOfDay { get; init; }
  }
}
