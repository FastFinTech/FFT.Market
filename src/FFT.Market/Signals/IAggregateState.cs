// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Signals
{
  using System;
  using System.Buffers;

  /// <summary>
  /// Represents the state of a domain object in an Event-Sourcing system.
  /// </summary>
  public interface IAggregateState
  {
    /// <summary>
    /// The object id.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// The state version. Can be thought of as the count of the events that
    /// have been applied.
    /// </summary>
    long Version { get; }

    /// <summary>
    /// Performs basic validation checking of a command prior to handling it.
    /// Domain-specific validation is performed by inheriting implementations.
    /// </summary>
    void ValidateCommand(ICommand command)
    {
      if (command.AggregateId != Id)
        throw new Exception($"Event aggregate id '{command.AggregateId:N}' did not match expected id '{Id:N}'.");

      if (command.ExpectedVersion != Version)
        throw new Exception($"Command expected version '{command.ExpectedVersion}' did not match expected version '{Version}'.");
    }

    /// <summary>
    /// Performs basic validation checking of an event prior to applying it.
    /// Domain-specific validation is performed by inheriting implementations.
    /// </summary>
    void ValidateEvent(IEvent @event)
    {
      if (@event.AggregateId != Id)
        throw new Exception($"Event aggregate id '{@event.AggregateId:N}' did not match expected id '{Id:N}'.");

      if (@event.Version != Version + 1)
        throw new Exception($"Event version '{@event.Version}' did not match expected version '{Version + 1}'.");
    }

    IEventSerializer GetEventSerializer();
  }
}
