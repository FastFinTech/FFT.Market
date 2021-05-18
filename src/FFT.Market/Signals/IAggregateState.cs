// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Signals
{
  using System;

  public interface IAggregateState
  {
    Guid Id { get; }

    long Version { get; }

    void ValidateCommand(ICommand command)
    {
      if (command.AggregateId != Id)
        throw new Exception($"Event aggregate id '{command.AggregateId:N}' did not match expected id '{Id:N}'.");

      if (command.ExpectedVersion != Version)
        throw new Exception($"Command expected version '{command.ExpectedVersion}' did not match expected version '{Version}'.");
    }

    void ValidateEvent(IEvent @event)
    {
      if (@event.AggregateId != Id)
        throw new Exception($"Event aggregate id '{@event.AggregateId:N}' did not match expected id '{Id:N}'.");

      if (@event.Version != Version + 1)
        throw new Exception($"Event version '{@event.Version}' did not match expected version '{Version + 1}'.");
    }
  }
}
