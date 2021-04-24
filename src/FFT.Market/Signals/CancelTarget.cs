// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Signals
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using FFT.TimeStamps;

  public sealed class CancelTarget : ICommand
  {
    public Guid AggregateId { get; init; }
    public long ExpectedVersion { get; init; }
    public TimeStamp At { get; init; }
    public string Reason { get; init; }
  }
}
