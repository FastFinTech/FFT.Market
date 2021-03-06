// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Bars
{
  using System;
  using System.Collections.Generic;
  using FFT.Market.DataSeries;
  using FFT.Market.DependencyTracking;
  using FFT.TimeStamps;

  /// <summary>
  /// Inherit this interface for any bars object you create. This allows
  /// inversion of control. All tough objects work with IBars.
  /// </summary>
  public interface IBars : IEnumerable<IBar>, IHaveDependencies, ITimeStampSeries
  {
    /// <summary>
    /// All the important defining properties encapsulated in one object that
    /// can be used for "Equals" comparison.
    /// </summary>
    BarsInfo BarsInfo { get; }

    IReadOnlyValueSeries<double> OpenValueSeries { get; }
    IReadOnlyValueSeries<double> HighValueSeries { get; }
    IReadOnlyValueSeries<double> LowValueSeries { get; }
    IReadOnlyValueSeries<double> CloseValueSeries { get; }
    IReadOnlyValueSeries<double> VolumeValueSeries { get; }

    IBar this[Index index] { get; }

    IReadOnlyValueSeries<double> GetValueSeries(BarInputType inputType);

    // These two are included in the ITimeStampSeries interface:
    //  int Count { get; }
    TimeStamp GetTimeStamp(Index index);
    double GetOpen(Index index);
    double GetHigh(Index index);
    double GetLow(Index index);
    double GetClose(Index index);
    double GetVolume(Index index);
    double GetValue(BarInputType inputType, Index index);

    void AddNewBar(IBar bar);
    void UpdateLastBar(double open, double high, double low, double close, double volume, TimeStamp timestamp);
  }
}
