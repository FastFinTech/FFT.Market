// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace FFT.Market.Signals
{
  public sealed class Signal : IAggregate<SignalState>
  {
    public Signal(Guid id)
    {
      State = new SignalState
      {
        Id = id,
      };
    }

    public SignalState State { get; set; }

    public void Apply(IEvent @event)
      => State = State.With(@event);

    public void Handle(ICommand command)
      => State = State.Handle(command);

    public IEvent[] HandlePreview(ICommand command)
      => State.HandlePreview(command);
  }
}
