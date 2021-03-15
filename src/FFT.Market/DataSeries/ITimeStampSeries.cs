// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.DataSeries
{
  using FFT.TimeStamps;

  /// <summary>
  /// Used to mark an object as having a series of timestamps accessible by index.
  /// For the extension methods to work properly, each timestamp is MUST be greater than or equal to the timestamp before.
  /// </summary>
  public interface ITimeStampSeries
  {
    /// <summary>
    /// Gets the number of data points in the series.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets the timestamp at the index specified.
    /// </summary>
    TimeStamp GetTimeStamp(int index);
  }

  public static class ITimeStampSeriesExtensionMethods
  {
    private const int MAX_PLAIN_ITERATION_SIZE = 32;

    /// <summary>
    /// Searches the time series for the location containing the given <paramref
    /// name="time"/>.
    /// </summary>
    /// <param name="series">The series to be searched.</param>
    /// <param name="time">The timestamp we are searching for.</param>
    /// <param name="firstIndex">If the search is successful, is set to the
    /// index of the first item at the time.</param>
    /// <param name="lastIndex">If the search is successful, is set to the index
    /// of the last item at the time.</param>
    /// <returns>True if the search is successful, False otherwise.</returns>
    public static bool TryFindTimeStampAt(this ITimeStampSeries series, in TimeStamp time, out int firstIndex, out int lastIndex)
    {
      if (!series.TryFindTimeStampAtOrBefore(time, out _, out var isExactMatch, out firstIndex, out lastIndex))
        return false;

      if (!isExactMatch)
        return Fail(out _, out _, out firstIndex, out lastIndex);

      return true;
    }

    /// <summary>
    /// Searches the time series for the begin and end locations for the time
    /// period between <paramref name="firstTime"/> and <paramref
    /// name="lastTime"/>.
    /// </summary>
    /// <returns>True if indexes were found, false otherwise.</returns>
    /// <param name="series">The series to be searched.</param>
    /// <param name="firstTime">The beginning of the time period that we are
    /// searching the series for.</param>
    /// <param name="lastTime">The end of the time period that we are searching
    /// the series for.</param>
    /// <param name="firstIndex">If the search is successful, is set to the
    /// first index of the series containing the given period of time.</param>
    /// <param name="lastIndex">If the search is successful, is set to the last
    /// index of the series containing the given period of time.</param>
    public static bool TryFindIndexesBetween(this ITimeStampSeries series, in TimeStamp firstTime, in TimeStamp lastTime, out int firstIndex, out int lastIndex)
    {
      // Find the location of the timestamp at or immediately after
      // "firstTargetTimeStamp" Note that, since we are immediately using the
      // public "FindTimeStamp" methods, silliness checking is being done in
      // those methods. So there's no need for us to do our own silliness
      // checking here.
      if (series.TryFindTimeStampAtOrAfter(firstTime, out var timeFound, out _, out var _firstIndex, out var _lastIndex))
      {
        // Check that there are actually timestamps between firstTargetTimeStamp
        // and lastTargetTimeStamp
        if (timeFound <= lastTime)
        {
          // save the start of the location to our own output variable
          firstIndex = _firstIndex;

          // now find the location of the timestamp at or immediately before "lastTargetTimeStamp"
          if (series.TryFindTimeStampAtOrBefore(lastTime, out timeFound, out _, out _firstIndex, out _lastIndex))
          {
            // save the end of the location to our own output variable
            lastIndex = _lastIndex;

            // exit, indicating that the search was successful
            return true;
          }
        }
      }

      // The searches were not successful, so set output variables to fail values and return false
      return Fail(out _, out _, out firstIndex, out lastIndex);
    }

    /// <summary>
    /// Searches the time series for the location containing a timestamp equal
    /// to or immediately before the targetTimeStamp.
    /// </summary>
    /// <param name="series">The time series to be searched</param>
    /// <param name="time">The timestamp being searched for</param>
    /// <param name="timeFound">The timestamp that was found. It could be equal
    /// to the targetTimeStamp, or if targetTimeStamp doesn't exist in the time
    /// series, it is the timestamp found immediately before
    /// targetTimeStamp</param>
    /// <param name="isExactMatch">True if the targetTimeStamp was found. False
    /// if the timestamp immediately preceeding it was found instead.</param>
    /// <param name="firstIndex">The first index of the range of datapoints
    /// containing the timestamp that was found</param>
    /// <param name="lastIndex">The last index of the range of datapoints
    /// containing the timestamp that was found</param>
    /// <returns>Returns true if a timestamp is found, false otherwise</returns>
    public static bool TryFindTimeStampAtOrBefore(this ITimeStampSeries series, in TimeStamp time, out TimeStamp timeFound, out bool isExactMatch, out int firstIndex, out int lastIndex)
    {
      if (series.Count == 0)
        return Fail(out timeFound, out isExactMatch, out firstIndex, out lastIndex);

      var xR = series.Count - 1;
      var timeL = series.GetTimeStamp(0);
      var timeR = series.GetTimeStamp(xR);

      if (timeL > time)
        return Fail(out timeFound, out isExactMatch, out firstIndex, out lastIndex);

      FindTimeStampAtOrBefore(series, time, 0, xR, timeL, timeR, out timeFound, out isExactMatch, out firstIndex, out lastIndex);
      return true;
    }

    /// <summary>
    /// Searches the time series for the location containing a timestamp equal
    /// to or immediately after the targetTimeStamp.
    /// </summary>
    /// <param name="series">The time series to be searched</param>
    /// <param name="time">The timestamp being searched for</param>
    /// <param name="timeFound">The timestamp that was found. It could be equal
    /// to the targetTimeStamp, or if targetTimeStamp doesn't exist in the time
    /// series, it is the timestamp found immediately after
    /// targetTimeStamp</param>
    /// <param name="isExactMatch">True if the targetTimeStamp was found. False
    /// if the timestamp immediately after it was found instead.</param>
    /// <param name="firstIndex">The first index of the range of datapoints
    /// containing the timestamp that was found</param>
    /// <param name="lastIndex">The last index of the range of datapoints
    /// containing the timestamp that was found</param>
    /// <returns>Returns true if a timestamp is found, false otherwise</returns>
    public static bool TryFindTimeStampAtOrAfter(this ITimeStampSeries series, TimeStamp time, out TimeStamp timeFound, out bool isExactMatch, out int firstIndex, out int lastIndex)
    {
      if (series.Count == 0)
        return Fail(out timeFound, out isExactMatch, out firstIndex, out lastIndex);

      var xR = series.Count - 1;
      var timeL = series.GetTimeStamp(0);
      var timeR = series.GetTimeStamp(xR);

      if (timeR < time)
        return Fail(out timeFound, out isExactMatch, out firstIndex, out lastIndex);

      FindTimeStampAtOrAfter(series, time, 0, xR, timeL, timeR, out timeFound, out isExactMatch, out firstIndex, out lastIndex);
      return true;
    }

    /// <summary>
    /// Private method that does the real work.
    /// This method is only called after "silliness checking" has been done and we know
    /// that a solution definitely exists.
    /// Here are the conditions assumed you ensured are satisfied before calling this method:
    ///   1. series.Count > 0
    ///   2. xMin >= 0
    ///   3. xMax < series.Count
    ///   4. XMax > xMin
    ///   4. timeStamp at xMin <= time
    ///   5. timeStamp at xMax >= time
    /// </summary>
    static void FindTimeStampAtOrBefore(ITimeStampSeries series, in TimeStamp time, int xL, int xR, TimeStamp timeL, TimeStamp timeR, out TimeStamp timeFound, out bool isExactMatch, out int firstIndex, out int lastIndex)
    {
      if (timeL == time)
      {
        timeFound = timeL;
        isExactMatch = true;
        firstIndex = xL;
        lastIndex = TraverseToLastOccurrence(series, timeFound, xR, xL);
        return;
      }

      if (timeR <= time)
      {
        timeFound = timeR;
        isExactMatch = timeFound == time;
        lastIndex = xR;
        firstIndex = TraverseToFirstOccurrence(series, timeFound, xL, xR);
        return;
      }

      if (xR - xL <= MAX_PLAIN_ITERATION_SIZE)
      {
        do
        {
          xR--;
          timeR = series.GetTimeStamp(xR);
        }
        while (timeR > time);
        timeFound = timeR;
        isExactMatch = timeFound == time;
        lastIndex = xR;
        firstIndex = TraverseToFirstOccurrence(series, timeFound, xL, xR);
        return;
      }

      var rGuess = (double)(time.TicksUtc - timeL.TicksUtc) / (double)(timeR.TicksUtc - timeL.TicksUtc);
      var xGuess = xL + (int)(rGuess * (xR - xL));
      if (xGuess <= xL) xGuess = xL + 1;
      else if (xGuess >= xR) xGuess = xR - 1;
      var timeGuess = series.GetTimeStamp(xGuess);
      var step = (xR - xL) / 10;

      while (timeGuess > time && xR - xL > MAX_PLAIN_ITERATION_SIZE)
      {
        xR = TraverseToFirstOccurrence(series, timeGuess, xL + 1, xGuess) - 1;
        timeR = series.GetTimeStamp(xR);
        xGuess = xR - step;
        if (xGuess <= xL) xGuess = xL + 1;
        timeGuess = series.GetTimeStamp(xGuess);
      }

      step = (xR - xL) / 10;

      while (timeGuess < time && xR - xL > MAX_PLAIN_ITERATION_SIZE)
      {
        if (timeGuess == timeL)
        {
          xGuess = TraverseToLastOccurrence(series, timeGuess, xR - 1, xGuess) + 1;
          timeGuess = series.GetTimeStamp(xGuess);
          if (timeGuess > time)
          {
            timeFound = timeL;
            isExactMatch = timeFound == time;
            lastIndex = xGuess - 1;
            firstIndex = xL;
            return;
          }
        }
        else
        {
          xL = TraverseToFirstOccurrence(series, timeGuess, xL, xGuess);
          timeL = timeGuess;
          xGuess = xL + step;
          if (xGuess >= xR) xGuess = xR - 1;
          timeGuess = series.GetTimeStamp(xGuess);
        }
      }

      if (timeGuess == time)
      {
        isExactMatch = true;
        timeFound = time;
        firstIndex = TraverseToFirstOccurrence(series, time, xL, xGuess);
        lastIndex = TraverseToLastOccurrence(series, time, xR, xGuess);
        return;
      }

      if (timeGuess > time)
      {
        xR = TraverseToFirstOccurrence(series, timeGuess, xL + 1, xGuess) - 1;
        timeR = series.GetTimeStamp(xR);
      }

      FindTimeStampAtOrBefore(series, time, xL, xR, timeL, timeR, out timeFound, out isExactMatch, out firstIndex, out lastIndex);
    }

    /// <summary>
    /// Private method that does the real work.
    /// This method is only called after "silliness checking" has been done and we know
    /// that a solution definitely exists.
    /// Here are the conditions assumed you ensured are satisfied before calling this method:
    ///   1. series.Count > 0
    ///   2. xL >= 0
    ///   3. xR < series.Count
    ///   4. xR > xL
    ///   4. timeStamp at xL <= time
    ///   5. timeStamp at xR >= time
    /// </summary>
    private static void FindTimeStampAtOrAfter(ITimeStampSeries series, in TimeStamp time, int xL, int xR, TimeStamp timeL, TimeStamp timeR, out TimeStamp timeFound, out bool isExactMatch, out int firstIndex, out int lastIndex)
    {
      if (timeL >= time)
      {
        timeFound = timeL;
        isExactMatch = timeFound == time;
        firstIndex = xL;
        lastIndex = TraverseToLastOccurrence(series, timeFound, xR, xL);
        return;
      }

      if (timeR == time)
      {
        timeFound = timeR;
        isExactMatch = true;
        lastIndex = xR;
        firstIndex = TraverseToFirstOccurrence(series, timeFound, xL, xR);
        return;
      }

      if (xR - xL <= MAX_PLAIN_ITERATION_SIZE)
      {
        do
        {
          xL++;
          timeL = series.GetTimeStamp(xL);
        } while (timeL < time);
        timeFound = timeL;
        isExactMatch = timeFound == time;
        firstIndex = xL;
        lastIndex = TraverseToLastOccurrence(series, timeFound, xR, xL);
        return;
      }

      var rGuess = (double)(time.TicksUtc - timeL.TicksUtc) / (double)(timeR.TicksUtc - timeL.TicksUtc);
      var xGuess = xL + (int)(rGuess * (xR - xL));
      if (xGuess <= xL) xGuess = xL + 1;
      else if (xGuess >= xR) xGuess = xR - 1;
      var timeGuess = series.GetTimeStamp(xGuess);
      var step = (xR - xL) / 10;

      while (timeGuess < time && xR - xL > MAX_PLAIN_ITERATION_SIZE)
      {
        xL = TraverseToLastOccurrence(series, timeGuess, xR - 1, xGuess) + 1;
        timeL = series.GetTimeStamp(xL);
        xGuess = xL + step;
        if (xGuess >= xR) xGuess = xR - 1;
        timeGuess = series.GetTimeStamp(xGuess);
      }

      step = (xR - xL) / 10;

      while (timeGuess > time && xR - xL > MAX_PLAIN_ITERATION_SIZE)
      {
        if (timeGuess == timeR)
        {
          xGuess = TraverseToFirstOccurrence(series, timeGuess, xL + 1, xGuess) - 1;
          timeGuess = series.GetTimeStamp(xGuess);
          if (timeGuess < time)
          {
            firstIndex = xGuess + 1;
            lastIndex = xR;
            timeFound = timeR;
            isExactMatch = timeFound == time;
            return;
          }
        }
        else
        {
          xR = TraverseToLastOccurrence(series, timeGuess, xR, xGuess);
          timeR = timeGuess;
          xGuess = xR - step;
          if (xGuess <= xL) xGuess = xL + 1;
          timeGuess = series.GetTimeStamp(xGuess);
        }
      }

      if (timeGuess == time)
      {
        isExactMatch = true;
        timeFound = time;
        firstIndex = TraverseToFirstOccurrence(series, time, xL, xGuess);
        lastIndex = TraverseToLastOccurrence(series, time, xR, xGuess);
        return;
      }

      if (timeGuess < time)
      {
        xL = TraverseToLastOccurrence(series, timeGuess, xR - 1, xGuess) + 1;
        timeL = series.GetTimeStamp(xL);
      }

      FindTimeStampAtOrAfter(series, time, xL, xR, timeL, timeR, out timeFound, out isExactMatch, out firstIndex, out lastIndex);
    }

    /// <summary>
    /// Traverses backwards along the time series looking for the first occurrence of the targetTimeStamp.
    /// Traversal stops at xMin.
    /// Method assumes:
    ///   The timestamp at 'index' is equal to targetTimeStamp.
    ///        It's the caller's responsibility to make sure it is or you will get 'unexpected' results.
    /// </summary>
    /// <returns>The index of the first occurrence of the targetTimeStamp</returns>
    private static int TraverseToFirstOccurrence(ITimeStampSeries series, in TimeStamp time, int xMin, int index)
    {
      while (index > xMin && series.GetTimeStamp(index - 1) == time)
        index--;
      return index;
    }

    /// <summary>
    /// Traverses forwards along the time series looking for the last occurrence of the targetTimeStamp.
    /// Traversal stops at xMax.
    /// Method assumes:
    ///   The timestamp at 'index' is equal to targetTimeStamp.
    ///        It's the caller's responsibility to make sure it is or you will get 'unexpected' results.
    /// </summary>
    /// <returns>The index of the last occurrence of the targetTimeStamp</returns>
    private static int TraverseToLastOccurrence(ITimeStampSeries series, in TimeStamp time, int xMax, int index)
    {
      while (index < xMax && series.GetTimeStamp(index + 1) == time)
        index++;
      return index;
    }

    /// <summary>
    /// Boiler plate code for setting up output variables when the search fails.
    /// </summary>
    private static bool Fail(out TimeStamp timeFound, out bool isExactMatch, out int firstIndex, out int lastIndex)
    {
      timeFound = TimeStamp.MinValue;
      isExactMatch = false;
      firstIndex = -1;
      lastIndex = -1;
      return false;
    }
  }
}
