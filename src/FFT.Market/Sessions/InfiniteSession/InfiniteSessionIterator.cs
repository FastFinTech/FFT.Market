// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Sessions.InfiniteSession
{
  using System;
  using FFT.TimeStamps;

  public sealed class InfiniteSessionIterator : ISessionIterator
  {
    public static readonly InfiniteSessionIterator Instance = new InfiniteSessionIterator();

    public TimeZoneInfo TimeZone => InfiniteSession.Instance.TimeZone;

    public ISession Previous => null!;

    public ISession Current => InfiniteSession.Instance;

    public ISession Next => null!;

    public bool IsNewSession => false;

    public bool IsInSession => true;

    public bool IsFirstTickOfSession => false;

    public TimeStamp CurrentTime { get; private set; }

    public void MoveUntil(TimeStamp until)
    {
      CurrentTime = until;
    }
  }
}
