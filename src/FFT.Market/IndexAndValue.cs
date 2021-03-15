// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market
{
  public sealed record IndexAndValue
  {
    public IndexAndValue(int index, double value)
      => (Index, Value) = (index, value);

    public int Index { get; set; }
    public double Value { get; set; }
  }
}
