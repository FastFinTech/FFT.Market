// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.DataSeries
{
  using System.Collections.Generic;
  using FFT.Market.DependencyTracking;

  public interface IReadOnlyValueSeries<T> : IEnumerable<T>, IHaveDependencies
  {
    int Count { get; }
    T this[int index] { get; }
  }
}
