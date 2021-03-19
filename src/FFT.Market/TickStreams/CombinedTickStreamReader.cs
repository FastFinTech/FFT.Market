// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.TickStreams
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using FFT.Market.Ticks;
  using FFT.TimeStamps;

  /// <summary>
  /// This is the sexiest damn class in the world.
  /// It combines several tick streams into a single tick stream,
  /// where each tick is ordered by its TimeStamp property.
  /// Read on to see how to create code that even the children will enjoy.
  /// </summary>
  public sealed class CombinedTickStreamReader : ITickStreamReader
  {
    // all the input streams that will be combined and ordered by time.
    private readonly ITickStreamReader[] _readers;

    // storage for the "peek tick"
    private Tick? _peekTick;

    ////////////////////////////////////////////////////////////////////////////////////////
    // So here's the magic that makes this class so goddam sexy.
    // An enumerator is created that can read all the ticks from the input streams, ordered
    // by time. However, we know that the enumerator will run out of ticks when it gets
    // to the end of the streams. But ticks are continually being added to the streams,
    // so we need a way to "reset" the enumerator. Let's see how it's done:
    ////////////////////////////////////////////////////////////////////////////////////////

    // storage for the enumerator object
    private IEnumerator<Tick> _enumerator;

    public CombinedTickStreamReader(params ITickStreamReader[] readers)
    {
      _readers = readers.ToArray();

      // Don't forget to seed the first enumerator so we don't need to add
      // null checks inside the "ExtractNext" method.
      _enumerator = GetEnumerator();
    }

    public long BytesRemaining
    {
      get
      {
        var result = 0L;
        foreach (var reader in _readers)
          result += reader.BytesRemaining;
        return result;
      }
    }

    public TickStreamInfo Info
      => throw new NotSupportedException("TickStreamInfo is not available for combined tick reader since it combines multiple tick streams.");

    /// <summary>
    /// Reads the next tick from the combined tick stream.
    /// Returns null when no more ticks are available. After 
    /// more ticks have been added to any of the input streams,
    /// the method will again begin to return not-null ticks.
    /// After a tick has been read once from this method, it is considered
    /// "read" and will not be available again.
    /// </summary>
    public Tick? ReadNext()
    {
      var next = _peekTick ?? ExtractNext();
      _peekTick = null;
      return next;
    }

    /// <summary>
    /// "Peeks" ahead to return the next tick available in the tick stream. 
    /// Returns null if no more ticks are available. After more ticks have been added
    /// to any of the input streams, the method will again being to return not-null ticks.
    /// Repeated calls to this method will continually return the same tick until that tick has
    /// been consumed by the "ReadNext" method.
    /// </summary>
    public Tick? PeekNext()
      => _peekTick ??= ExtractNext();

    /// <summary>
    /// Reads ticks from the sexy-ass enumerator created in the GetEnumerator method,
    /// returning null if no more ticks are available.
    /// Handles the logic required for recycling the enumerator.
    /// </summary>
    private Tick? ExtractNext()
    {
      // If there's a tick available in the enumerator, return it.
      if (_enumerator.MoveNext())
      {
        return _enumerator.Current;
      }
      else
      {
        // The enumerator either had no ticks or has reached the end of its run before new ticks were added
        // to the input streams. So we'll get a new enumerator and see if any new ticks have been added.
        _enumerator = GetEnumerator();
        // see if there's a tick available in the new enumerator and return it if there is.
        if (_enumerator.MoveNext())
        {
          return _enumerator.Current;
        }

        // Even the new enumerator doesn't have ticks, so we return null.
        // Next time this method is called, the new (and dead enumerator) will again be 
        // asked to "MoveNext" even though it's finished with its enumeration. That doesn't hurt;
        // the code will just move down to the point where a new enumerator is again created.
        return null;
      }
    }

    /// <summary>
    /// Creates an enumerator that will last until the end of the ticks that are
    /// currently available in the input tick streams.
    /// </summary>
    private IEnumerator<Tick> GetEnumerator()
    {
      // This gets the minimum timestamp of the first tick of each of the input
      // streams. if there are no ticks available in any of the input streams,
      // 'untilTime' will be TimeStamp.MaxValue
      var until = NextUntilTime();

      // Most likely there are just a few ticks available with the given minimum
      // timestamp, so lets go ahead and extract them all.
      while (until < TimeStamp.MaxValue)
      {
        foreach (var reader in _readers)
        {
          foreach (var tick in reader.ReadUntil(until))
          {
            yield return tick;
          }
        }

        // now that we have extracted all ticks with the minimum timestamp, lets
        // start again by getting the next timestamp to be extracted.
        until = NextUntilTime();
      }

      // Now all available ticks have been read out of the streams (until ==
      // TimeStamp.MaxValue) this enumerator is done. It needs to be replaced.
      // When new ticks are added to the input streams, newly-created
      // enumerators will be able to get them.

      // And that's how you write sexy-ass code.
    }

    private TimeStamp NextUntilTime()
    {
      var result = TimeStamp.MaxValue;
      foreach (var reader in _readers)
      {
        var readerNext = reader.GetTimestampNextTickOrMaxValue();
        if (readerNext < result)
          result = readerNext;
      }

      return result;
    }
  }
}
