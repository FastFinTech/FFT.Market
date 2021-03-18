// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Engines.WavePattern
{
  public enum WaveStates
  {
    /// <summary>
    /// Apex is in this state when it is first created and remains in this
    /// state while the A is shifted along from one high to another until it
    /// is time to shift state to FormedP
    /// </summary>
    FormedA = 0,

    /// <summary>
    /// Apex flips to this state in 2 circumstances: 1. From FormedA, when a
    /// bar forms with lower high than the A bar AND lower low than the A
    /// bar. 2. From FormedE, when a bar forms with a lower low (by one
    /// tick) than the previous P bar.
    /// </summary>
    FormedP = 1,

    /// <summary>
    /// Apex flips to this state from FormedP when a bar forms with a high
    /// x-ticks above the high of the PBar. Sometimes, this state may be
    /// skipped entirely as the Apex transitions immediately into FormedX.
    /// (Of course the E will still be drawn when this state is skipped)
    /// </summary>
    FormedE = 2,

    /// <summary>
    /// Apex flips to this state (from FormedP or FormedE) when a bar forms
    /// with a high 1 tick above the high of the A Bar.
    /// </summary>
    FormedX = 3,
  }
}
