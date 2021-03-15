// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Engines.ApexPattern
{
  using System;
  using static System.Math;

  public sealed record ApexPatternEngineSettings : EngineSettings
  {
    private int _eTicks = 2;
    private int _xTicks = 1;
    private int _reversalOffsetTicks = 1;

    /// <summary>
    /// The number of ticks that a bar must rise above the high of a P bar to
    /// form an E.
    /// </summary>
    public int ETicks
    {
      get { return _eTicks; }
      init { _eTicks = Max(1, value); }
    }

    /// <summary>
    /// The number of ticks that a bar must rise above the high of the A bar to
    /// form an X. Note that this number is allowed to be negative, and would
    /// allow for a series of consecutive lower green apexes.
    /// </summary>
    public int XTicks
    {
      get { return _xTicks; }
      init { _xTicks = value; } // no range or sanity checking here
    }

    public int ReversalOffsetTicks
    {
      get { return _reversalOffsetTicks; }
      init { _reversalOffsetTicks = Max(0, value); }
    }
  }
}
