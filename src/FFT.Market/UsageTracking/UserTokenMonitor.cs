// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.UsageTracking
{
  using System;
  using System.Threading;
  using Nito.Disposables;

  /// <summary>
  /// Use an instance of this class within any of your classes when they need to
  /// implement the <see cref="IHaveUserCountToken"/> interface.
  /// </summary>
  public sealed class UserTokenMonitor : IHaveUserCountToken
  {
    private int _userCount = 0;

    /// <summary>
    /// This event is invoked whenever the user count changes. Subscribe to it
    /// to respond to changing user count. It is NOT threadsafe. If multiple
    /// user additions and removals happen at about the same time, this event
    /// might fire in an uneven sequence.
    /// </summary>
    public event Action<int> UserCountChanged;

    /// <summary>
    /// This event is invoked when the user count drops back down to zero.
    /// </summary>
    public event Action UserCountZero;

    /// <inheritdoc />
    public IDisposable GetUserCountToken()
    {
      var count = Interlocked.Increment(ref _userCount);
      UserCountChanged?.Invoke(count);
      return Disposable.Create(() =>
      {
        var count = Interlocked.Decrement(ref _userCount);
        UserCountChanged?.Invoke(count);
        if (count == 0)
          UserCountZero?.Invoke();
      });
    }
  }
}
