// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Sessions
{
  using System;
  using FFT.TimeStamps;

  public interface ISession
  {
    TimeZoneInfo TimeZone { get; }

    DateStamp SessionDate { get; }

    TimeStamp SessionStart { get; }

    TimeStamp SessionEnd { get; }
  }
}
