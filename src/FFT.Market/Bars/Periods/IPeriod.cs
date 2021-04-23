// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Bars.Periods
{
  public interface IPeriod
  {
    string Name { get; }

    bool IsEvenTimeSpacingBars { get; }

    bool IsUnevenTimeSpacingBars => !IsEvenTimeSpacingBars;

    IPeriod Multiply(double value);
  }
}
