// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.TickStreams
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using FFT.Market.Ticks;
  using FFT.TimeStamps;

  public interface ITickStreamReader
  {
    TickStreamInfo Info { get; }

    /// <summary>
    /// Use this property for reporting progress to the user. Don't use it in
    /// processing code, because it's inefficient. Also, it will return zero
    /// when the peek tick has been read from the byte buffer but not yet
    /// consumed by the ReadNext() method.
    /// </summary>
    long BytesRemaining { get; }

    /// <summary>
    /// Returns the next tick in the stream. Returns null if the end of the stream has been reached.
    /// Note that if the stream is live and ticks are being continually added, repeated calls to
    /// "ReadTick" will return null and then return actual ticks after more have been added.
    /// </summary>
    Tick? ReadNext();

    /// <summary>
    /// Returns the next tick in the stream that has not been officially "read". Repeated calls to
    /// "PeekNext" will return the same tick until it has been officially "read" using "ReadNext".
    /// Returns null if the end of the stream has been reached, but repeated calls will return actual
    /// ticks after more ticks have been written to the stream.
    /// </summary>
    Tick? PeekNext();
  }

  public static class ITickStreamReaderExtensions
  {
    /// <summary>
    /// Reads all available ticks from the ITickStreamReader.
    /// </summary>
    public static IEnumerable<Tick> ReadRemaining(this ITickStreamReader reader)
    {
      while (reader.ReadNext() is Tick tick)
      {
        yield return tick;
      }
    }

    /// <summary>
    /// Reads all ticks from the ITickStreamReader with TimeStamp property less
    /// than or equal to <paramref name="until"/>.
    /// </summary>
    public static IEnumerable<Tick> ReadUntil(this ITickStreamReader reader, TimeStamp until)
    {
      while (reader.GetTimestampNextTickOrMaxValue() <= until)
        yield return reader.ReadNext()!;
    }

    /// <summary>
    /// Reads all ticks from the ITickStreamReader with TimeStamp property less
    /// than <paramref name="until"/>.
    /// </summary>
    public static IEnumerable<Tick> ReadUntilJustBefore(this ITickStreamReader reader, TimeStamp until)
    {
      while (reader.GetTimestampNextTickOrMaxValue() < until)
        yield return reader.ReadNext()!;
    }

    /// <summary>
    /// Sets up the tick stream reader so that the next tick it returns via
    /// ReadNext() will have a timestampLocal property >= until.
    /// </summary>
    public static void MoveUntil(this ITickStreamReader reader, TimeStamp until)
    {
      // don't forget to call Count or something similar to actually run the
      // enumerator.
      reader.ReadUntilJustBefore(until).Count();
    }

    /// <summary>
    /// Gets the TimeStamp property of the "PeekTick" in the ITickStreamReader.
    /// Returns TimeStamp.MaxValue if no peek tick is available.
    /// </summary>
    public static TimeStamp GetTimestampNextTickOrMaxValue(this ITickStreamReader reader)
    {
      var peekTick = reader.PeekNext();
      return peekTick is null ? TimeStamp.MaxValue : peekTick.TimeStamp;
    }

    public static string Dump(this ITickStreamReader reader, TimeZoneInfo timeZone)
    {
      var sb = new StringBuilder();
      var converter = ConversionIterators.FromTimeStamp(timeZone);
      foreach (var tick in reader.ReadRemaining())
      {
        sb.AppendLine($"{converter.GetDateTime(tick.TimeStamp).ToString("yyyy-MM-dd HH:mm:ss.fff")}\t{tick.Price}\t{tick.Volume}");
      }

      return sb.ToString();
    }
  }
}
