// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Bars
{
  using System.Collections.Generic;

  /// <summary>
  /// Use this class to sequentially read all bars in a series from beginning to
  /// end. This object is intended to be used by a single thread reading bars
  /// that are being built by another thread. In other words, it's not
  /// thread-safe to be read by multiple threads.
  /// </summary>
  public sealed class BarsReader
  {
    private readonly IBars _bars;

    public BarsReader(IBars bars)
    {
      _bars = bars;

      // Since the bars object may contain no bars when this reader is
      // constructed, we always initialize the reader by NOT pointing to the
      // first bar in the series. The user will have to call "MoveToNextBar()"
      // or similar to be able to access the first bar in the series.
      CurrentBar = null;
      CurrentBarIndex = -1;
    }

    /// <summary>
    /// The total number of bars presently in the bar series. This value could
    /// increase over time as bars are added to the bar series.
    /// </summary>
    public int Count => _bars.Count;

    /// <summary>
    /// The bar that the reader is currently pointing to. Note that this value
    /// is NULL when the iterator is first constructed.
    /// </summary>
    public IBar? CurrentBar { get; private set; }

    /// <summary>
    /// The index of the bar that the reader is currently pointing to. Note that
    /// this value is -1 when the reader is first constructed.
    /// </summary>
    public int CurrentBarIndex { get; private set; }

    /// <summary>
    /// Moves directly to the last bar of the series without iterating through
    /// the bars in between. Returns true if the pointer was moved. Returns
    /// false if the pointer was already at the end of the series or if the
    /// series contains no bars.
    /// </summary>
    public bool MoveToEnd()
    {
      var lastIndex = _bars.Count - 1;
      if (lastIndex >= 0 && lastIndex > CurrentBarIndex)
      {
        CurrentBarIndex = lastIndex;
        CurrentBar = _bars[lastIndex];
        return true;
      }

      return false;
    }

    /// <summary>
    /// Moves to the next bar of the series. Returns true if the pointer was
    /// moved. Returns false if the pointer was already at the end of the series
    /// or if the series contains no bars.
    /// </summary>
    public bool MoveToNextBar()
    {
      var lastIndex = _bars.Count - 1;
      if (lastIndex >= 0 && lastIndex > CurrentBarIndex)
      {
        CurrentBarIndex++;
        CurrentBar = _bars[CurrentBarIndex];
        return true;
      }

      return false;
    }

    /// <summary>
    /// Iterates through all bars in the series to the end. First bar returned
    /// in the enumeration is the bar immediately after CurrentBar.
    /// </summary>
    /// <remarks>If you just want to get to the end of the bar series, use the
    /// "MoveToEnd" method instead. Also note that this method won't move
    /// through to the last bar unless your code actually READS the results of
    /// the interation. Eg, barsReader.ReadToEnd() won't run the iterator.
    /// barsReader.ReadToEnd().Count() will.</remarks>
    public IEnumerable<IBar> ReadToEnd()
    {
      var lastIndex = _bars.Count - 1;
      if (lastIndex >= 0)
      {
        while (CurrentBarIndex < lastIndex)
        {
          CurrentBarIndex++;
          CurrentBar = _bars[CurrentBarIndex];
          yield return CurrentBar;
        }
      }
    }
  }
}
