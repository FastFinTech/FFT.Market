// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Signals
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;

  /// <summary>
  /// Represents a domain object in an Event-Sourcing system.
  /// </summary>
  /// <typeparam name="TState">The type that contains the state of the domain
  /// object.</typeparam>
  public interface IAggregate<TState>
    where TState : IAggregateState
  {
    /// <summary>
    /// Gets the aggregate state.
    /// </summary>
    TState State { get; set; }

    /// <summary>
    /// Processes the commands and updates the aggregate state with any events
    /// that were raised.
    /// </summary>
    void Handle(ICommand command);

    /// <summary>
    /// Makes a dry run of the command and returns any events that would have
    /// been raised.
    /// </summary>
    IEvent[] HandlePreview(ICommand command);

    /// <summary>
    /// Updates the aggregate state with the given event.
    /// </summary>
    void Apply(IEvent @event);
  }
}
