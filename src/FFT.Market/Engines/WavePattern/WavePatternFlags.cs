// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Engines.WavePattern
{
  /// <summary>
  /// This flags object is populated with flags set according to events that
  /// occured on the last bar.
  /// </summary>
  public sealed class WavePatternFlags
  {
    /// <summary>
    /// Raised when a new apex is created and stored as CurrentTrendApex.
    /// </summary>
    public const uint NewA = 1 << 0;

    /// <summary>
    /// Raised when the CurrentTrendApex shifts its A.
    /// </summary>
    public const uint ShiftedA = 1 << 1;

    /// <summary>
    /// Raised when the CurrentTrendApex creates a P bar.
    /// </summary>
    public const uint FormedP = 1 << 2;

    /// <summary>
    /// Raised when the CurrentTrendApex shifts its P.
    /// </summary>
    public const uint ShiftedP = 1 << 3;

    /// <summary>
    /// Raised when the current trend apex sets or adjusts an ETrigger value.
    /// </summary>
    public const uint SetOrAdjustedETriggervalue = 1 << 4;

    /// <summary>
    /// Raised when the CurrentTrendApex creates an E bar.
    /// </summary>
    public const uint FormedE = 1 << 5;

    /// <summary>
    /// Raised when the CurrentTrendApex's E fails (will coincide with
    /// "ShiftedP" flag).
    /// </summary>
    public const uint FailedE = 1 << 6;

    /// <summary>
    /// Raised when the CurrentTrendApex creates an X.
    /// </summary>
    public const uint FormedX = 1 << 7;

    /// <summary>
    /// Raised when the CurrentReversalApex completes, above the red powerline
    /// (coincides with NewA because a new apex is created and set as
    /// CurrentTrendApex).
    /// </summary>
    public const uint SwitchedDirectionUp = 1 << 8;

    /// <summary>
    /// Raised when the CurrentReversalApex completes, below the green powerline
    /// (coincides with NewA because a new apex is created and set as
    /// CurrentTrendApex).
    /// </summary>
    public const uint SwitchedDirectionDown = 1 << 9;
  }
}
