// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Sessions.InfiniteSession
{
  using System;
  using FFT.TimeStamps;
  using FFT.TimeZoneList;

  public sealed class InfiniteSession : ISession
  {
    public static readonly InfiniteSession Instance = new InfiniteSession();

    private InfiniteSession() { }

    public TimeZoneInfo TimeZone => TimeZones.UTC;

    public DateStamp SessionDate => DateStamp.MinValue;

    public TimeStamp SessionStart => TimeStamp.MinValue;

    public TimeStamp SessionEnd => TimeStamp.MaxValue;
  }
}
