// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.DataSeries.DataSeries
{
  using System;
  using System.Collections;
  using System.Collections.Generic;

  public class ReadOnlyValueWrapper<T> : IReadOnlyValueSeries<T>
  {
    private readonly Func<int> _getCount;
    private readonly Func<int, T> _getItem;
    private readonly Func<IEnumerable<object>> _getDependencies;
    private readonly Func<IEnumerator<T>> _getEnumerator;

    public ReadOnlyValueWrapper(Func<int> getCount, Func<int, T> getItem, Func<IEnumerable<object>> getDependencies, Func<IEnumerator<T>> getEnumerator)
    {
      _getCount = getCount;
      _getItem = getItem;
      _getDependencies = getDependencies;
      _getEnumerator = getEnumerator;
    }

    public int Count => _getCount();
    public T this[int index] => _getItem(index);
    public IEnumerable<object> GetDependencies() => _getDependencies();
    public IEnumerator<T> GetEnumerator() => _getEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _getEnumerator();
  }
}
