// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics.CodeAnalysis;

  public sealed class EnumerableEqualityComparer<T> : IEqualityComparer<IEnumerable<T>>
  {
    public static readonly EnumerableEqualityComparer<T> Default = new();

    public bool Equals(IEnumerable<T>? x, IEnumerable<T>? y)
    {
      if (ReferenceEquals(x, y)) return true;
      if (x is null || y is null) return false;

      using var enumeratorX = x.GetEnumerator();
      using var enumeratorY = y.GetEnumerator();
      while(enumeratorX.MoveNext())
      {
        if (!enumeratorY.MoveNext())
          return false;
        if (!EqualityComparer<T>.Default.Equals(enumeratorX.Current, enumeratorY.Current))
          return false;
      }

      return !enumeratorY.MoveNext();
    }

    public int GetHashCode([DisallowNull] IEnumerable<T> obj)
    {
      HashCode hash = default;
      hash.Add(typeof(T));
      foreach (var value in obj)
        hash.Add(value);
      return hash.ToHashCode();
    }
  }
}
