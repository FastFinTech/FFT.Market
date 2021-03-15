// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Bars
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using FFT.Market.TickStreams;
  using FFT.TimeStamps;

  public static class IBarsReaderExtensions
  {
    /// <summary>
    /// Reads all available bars from the <paramref name="reader"/>.
    /// </summary>
    public static IEnumerable<IBar> ReadRemaining(this IBarsReader reader)
    {
      while (reader.ReadNext() is IBar bar)
      {
        yield return bar;
      }
    }

    /// <summary>
    /// Reads all ticks from the <paramref name="reader"/> with TimeStamp
    /// property less than or equal to <paramref name="until"/>.
    /// </summary>
    public static IEnumerable<IBar> ReadUntil(this IBarsReader reader, TimeStamp until)
    {
      while (reader.GetTimestampNextBarOrMaxValue() <= until)
        yield return reader.ReadNext();
    }

    /// <summary>
    /// Reads all ticks from the IBarsReader with TimeStamp property less than
    /// "until".
    /// </summary>
    public static IEnumerable<IBar> ReadUntilJustBefore(this IBarsReader reader, TimeStamp until)
    {
      while (reader.GetTimestampNextBarOrMaxValue() < until)
        yield return reader.ReadNext();
    }

    /// <summary>
    /// Sets up the bar stream reader so that the next bar it returns via
    /// ReadNext() will have a timestampLocal property >= until.
    /// </summary>
    public static void MoveUntil(this IBarsReader reader, TimeStamp until)
    {
      // don't forget to call Count or something similar to actually run the enumerator
      reader.ReadUntilJustBefore(until).Count();
    }

    /// <summary>
    /// Gets the TimeStamp property of the "PeekBar" in the <paramref
    /// name="reader"/>. Returns TimeStamp.MaxValue if no peek tick is
    /// available.
    /// </summary>
    public static TimeStamp GetTimestampNextBarOrMaxValue(this IBarsReader reader)
    {
      return reader.PeekNext()?.TimeStamp ?? TimeStamp.MaxValue;
    }

    public static string Dump(this ITickStreamReader reader, TimeZoneInfo timeZone)
    {
      var sb = new StringBuilder();
      var converter = ConversionIterators.FromTimeStamp(timeZone);
      foreach (var tick in reader.ReadRemaining())
      {
        var dateTime = converter.GetDateTime(tick.TimeStamp);
        sb.AppendLine($"{dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}\t{tick.Price}\t{tick.Volume}");
      }

      return sb.ToString();
    }
  }
}
