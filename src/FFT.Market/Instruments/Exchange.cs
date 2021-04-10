// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Instruments
{
  using System.Diagnostics;

  [DebuggerDisplay("{LongName}")]
  public record Exchange
  {
    public Exchange() { }

    public Exchange(string shortName, string longName)
      => (ShortName, LongName) = (shortName, longName);

    public string ShortName { get; init; }
    public string LongName { get; init; }
  }
}
