// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Sessions.InfiniteSession
{
  using FFT.TimeStamps;

  public sealed record InfiniteSessionInfo : ISessionInfo
  {
    public ISessionIterator CreateIterator(TimeStamp from)
      => InfiniteSessionIterator.Instance;

    public ISession GetActiveSessionAt(TimeStamp at)
      => InfiniteSessionIterator.Instance.Current;
  }
}
