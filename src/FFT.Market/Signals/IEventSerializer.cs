// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Signals
{
  using System;
  using System.Text.Json;

  public interface IEventSerializer
  {
    void Serialize(IEvent @event, out string eventType, Utf8JsonWriter writer);

    IEvent Deserialize(string eventType, ReadOnlySpan<byte> data);
  }
}
