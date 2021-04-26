// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market
{
  using System;
  using System.Text.Json;
  using System.Text.Json.Serialization;
  using static System.Math;

  /// <summary>
  /// Expresses a direction in terms of Up, Down, or Unknown.
  /// This class is heavily used in performance-critical scenarios and it has been coded with efficiency in mind.
  /// Its most common usage is to compare two instances to determine if they are equal, or to see which is greater.
  /// It was found through testing that the most efficient way to implement this is to ensure that ONLY THREE instances
  /// of this type are ever created (one for each direction including "Unknown"), and that we make use of the object equality
  /// operator "==" whenever possible for these purposes as it is faster than accessing member variables.
  /// When modifying this class you must understand:
  /// 1. To create more efficient code, much of the logic within consists of expression similar to "if (this == Up)"
  ///    instead of something like "if (this._direction == 1)". Tests show that the class is working faster this way.
  ///    To keep the system working when coded like this instead of accessing member variables, it's important that:
  ///    a) The class remain "sealed", and,
  ///    b) NO OTHER instances of this class be created and the constructor be kept private.
  /// When using this class you must understand:
  /// 1. It's not intended that instances of this type be null. In many cases you will experience <see cref="NullReferenceException"/>
  ///    when passing null into any of the method or operator parameters. This is on purpose, as null-checking is a waste of cpu
  ///    in performance-critical code paths, when it is documented and you understand that you are EXPECTED to initialize your
  ///    Direction variables as Direction.Unknown.
  /// </summary>
  [JsonConverter(typeof(Converter))]
  public sealed class Direction : IEquatable<Direction>, IComparable<Direction>
  {
    public static readonly Direction Unknown = new Direction(0, "Unknown");
    public static readonly Direction Up = new Direction(1, "Up");
    public static readonly Direction Down = new Direction(-1, "Down");

    private readonly int _direction;
    private readonly string _name;

    private Direction(int direction, string name)
    {
      _direction = direction;
      _name = name;
    }

    public bool IsUp => this == Up;
    public bool IsDown => this == Down;
    public bool IsUnknown => this == Unknown;

    public Direction Opposite
    {
      get
      {
        if (this == Up) return Down;
        if (this == Down) return Up;
        throw new Exception("Cannot get opposite direction from an unknown direction.");
      }
    }

    public static implicit operator int(Direction d1)
      => d1._direction;

    public static explicit operator Direction(double d)
    {
      if (d > 0) return Up;
      if (d < 0) return Down;
      return Unknown;
    }

    public static explicit operator Direction(int d)
    {
      if (d > 0) return Up;
      if (d < 0) return Down;
      return Unknown;
    }

    /// <summary>
    /// Returns <see cref="Up"/> when both multiplicands are the same and not <see cref="Unknown"/>.
    /// Returns <see cref="Down"/> when the two multiplicands are different and not <see cref="Unknown"/>.
    /// Returns <see cref="Unknown"/> when either of the multiplicands are <see cref="Unknown"/>.
    /// </summary>
    /// <exception cref="NullReferenceException">Thrown when either operand is <see langword="null""/>.</exception>
    public static Direction operator *(Direction d1, Direction d2)
        => (Direction)(d1._direction * d2._direction);

    public static Direction FromValues(double first, double second)
    {
      switch (second.CompareTo(first))
      {
        case 1: return Up;
        case -1: return Down;
      }

      return Unknown;
    }

    public static Direction FromValues(double first, double second, double minDelta)
    {
      var actualDelta = Abs(first - second);
      if (actualDelta < minDelta) return Unknown;
      if (second.CompareTo(first) == 1) return Up;
      return Down;
    }

    public override bool Equals(object? obj) => this == obj;

    public bool Equals(Direction? other) => this == other;

    public override int GetHashCode() => _direction;

    public override string ToString()
      => _name;

    public string ToString(string resultIfUp, string resultIfDown)
    {
      if (this == Up) return resultIfUp;
      if (this == Down) return resultIfDown;
      throw new Exception("Direction is unknown.");
    }

    /// <summary>
    /// Comparison treats these values in ascending order: <see cref="Down"/>,
    /// <see cref="Unknown"/>, <see cref="Up"/>.
    /// </summary>
    /// <exception cref="NullReferenceException">Thrown when <paramref
    /// name="other"/> is <see langword="null""/>.</exception>
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
    public int CompareTo(Direction other) => _direction.CompareTo(other._direction);
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).

    private class Converter : JsonConverter<Direction>
    {
      public override Direction? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        if (reader.TokenType == JsonTokenType.Null)
          return null;

        if (reader.TokenType == JsonTokenType.String)
        {
          var stringValue = reader.GetString();
          return stringValue switch
          {
            "Up" => Direction.Up,
            "Down" => Direction.Down,
            "Unknown" => Direction.Unknown,
            _ => throw new JsonException($"Unable to parse a '{nameof(Direction)}' from value '{stringValue}'."),
          };
        }

        throw new JsonException($"Token type not known when parsing '{nameof(Direction)}' type.");
      }

      public override void Write(Utf8JsonWriter writer, Direction value, JsonSerializerOptions options)
        => writer.WriteStringValue(value?.ToString());
    }
  }
}
