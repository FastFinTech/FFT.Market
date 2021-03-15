// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.DependencyTracking
{
  using System.Collections.Generic;

  public interface IHaveDependencies
  {
    IEnumerable<object> GetDependencies();
  }
}
