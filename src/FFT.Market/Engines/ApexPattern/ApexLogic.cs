// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Engines.ApexPattern
{
  using System;
  using System.Collections.Generic;
  using System.Collections.Immutable;
  using FFT.Market.Bars;
  using static System.Math;

  public class ApexLogic : IAPEX
  {
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1502 // Element should not be on a single line

    private IBars bars;
    private double eDistanceInPoints;
    private double xDistanceInPoints;

    private int barIndex;
    private int barIndexOfPreviousTick;
    private ApexPatternFlags Flags;

    private ApexLogic() { }

    public Direction Direction { get; private set; }
    public ApexStates State { get; private set; }
    public IndexAndValue A { get; private set; }
    public IndexAndValue? P { get; private set; }
    public IndexAndValue? E { get; private set; }
    public IndexAndValue? X { get; private set; }
    public ImmutableList<IndexAndValue> FailedPs { get; private set; } = ImmutableList<IndexAndValue>.Empty;
    public ImmutableList<IndexAndValue> FailedEs { get; private set; } = ImmutableList<IndexAndValue>.Empty;
    public double? ETriggerValue { get; private set; }
    public double? XTriggerValue { get; private set; }
    public double MinHigh { get; private set; }
    public double MaxLow { get; private set; }
    public int? LastIndexOfPowerline { get; private set; }

    private double CurrentHigh => bars.GetHigh(barIndex);
    private double PreviousHigh => bars.GetHigh(barIndex - 1);
    private double CurrentLow => bars.GetLow(barIndex);
    private double PreviousLow => bars.GetLow(barIndex - 1);

    public static ApexLogic Create(Direction direction, IBars bars, int barIndex, double eDistanceInPoints, double xDistanceInPoints)
    {
      var currentHigh = bars.GetHigh(barIndex);
      var currentLow = bars.GetLow(barIndex);
      return new ApexLogic
      {
        bars = bars,
        barIndexOfPreviousTick = barIndex,
        eDistanceInPoints = eDistanceInPoints,
        xDistanceInPoints = xDistanceInPoints,
        Direction = direction,
        State = ApexStates.FormedA,
        A = new IndexAndValue(barIndex, direction.IsUp ? currentHigh : currentLow),
        MaxLow = double.MinValue, // currentLow,
        MinHigh = double.MaxValue, // currentHigh,
      };
    }

    public ApexPatternFlags Process(int barIndex)
    {
      this.barIndex = barIndex;

      Flags = 0;

      switch (State)
      {
        case ApexStates.FormedA:
          {
            if (barIndex == A.Index)
            {
              A.Value = Direction.IsUp ? CurrentHigh : CurrentLow;
            }
            else
            {
              if (TryFormP())
              {
                SetETriggerValue();
                SetXTriggerValue();

                // Handle edge case when A bar is also P bar, if the following bar has a higher low
                TryAdjustETriggerValue();
              }
              else
              {
                TryShiftA();
              }
            }

            break;
          }

        case ApexStates.FormedP:
          {
            if (barIndex == P!.Index)
            {
              P.Value = Direction.IsUp ? CurrentLow : CurrentHigh;
            }
            else
            {
              if (TryShiftP())
              {
                SetETriggerValue();
              }
              else if (TryFormE())
              {
                if (TryFormX())
                {
                }
              }
              else
              {
                if (TryAdjustETriggerValue())
                {
                }
              }
            }

            break;
          }

        case ApexStates.FormedE:
          {
            if (TryFailE())
            {
              SetETriggerValue();
            }
            else if (TryFormX())
            {
            }

            break;
          }

        case ApexStates.FormedX:
        default:
          {
            // note to readers: Having this exception here helped a LOT in debugging.
            // It's always a great practise to explicity throw on unexpected states rather than let the code ignore them!
            throw State.UnknownValueException();
          }
      }

      if (IsFirstTickOfBar())
      {
        MaxLow = Max(MaxLow, PreviousLow);
        MinHigh = Min(MinHigh, PreviousHigh);
      }

      barIndexOfPreviousTick = barIndex;
      return Flags;
    }

    private bool IsFirstTickOfBar()
    {
      return barIndex > barIndexOfPreviousTick;
    }

    private bool HasJustCompletedBar(Func<int, bool> condition)
    {
      return barIndex > barIndexOfPreviousTick && condition(barIndexOfPreviousTick);
    }

    /// <summary>
    /// Attempts to shift the A, returning true if the A was shifted.
    /// Also sets flags if the A was shifted.
    /// </summary>
    private bool TryShiftA()
    {
      if (Direction.IsUp)
      {
        // shift the A if there is an equal or higher high
        if (CurrentHigh >= A.Value)
        {
          A = new IndexAndValue(barIndex, CurrentHigh);
          Flags |= ApexPatternFlags.ShiftedA;

          MaxLow = double.MinValue;
          MinHigh = double.MaxValue;

          // indicate that the A was shifted
          return true;
        }
      }
      else
      {
        // shift the A if there is an equal or lower low
        if (CurrentLow <= A.Value)
        {
          A = new IndexAndValue(barIndex, CurrentLow);
          Flags |= ApexPatternFlags.ShiftedA;

          MaxLow = double.MinValue;
          MinHigh = double.MaxValue;

          // indicate that the A was shifted
          return true;
        }
      }

      // indicate that the A was not shifted
      return false;
    }

    /// <summary>
    /// Attempts to form a P, returning true if the P was formed.
    /// Also sets the state and flags if the P was formed.
    /// </summary>
    private bool TryFormP()
    {
      // Forming a P can only be done on the completion of a bar,
      // because it requires not only a lower low than than previous P bar, but also a lower high than the A bar,
      // and you don't know the high of the bar until after it has completed.
      // Also, it can only be done on the completion of any bar AFTER the A bar, not on the completion of the A bar itself.
      if (HasJustCompletedBar(index => index > A.Index))
      {
        var aHigh = bars.GetHigh(A.Index); // the high of the A bar
        var aLow = bars.GetLow(A.Index); // the low of the A bar

        if (Direction.IsUp)
        {
          // a P is formed when the  bar has a lower high and a lower low than those of the A bar
          if (PreviousLow < aLow && PreviousHigh < aHigh)
          {
            // set the value of the P at the low of the current bar
            P = new IndexAndValue(barIndex - 1, PreviousLow);
            State = ApexStates.FormedP;
            Flags |= ApexPatternFlags.FormedP;

            // indicate that a P was formed
            return true;
          }

          // Handle edge case when A bar is also P bar, if the following bar, when completed, has a lower high
          if (barIndexOfPreviousTick == A.Index + 1)
          {
            if (bars.GetHigh(A.Index + 1) < aHigh)
            {
              P = new IndexAndValue(A.Index, aLow);
              State = ApexStates.FormedP;
              Flags |= ApexPatternFlags.FormedP;

              // indicate that a P was formed
              return true;
            }
          }
        }
        else
        {
          // a P is formed when the bar has a higher high and a higher low than those of the A bar
          if (PreviousLow > aLow && PreviousHigh > aHigh)
          {
            // set the value of the P at the high of the current bar
            P = new IndexAndValue(barIndex - 1, PreviousHigh);
            State = ApexStates.FormedP;
            Flags |= ApexPatternFlags.FormedP;

            // indicate that a P was formed
            return true;
          }

          // Handle edge case when A bar is also P bar, if the following bar, when completed, has a higher low
          if (barIndexOfPreviousTick == A.Index + 1)
          {
            if (bars.GetLow(A.Index + 1) > aLow)
            {
              P = new IndexAndValue(A.Index, aLow);
              State = ApexStates.FormedP;
              Flags |= ApexPatternFlags.FormedP;

              // indicate that a P was formed
              return true;
            }
          }
        }
      }

      // indicate that a P was NOT formed
      return false;
    }

    /// <summary>
    /// Attempts to shift the P, returning true if the P was shifted.
    /// Also sets the flags if the P was shifted.
    /// </summary>
    private bool TryShiftP()
    {
      if (Direction.IsUp)
      {
        // The P is shifted if there is a lower low
        if (CurrentLow < P.Value)
        {
          // set the p value at the low of the current bar
          P = new IndexAndValue(barIndex, CurrentLow);
          Flags |= ApexPatternFlags.ShiftedP;

          // indicate that the P was shifted
          return true;
        }
      }
      else
      {
        // The P is shifted if there is a higher high
        if (CurrentHigh > P.Value)
        {
          // set the p value at the high of the current bar
          P = new IndexAndValue(barIndex, CurrentHigh);
          Flags |= ApexPatternFlags.ShiftedP;

          // indicate that the P was shifted
          return true;
        }
      }

      // indicate that the P was NOT shifted
      return false;
    }

    /// <summary>
    /// Attempts to form an E, returning true if an E was formed.
    /// Also sets flags and state if an E was formed.
    /// </summary>
    private bool TryFormE()
    {
      if (Direction.IsUp)
      {
        // an E is formed if the current high is >= ETriggerValue
        if (CurrentHigh >= ETriggerValue!.Value)
        {
          // set the E value at the E trigger value
          E = new IndexAndValue(barIndex, ETriggerValue.Value);
          Flags |= ApexPatternFlags.FormedE;
          State = ApexStates.FormedE;

          // indicate that an E was formed
          return true;
        }
      }
      else
      {
        // an E is formed if the current low is <= ETriggerValue
        if (CurrentLow <= ETriggerValue!.Value)
        {
          // set the E value at the E trigger value
          E = new IndexAndValue(barIndex, ETriggerValue.Value);
          Flags |= ApexPatternFlags.FormedE;
          State = ApexStates.FormedE;

          // indicate that an E was formed
          return true;
        }
      }

      // indicate that an E was NOT formed
      return false;
    }

    /// <summary>
    /// Attempts to fail the E, returning true if the E was failed.
    /// Also sets flags and state if the E was failed.
    /// </summary>
    private bool TryFailE()
    {
      if (Direction.IsUp)
      {
        // the E fails if the current low is less than the low of the P bar (which was stored as P.Value)
        if (CurrentLow < P!.Value)
        {
          // save the current P and E to the list of fails, for display purposes
          FailedEs = FailedEs.Add(E);
          FailedPs = FailedPs.Add(P);

          P = new IndexAndValue(barIndex, CurrentLow);
          E = null!;
          Flags |= ApexPatternFlags.FailedE;
          Flags |= ApexPatternFlags.ShiftedP; // note that we are also shifting the P when we fail the E
          State = ApexStates.FormedP;

          // indicate that the E failed
          return true;
        }
      }
      else
      {
        // the E fails if the current high is greater than the high of the P bar (which was stored as P.Value)
        if (CurrentHigh > P!.Value)
        {
          // save the current P and E to the list of fails, for display purposes
          FailedEs = FailedEs.Add(E!);
          FailedPs = FailedPs.Add(P);

          P = new IndexAndValue(barIndex, CurrentHigh);
          E = null!;
          Flags |= ApexPatternFlags.FailedE;
          Flags |= ApexPatternFlags.ShiftedP; // note that we are also shifting the P when we fail the E
          State = ApexStates.FormedP;

          // indicate that the E failed
          return true;
        }
      }

      // indicate that the E was NOT failed
      return false;
    }

    /// <summary>
    /// Attempts to form an X, returning true if an X was formed.
    /// Also sets flags and state if an X was formed.
    /// </summary>
    private bool TryFormX()
    {
      if (Direction.IsUp)
      {
        // an X is formed if the current high is >= XTriggerValue
        if (CurrentHigh >= XTriggerValue!.Value)
        {
          // set the value of the X at the X trigger value, not necessarily at the high of the current bar
          X = new IndexAndValue(barIndex, XTriggerValue.Value);
          Flags |= ApexPatternFlags.FormedX;
          State = ApexStates.FormedX;

          // indicate that an X was formed
          return true;
        }
      }
      else
      {
        // an X is formed if the current low is <= XTriggerValue
        if (CurrentLow <= XTriggerValue!.Value)
        {
          // set the value of the X at the X trigger value, not necessarily at the low of the current bar
          X = new IndexAndValue(barIndex, XTriggerValue.Value);
          Flags |= ApexPatternFlags.FormedX;
          State = ApexStates.FormedX;

          // indicate that an X was formed
          return true;
        }
      }

      // indicate that an X was NOT formed
      return false;
    }

    private void SetETriggerValue()
    {
      ETriggerValue = bars.BarsInfo.Instrument.Round2Tick(Direction.IsUp
          ? bars.GetHigh(P!.Index) + eDistanceInPoints
          : bars.GetLow(P!.Index) - eDistanceInPoints);
      Flags |= ApexPatternFlags.SetOrAdjustedETriggervalue;
    }

    private void SetXTriggerValue()
    {
      XTriggerValue = bars.BarsInfo.Instrument.Round2Tick(Direction.IsUp
          ? A.Value + xDistanceInPoints
          : A.Value - xDistanceInPoints);
    }

    private bool TryAdjustETriggerValue()
    {
      // have we just completed a bar that is after the P bar?
      if (HasJustCompletedBar(index => index > P.Index))
      {
        if (Direction.IsUp)
        {
          // does the completed bar have a lower high than the P bar?
          // If so, we can bring the ETriggerValue down a bit.
          var newValue = bars.BarsInfo.Instrument.Round2Tick(PreviousHigh + eDistanceInPoints);
          if (newValue < ETriggerValue!.Value)
          {
            ETriggerValue = newValue;
            Flags |= ApexPatternFlags.SetOrAdjustedETriggervalue;
            return true;
          }
        }
        else
        {
          // does the completed bar have a higher low than the P bar? 
          // If so, we can bring the ETriggerValue up a bit.
          var newValue = bars.BarsInfo.Instrument.Round2Tick(PreviousLow - eDistanceInPoints);
          if (newValue > ETriggerValue!.Value)
          {
            ETriggerValue = newValue;
            Flags |= ApexPatternFlags.SetOrAdjustedETriggervalue;
            return true;
          }
        }
      }

      return false;
    }
  }
}
