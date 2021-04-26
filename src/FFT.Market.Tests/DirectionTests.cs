// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Tests
{
  using System;
  using System.Text.Json;
  using FFT.Market.Signals;
  using FFT.TimeStamps;
  using Microsoft.VisualStudio.TestTools.UnitTesting;

  [TestClass]
  public class DirectionTests
  {
    [TestMethod]
    public void Direction_Serialization()
    {
      var directions = new[] { Direction.Up, Direction.Down, Direction.Unknown, null };
      var json = JsonSerializer.Serialize(directions);
      var newDirections = JsonSerializer.Deserialize<Direction?[]>(json);
      Assert.IsTrue(EnumerableEqualityComparer<Direction>.Default.Equals(directions!, newDirections!));
      Assert.IsNull(JsonSerializer.Deserialize<Direction?>("null"));
      Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize<Direction?>(string.Empty));
      Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize<Direction?>("someRubbish"));
    }
  }
}
