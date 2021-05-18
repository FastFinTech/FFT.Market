// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Signals
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;

  public interface IAggregate<TState>
    where TState : IAggregateState
  {
    TState State { get; set; }
    void Handle(ICommand command);
    IEvent[] HandlePreview(ICommand command);
    void Apply(IEvent @event);
  }
}
