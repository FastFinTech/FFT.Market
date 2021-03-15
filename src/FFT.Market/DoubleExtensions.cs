// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Runtime.InteropServices;
  using System.Text;
  using System.Threading.Tasks;
  using static System.Math;

  public static class DoubleExtensions
  {
    public static double Round(this double value)
      => Math.Round(value, MidpointRounding.AwayFromZero);

    public static int RoundToInt(this double value)
      => (int)Math.Round(value, MidpointRounding.AwayFromZero);

    public static double RoundToTick(this double value, double tickSize)
    {
      // faster implementation: 16sec for 100,000,000 iterations
      // this implementation has a double division and a double rounding, and only then casts to decimal. 
      // operations on doubles are about fifteen times faster than operations on decimals, so this implementation is faster 
      // because it performs fewer decimal operations than the older implementation
      return (double)((decimal)Math.Round(value / tickSize, MidpointRounding.AwayFromZero) * (decimal)tickSize);

      // older, slower implementation: 27sec for 100,000,000 iterations
      // this implementation is slower (almost twice as slow) because it has an extra decimal division and decimal rounding
      //var valueAsDecimal = (decimal)value;
      //var tickSizeAsDecimal = (decimal)tickSize;
      //return (double)(Math.Round(valueAsDecimal / tickSizeAsDecimal) * tickSizeAsDecimal);
    }

    public static double AddTicks(this double value, double tickSize, int numTicks)
      => (double)((decimal)Math.Round((value / tickSize) + numTicks, MidpointRounding.AwayFromZero) * (decimal)tickSize);

    public static int ToTicks(this double value, double tickSize)
      => (int)Math.Round(value / tickSize, MidpointRounding.AwayFromZero);

    public static double ToPoints(this int numTicks, double tickSize)
      => (double)(numTicks * (decimal)tickSize);

    public static int ApproxCompare(this double value, double other)
    {
      var difference = value - other;
      if (difference > double.Epsilon) return 1;
      if (difference < -double.Epsilon) return -1;
      return 0;
    }

    public static double RoundToSignificantFigures(this double value, int numSignificantFigures)
    {
      if (value == 0) return 0.0;
      var scale = Pow(10, Floor(Log10(Abs(value))) + 1);
      // Perform the last step using decimals to prevent double-arithmetic re-introducing tiny errors (and more figures to the result)
      return (double)((decimal)scale * (decimal)Math.Round(value / scale, numSignificantFigures, MidpointRounding.AwayFromZero));
    }

    public static List<double> GetPricesForInterval(this double firstPrice, double secondPrice, double tickSize)
    {
      var result = new List<double>();
      if (secondPrice >= firstPrice)
      {
        for (var price = firstPrice; price <= secondPrice; price = price.AddTicks(tickSize, 1))
          result.Add(price);
      }
      else
      {
        for (var price = secondPrice; price <= firstPrice; price = price.AddTicks(tickSize, 1))
          result.Add(price);
      }

      return result;
    }
  }
}
