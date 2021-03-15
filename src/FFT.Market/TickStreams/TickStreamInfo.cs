// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.TickStreams
{
  using System;
  using FFT.Market.Instruments;
  using FFT.Market.Sessions.TradingHoursSessions;

  public sealed class TickStreamInfo : IEquatable<TickStreamInfo>
  {
    public TickStreamInfo(IInstrument instrument, TradingSessions tradingSessions)
    {
      Instrument = instrument;
      TradingSessions = tradingSessions;
      Value = HashCode.Combine(Instrument, TradingSessions);
    }

    public IInstrument Instrument { get; }
    public TradingSessions TradingSessions { get; }

    /// <summary>
    /// Use this value for fast identifiation/comparison of this TickStream info,
    /// instead of the <see cref="GetHashCode"/> and <see cref="Equals(object)"/> methods,
    /// which are slower.
    /// </summary>
    public int Value { get; }

    public static bool operator ==(TickStreamInfo left, TickStreamInfo right)
      => left?.Equals(right) == true;

    public static bool operator !=(TickStreamInfo left, TickStreamInfo right)
      => left?.Equals(right) != true;

    /// <summary>
    /// These methods are provided so that this object can be used in HashSets and the Linq "Distinct" method etc,
    /// but should not be used in any cpu-intensive repetitive operation becuase they are slow. When performance is
    /// required, make comparisions using the <see cref="Value"/> field instead.
    /// </summary>
    public override int GetHashCode() => Value;

    /// <summary>
    /// These methods are provided so that this object can be used in HashSets and the Linq "Distinct" method etc,
    /// but should not be used in any cpu-intensive repetitive operation becuase they are slow. When performance is
    /// required, make comparisions using the <see cref="Value"/> field instead.
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as TickStreamInfo);

    /// <summary>
    /// These methods are provided so that this object can be used in HashSets and the Linq "Distinct" method etc,
    /// but should not be used in any cpu-intensive repetitive operation becuase they are slow. When performance is
    /// required, make comparisions using the <see cref="Value"/> field instead.
    /// </summary>
    public bool Equals(TickStreamInfo? other) => Value == other?.Value;
  }
}
