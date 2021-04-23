// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Signals
{
  using System;
  using FFT.TimeStamps;

  public interface ICommand
  {
    Guid AggregateId { get; }
    long ExpectedVersion { get; }
    TimeStamp At { get; }
  }
}
