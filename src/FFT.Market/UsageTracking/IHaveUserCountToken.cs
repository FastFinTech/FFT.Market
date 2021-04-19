// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.UsageTracking
{
  using System;

  /// <summary>
  /// Implement this class when you need to track the number of users of your
  /// service and take various actions depending on the changing count.
  /// </summary>
  public interface IHaveUserCountToken
  {
    /// <summary>
    /// Gets a value indicating whether this object will self-dispose when all
    /// user count tokens have been disposed. Default value is <c>true.</c>
    /// Setting this value <c>false</c> will prevent the object from
    /// self-disposing.
    /// </summary>
    bool ShouldDisposeWhenAllUsersAreFinished { get; }

    /// <summary>
    /// Calling this method indicators that your code is using the service.
    /// Disposing the result indicates that your code has finished using it.
    /// </summary>
    IDisposable GetUserCountToken();
  }
}
