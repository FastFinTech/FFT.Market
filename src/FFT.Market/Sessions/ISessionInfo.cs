// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Sessions
{
  using FFT.TimeStamps;

  /// <summary>
  /// Implementations of this base class contain all the information needed to generate an
  /// <see cref="ISessionIterator"/> of any kind.
  /// Implementations are required to be Memberwise equatable so that they can be used
  /// as part of the key for cached data and data providers.
  /// </summary>
  public interface ISessionInfo
  {
    ISessionIterator CreateIterator(TimeStamp from);

    /// <summary>
    /// This is CPU-intensive. Don't use it in repetitive code! If <paramref
    /// name="at"/> is the exact beginning of a session and also the exact end
    /// of the previous session, this method will return the previous session,
    /// because sessions are inclusive of their end time and exclusive of their
    /// start time. Therefore, if you have the session start time and you want
    /// the session that's about to start, it would be best to call this method
    /// using "GetActiveSessionAt(sessionStartTime.AddTicks(1))".
    /// </summary>
    ISession GetActiveSessionAt(TimeStamp at);
  }
}
