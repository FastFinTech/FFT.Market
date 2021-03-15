// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.DataSeries
{
  using System.Collections;
  using System.Collections.Generic;
  using FFT.Market.Bars;

  public class BarBasedValueSeries : IReadOnlyValueSeries<double>
  {
    public BarBasedValueSeries(IBars bars, BarInputType inputType)
    {
      Bars = bars;
      InputType = inputType;
    }

    public IBars Bars { get; }
    public BarInputType InputType { get; }
    public int Count => Bars.Count;
    public object Source => Bars;
    public double this[int index] => Bars.GetValue(InputType, index);

    public IEnumerable<object> GetDependencies()
    {
      yield return Bars;
    }

    public IEnumerator<double> GetEnumerator()
    {
      for (var i = 0; i < Bars.Count; i++)
        yield return Bars.GetValue(InputType, i);
    }

    IEnumerator IEnumerable.GetEnumerator()
      => GetEnumerator();
  }
}
