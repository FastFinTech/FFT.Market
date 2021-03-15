// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Engines.ApexPattern
{
  using System.Collections.Immutable;
  using FFT.Market;

  public interface IAPEX
  {
    Direction Direction { get; }
    ApexStates State { get; }
    IndexAndValue A { get; }
    IndexAndValue? P { get; }
    IndexAndValue? E { get; }
    IndexAndValue? X { get; }
    ImmutableList<IndexAndValue> FailedPs { get; }
    ImmutableList<IndexAndValue> FailedEs { get; }

    /// <summary>
    /// ETriggerValue is updated when a P is formed or shifted.
    /// ETriggerValue is null until that time.
    /// </summary>
    double? ETriggerValue { get; }

    /// <summary>
    /// XTriggerValue is updated when a P is formed or shifted.
    /// XTriggerValue is null until that time.
    /// </summary>
    double? XTriggerValue { get; }

    /// <summary>
    /// The last (right-most) bar index that the powerline should be drawn to.
    /// It's here for display purposes and possibly might be used in future in some algo.
    /// </summary>
    int? LastIndexOfPowerline { get; }

    /// <summary>
    /// The minimum high of any bar included within this apex (all the way from the A to the X)
    /// This is required for determining whether a completed reversal apex has triggered a reversal signal.
    /// Note that because the A shifts, only the values of the final A bar are used
    /// previous A bars are not included in the aggregation of this value.
    /// </summary>
    double MinHigh { get; }

    /// <summary>
    /// The maximum low of any bar included within this apex (all the way from the A to the X)
    /// This is required for determining whether a completed reversal apex has triggered a reversal signal.
    /// Note that because the A shifts, only the values of the final A bar are used ...
    /// previous A bars are not included in the aggregation of this value.
    /// </summary>
    double MaxLow { get; }
  }
}
