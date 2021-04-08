// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.UsageTracking
{
  using System;
  using Nito.Disposables;

  /// <summary>
  /// Use an instance of this class within any of your classes when they need to
  /// implement the <see cref="IHaveUserCountToken"/> interface.
  /// </summary>
  public sealed class UserTokenMonitor : IHaveUserCountToken
  {
    private readonly object _sync = new();

    private int _userCount = 0;

    /// <summary>
    /// This event is invoked whenever the user count changes. Subscribe to it
    /// to respond to changing user count.
    /// </summary>
    public event Action<int> UserCountChanged;

    /// <summary>
    /// This event is invoked when the user count drops back down to zero.
    /// </summary>
    public event Action UserCountZero;

    /// <inheritdoc />
    public IDisposable GetUserCountToken()
    {
      lock (_sync)
      {
        UserCountChanged?.Invoke(++_userCount);
        return Disposable.Create(() =>
        {
          lock (_sync)
          {
            UserCountChanged?.Invoke(--_userCount);
            if (_userCount == 0)
              UserCountZero?.Invoke();
          }
        });
      }
    }
  }
}
