// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market
{
  using System;
  using System.Runtime.CompilerServices;
  using static System.Math;
  using static System.MidpointRounding;

  /// <summary>
  /// Provides extension methods that perform <c>decimal</c>-precision
  /// operations on <c>double</c> objects. A <c>double</c> is half the size of a
  /// <c>decimal</c>, and operations are fifteen times faster. This makes it
  /// imperative to use the <c>double</c> type in a fintech app's hotpath, and
  /// these high-precision operations become needful as a result.
  /// </summary>
  public static class DoubleExtensions
  {
    /// <summary>
    /// Rounds the given <paramref name="value"/> to the nearest integer value
    /// and returns it as an <c>int</c>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RoundToInt(this double value)
      => (int)Round(value, AwayFromZero);

    /// <summary>
    /// Rounds the given <paramref name="value"/> to the nearest increment
    /// value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double RoundToIncrement(this double value, double increment)
    {
      // faster implementation: 16sec for 100,000,000 iterations this
      // implementation has a double division and a double rounding, and only
      // then casts to decimal. operations on doubles are about fifteen times
      // faster than operations on decimals, so this implementation is faster
      // because it performs fewer decimal operations than the older
      // implementation
      return (double)((decimal)Round(value / increment, AwayFromZero) * (decimal)increment);

      // older, slower implementation: 27sec for 100,000,000 iterations
      // this implementation is slower (almost twice as slow) because it has an extra decimal division and decimal rounding
      //var valueAsDecimal = (decimal)value;
      //var tickSizeAsDecimal = (decimal)tickSize;
      //return (double)(Math.Round(valueAsDecimal / tickSizeAsDecimal) * tickSizeAsDecimal);
    }

    /// <summary>
    /// Adds the given <paramref name="increment"/> <paramref name="numIncrements"/>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double AddIncrements(this double value, double increment, int numIncrements)
      => (double)((decimal)Round((value / increment) + numIncrements, AwayFromZero) * (decimal)increment);
    // TODO: Benchmark this and see if it's any faster.
    // => (double)(((value / increment).RoundToInt() + numIncrements) * (decimal)increment);

    /// <summary>
    /// Converts the given <paramref name="value"/> to an integer value
    /// representing the number of <paramref name="increment"/>s it contains.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToIncrements(this double value, double increment)
      => (int)Round(value / increment, AwayFromZero);

    /// <summary>
    /// Converts the given <paramref name="numIncrements"/> and <paramref
    /// name="increment"/> to a <c>double</c> value representing the actual
    /// value of the given number of increments.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ToPoints(this int numIncrements, double increment)
      => (double)(numIncrements * (decimal)increment);

    /// <summary>
    /// Performs a comparison of the two values, returning "0" (equal) if the
    /// two values differ by less than <see cref="double.Epsilon"/>. Otherwise,
    /// <c>1</c> is returned if <paramref name="value"/> is greater. <c>-1</c>
    /// is returned if <paramref name="other"/> is greater.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ApproxCompare(this double value, double other)
    {
      var difference = value - other;
      if (difference > double.Epsilon) return 1;
      if (difference < -double.Epsilon) return -1;
      return 0;
    }

    /// <summary>
    /// Rounds the given <paramref name="value"/> to the given <paramref
    /// name="numSignificantFigures"/> and returns the result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double RoundToSignificantFigures(this double value, int numSignificantFigures)
    {
      if (value == 0) return 0;
      var scale = Pow(10, Floor(Log10(Abs(value))) + 1);
      // Perform the last step using decimals to prevent double-arithmetic re-introducing tiny errors (and more figures to the result)
      return (double)((decimal)scale * (decimal)Math.Round(value / scale, numSignificantFigures, AwayFromZero));
    }
  }
}
