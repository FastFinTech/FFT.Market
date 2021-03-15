// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.DataSeries
{
  public interface IValueSeries<T> : IReadOnlyValueSeries<T>
  {
    void Add(T value);
    void Update(T value);
    void Set(int index, T value);
  }
}
