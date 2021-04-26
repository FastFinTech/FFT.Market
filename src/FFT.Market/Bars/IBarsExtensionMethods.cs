// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Bars
{
  using System;
  using System.Globalization;
  using FFT.Market.DataSeries;
  using FFT.TimeStamps;

  public static class IBarsExtensionMethods
  {
    public static bool ValuesEqual(this IBar bar1, IBar bar2)
    {
      if (bar1 is null || bar2 is null)
        return false;

      return bar1.Open == bar2.Open
          && bar1.High == bar2.High
          && bar1.Low == bar2.Low
          && bar1.Close == bar2.Close
          && bar1.Volume == bar2.Volume
          && bar1.TimeStamp == bar2.TimeStamp;
    }

    /// <summary>
    /// Use this method when there are multiple bars with the same timestamp,
    /// and you need to find the first bar at the given timestamp with the given
    /// close price.
    /// </summary>
    /// <param name="target">The IBars to be searched.</param>
    /// <param name="targetTimeStamp">The bar timestamp you are looking
    /// for.</param>
    /// <param name="targetClose">The close price of the bar you are looking
    /// for.</param>
    /// <param name="index">The index of the bar that was found, with respect to
    /// the entire IBars sequence.</param>
    /// <returns>True if the bar was found, False otherwise.</returns>
    public static bool TryFindIndexOf(this IBars target, TimeStamp targetTimeStamp, double targetClose, out int index)
    {
      if (target.TryFindTimeStampAt(targetTimeStamp, out var firstIndex, out var lastIndex))
      {
        for (var i = firstIndex; i <= lastIndex; i++)
        {
          if (target.GetClose(i) == targetClose)
          {
            index = i;
            return true;
          }
        }
      }

      index = -1;
      return false;
    }

    /// <summary>
    /// Gets a BarPositionDescriptor that can be used to search for the equivalent-position bar in another IBars sequence.
    /// </summary>
    public static BarPositionDescriptor GetBarPositionDescriptor(this IBars target, int barIndex)
    {
      var result = new BarPositionDescriptor
      {
        TimeStamp = target.GetTimeStamp(barIndex),
        Close = target.GetClose(barIndex),
        SequenceNumber = 0,
      };
      for (barIndex--; barIndex >= 0 && target.GetTimeStamp(barIndex) == result.TimeStamp; barIndex--)
        if (target.GetClose(barIndex) == result.Close)
          result.SequenceNumber++;
      return result;
    }

    /// <summary>
    /// Searches an IBars series to get the number of bars between fromIndex and toIndex that close at the given 'close' value
    /// </summary>
    public static int GetNumberOfClosesAt(this IBars target, int fromIndex, int toIndex, double close)
    {
      var count = 0;
      for (var i = fromIndex; i <= toIndex; i++)
      {
        if (target.GetClose(i) == close)
          count++;
      }

      return count;
    }

    /// <summary>
    /// Searches an IBars series between the given minIndex and maxIndex to find the index of the nth bar with the given close.
    /// Returns true if such a bar is found.
    /// If such a bar is not found, false will be returned and "index" will be set to -1 no bars with the given close were found, or
    /// the index of the last bar having the given index.
    /// </summary>
    public static bool TryGetIndexOfNthClose(this IBars target, int minIndex, int maxIndex, double close, int n, out int index)
    {
      index = -1;
      for (int numFound = 0, i = minIndex; i <= maxIndex; i++)
      {
        if (target.GetClose(i) == close)
        {
          index = i;
          if (numFound++ == n)
            return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Searches an IBars series to find the index of a bar exactly matching the given barPositionDescriptor, returning true if the exactly-matching bar was found.
    /// If bars at the given timestamp and close do exist, but not enough to reach the barPositionDescriptor.SequenceNumber, 'barIndex' will contain the index of
    /// the last bar with the right timestamp and close, even though 'false' is returned.
    /// </summary>
    /// <param name="barIndex">If the method returns true, barIndex is the index of the bar exactly matching the barPositionDescriptor.
    /// If the method returns false, barIndex will be the index of the last bar with the correct timestamp and close, if there were not enough bars existing to match barPositionDescriptor.SequenceNumber.
    /// If there were no bars with the correct timestamp and close, barIndex will be -1</param>
    public static bool TryFindBarExact(this IBars target, BarPositionDescriptor barPositionDescriptor, out int barIndex)
    {
      barIndex = -1;
      int firstIndex, lastIndex;
      if (!target.TryFindTimeStampAt(barPositionDescriptor.TimeStamp, out firstIndex, out lastIndex))
        return false;
      return target.TryGetIndexOfNthClose(firstIndex, lastIndex, barPositionDescriptor.Close, barPositionDescriptor.SequenceNumber, out barIndex);
    }

    /// <summary>
    /// Searches an IBars series to find a bar exactly matching the given barPositionDescriptor, or one bar before.
    /// Returns false if barPositionDescriptor.TimeStampLocal is before target.GetTimeStampLocal(0), otherwise returns true.
    /// <param name="barIndex">-1 if the method returns false. Otherwise it's the index of the exactly-matching bar, or the bar before.</param>
    /// <param name="isExactMatch">True if an exact match was found. False if the bar before was used.</param>
    public static bool TryFindBarExactOrOneBefore(this IBars target, BarPositionDescriptor barPositionDescriptor, out int barIndex, out bool isExactMatch)
    {
      int firstIndex, lastIndex;
      TimeStamp timeStampFound;
      if (!target.TryFindTimeStampAtOrBefore(barPositionDescriptor.TimeStamp, out timeStampFound, out isExactMatch, out firstIndex, out lastIndex))
      {
        barIndex = -1;
        return false;
      }

      if (!isExactMatch)
      {
        barIndex = lastIndex;
        return true;
      }

      isExactMatch = target.TryGetIndexOfNthClose(firstIndex, lastIndex, barPositionDescriptor.Close, barPositionDescriptor.SequenceNumber, out barIndex);
      if (isExactMatch)
        return true;
      if (barIndex >= firstIndex)
        return true;
      barIndex = lastIndex;
      return true;
    }

    public static bool TryFindBarExactOrOneAfter(this IBars target, BarPositionDescriptor barPositionDescriptor, out int barIndex, out bool isExactMatch)
    {
      int firstIndex, lastIndex;
      TimeStamp timeStampFound;
      if (!target.TryFindTimeStampAtOrAfter(barPositionDescriptor.TimeStamp, out timeStampFound, out isExactMatch, out firstIndex, out lastIndex))
      {
        barIndex = -1;
        return false;
      }

      if (!isExactMatch)
      {
        barIndex = firstIndex;
        return true;
      }

      isExactMatch = target.TryGetIndexOfNthClose(firstIndex, lastIndex, barPositionDescriptor.Close, barPositionDescriptor.SequenceNumber, out barIndex);
      if (isExactMatch)
        return true;
      barIndex = Math.Min(target.Count - 1, lastIndex + 1);
      return true;
    }

    public static bool TryFindBar(this IBars target, TimeStamp timeStampLocal, double close, int sequenceNumber, out int index)
    {
      index = -1;

      int firstIndex, lastIndex;
      if (!target.TryFindTimeStampAt(timeStampLocal, out firstIndex, out lastIndex))
        return false;

      var numFound = 0;
      for (var i = firstIndex; i <= lastIndex; i++)
      {
        if (target.GetClose(i) == close)
        {
          numFound++;
          if (numFound == sequenceNumber + 1)
          {
            index = i;
            return true;
          }
        }
      }

      return false;
    }
  }
}
