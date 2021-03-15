// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Sessions
{
  using System;
  using FFT.TimeStamps;

  /// <summary>
  /// Keeps track of the <see cref="ISession"/> active at any given moment in time.
  /// IMPORTANT!! Follow this spec when implementing an <see cref="ISessionIterator"/>.
  ///     1. A session is exclusive of its start time, and inclusive of its end time.
  ///         Therefore, if you move until the start time of a session, the current time will be
  ///         considered "not in session", unless, of course, it is also the end time of the previous session.
  ///     2. When there are gaps between sessions:
  ///         If you move to a time that is past the end of a session, but not past the start of the next session:
  ///             a) <see cref="Current"/> session will be incremented to the next session, which is not yet in progress.
  ///             b) <see cref="IsNewSession"/> will be True since this tick resulting in incrementing the <see cref="Current"/> property.
  ///             c) <see cref="IsInSession"/> will be False since we have not moved past the start of the <see cref="Current"/> session.
  ///             d) <see cref="IsFirstTickOfSession"/> will be False since we are not yet IN the active session period.
  ///         Then, you move a little further forward in time, but not past the start of the CURRENT session:
  ///             a) <see cref="IsNewSession"/> will now be False, because you have moved forward at least once and the <see cref="Current"/> session was not incremented.
  ///         Then, you move past the start of the CURRENT session.
  ///             a) <see cref="IsInSession"/> will become True.
  ///             b) <see cref="IsFirstTickOfSession"/> will be True, since this is the first tick actually inside the active session.
  ///             c) <see cref="IsNewSession"/> will still be False.
  ///     3. The FIRST call to <see cref="MoveUntil(TimeStamp)"/> should result in the following:
  ///         a) <see cref="IsNewSession"/> is set to True.
  ///         b) <see cref="IsInSession"/> and <see cref="IsFirstTickOfSession"/> are set to True if the given "until" time is inside the active session.
  ///         To achieve this, you will need to initialize the iterator in its constructor so that the <see cref="MoveUntil(TimeStamp)"/>
  ///         will result in the iterator moving forward to a new session with a minimum of cpu usage.
  /// </summary>
  public interface ISessionIterator
  {
    TimeZoneInfo TimeZone { get; }

    ISession Previous { get; }

    ISession Current { get; }

    ISession Next { get; }

    /// <summary>
    /// True when the most recent call to <see cref="MoveUntil(TimeStamp)"/> resulted in a move to a new session.
    /// Note that the <see cref="CurrentTime"/> may still be BEFORE the start of the active session,
    /// so <see cref="IsInSession"/> could still be false.
    /// </summary>
    bool IsNewSession { get; }

    /// <summary>
    /// True when <see cref="CurrentTime"/> is inside an active session.
    /// </summary>
    bool IsInSession { get; }

    /// <summary>
    /// True on the first move into an active session.
    /// </summary>
    bool IsFirstTickOfSession { get; }

    /// <summary>
    /// Contains the time moved to by the <see cref="MoveUntil(TimeStamp)"/> method.
    /// </summary>
    TimeStamp CurrentTime { get; }

    /// <summary>
    /// Moves the iterator forward to the given <paramref name="until"/> time,
    /// updating all the other state properties such as <see cref="IsInSession"/>.
    /// Make sure you call this ONLY with increasing <paramref name="until"/> values,
    /// or you will get unexpected results.
    /// </summary>
    void MoveUntil(TimeStamp until);
  }
}
