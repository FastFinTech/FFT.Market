// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Signals
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;

  public abstract class AggregateBase<T>
    where T : AggregateBase<T>
  {
    private static readonly Dictionary<Type, Action<ICommand>> _commandHandlers = new();

    private List<IEvent> _events = new();

    public AggregateBase(Guid id)
    {
      Id = id;
    }

    public Guid Id { get; }

    public long Version { get; private set; }

    public int EventCount => _events.Count;

    public void Load(IEnumerable<IEvent> events)
    {
      foreach (var @event in events)
        Apply(@event);
      PopEvents();
    }

    public int Handle(ICommand command)
    {
      if (command.AggregateId != Id)
        throw new InvalidOperationException($"Command aggregate id '{command.AggregateId}' did not match actual id '{Id}'.");

      if (command.ExpectedVersion != Version)
        throw new InvalidOperationException($"Command expected version '{command.ExpectedVersion}' did not match actual version '{Version}'.");

      var previousEventCount = _events.Count;

      this.Invoke("Handle", command);
      return _events.Count - previousEventCount;
    }

    public IEnumerable<IEvent> PopEvents()
    {
      var events = _events;
      _events = new();
      return events;
    }

    protected void Apply(IEvent @event)
    {
      this.Invoke("On", @event);
      Version++;
      _events.Add(@event);
    }
  }
}
