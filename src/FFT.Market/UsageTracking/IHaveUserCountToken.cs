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
    /// Calling this method indicators that your code is using the service.
    /// Disposing the result indicates that your code has finished using it.
    /// </summary>
    IDisposable GetUserCountToken();
  }
}
