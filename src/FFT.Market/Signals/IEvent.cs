// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Signals
{
  using System;
  using FFT.TimeStamps;

  public interface IEvent
  {
    Guid AggregateId { get; }
    long Version { get; }
    TimeStamp At { get; }
  }
}
