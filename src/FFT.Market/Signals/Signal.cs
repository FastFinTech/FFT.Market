// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Signals
{
  using System;
  using System.Collections.Generic;
  using System.Collections.Immutable;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using FFT.TimeStamps;

  public sealed class Signal : AggregateBase<Signal>
  {
    public Signal(Guid id)
      : base(id)
    {
    }

    public TimeStamp CreatedAt { get; private set; }

    public string StrategyName { get; private set; }

    public string SignalName { get; private set; }

    public string Instrument { get; private set; }

    public string Exchange { get; private set; }

    public SignalCancellation? Cancellation { get; private set; }

    public Entry? Entry { get; private set; }

    public Fill? EntryFill { get; private set; }

    public ImmutableList<EntryCancellation> CanceledEntries { get; private set; } = ImmutableList<EntryCancellation>.Empty;

    public ImmutableList<StopLossCancellation> CanceledStopLosses { get; private set; } = ImmutableList<StopLossCancellation>.Empty;

    public ImmutableList<TargetCancellation> CanceledTargets { get; private set; } = ImmutableList<TargetCancellation>.Empty;

    public StopLoss? StopLoss { get; private set; }

    public Target? Target { get; private set; }

    public Fill? ExitFill { get; private set; }

    private void Handle(CreateSignal command)
    {
      if (Version != 0)
        throw new InvalidOperationException("A signal can only be created once.");

      if (string.IsNullOrWhiteSpace(command.StrategyName))
        throw new InvalidOperationException("Strategy name cannot be empty.");

      if (string.IsNullOrWhiteSpace(command.SignalName))
        throw new InvalidOperationException("Signal name cannot be empty.");

      if (string.IsNullOrWhiteSpace(command.Instrument))
        throw new InvalidOperationException("Instrument cannot be empty.");

      if (string.IsNullOrWhiteSpace(command.Exchange))
        throw new InvalidOperationException("Exchange cannot be empty.");

      Apply(new SignalCreated
      {
        AggregateId = Id,
        At = command.At,
        Version = Version + 1,
        StrategyName = command.StrategyName,
        SignalName = command.SignalName,
        Instrument = command.Instrument,
        Exchange = command.Exchange,
      });
    }

    private void Handle(CancelSignal command)
    {
      if (Version == 0)
        throw new InvalidOperationException("Command cannot be executed on a signal that has not been created.");

      if (EntryFill is not null)
        throw new InvalidOperationException("Cannot cancel a signal after the entry has been filled.");

      if (Cancellation is not null)
        throw new InvalidOperationException("Cannot cancel a signal more than once.");

      if (string.IsNullOrWhiteSpace(command.Reason))
        throw new InvalidOperationException("Signal cancellation reason must be provided.");

      Apply(new SignalCanceled
      {
        AggregateId = Id,
        At = command.At,
        Version = Version + 1,
        Reason = command.Reason,
      });
    }

    private void Handle(SetEntry command)
    {
      if (Version == 0)
        throw new InvalidOperationException("Command cannot be executed on a signal that has not been created.");

      if (Cancellation is not null)
        throw new InvalidOperationException("Cannot set entry after the signal has been canceled.");

      if (EntryFill is not null)
        throw new InvalidOperationException("Cannot set entry after it has been filled.");

      if (command.Direction.IsUnknown)
        throw new InvalidOperationException("Direction must be set.");

      if (command.Price <= 0)
        throw new InvalidOperationException("Price must be positive.");

      if (string.IsNullOrWhiteSpace(command.Tag))
        throw new InvalidOperationException("Tag must be set.");

      if (Entry is not null)
      {
        if (Entry.Direction != command.Direction)
          throw new InvalidOperationException("Cannot change the direction of a signal.");
      }

      Apply(new EntrySet
      {
        AggregateId = Id,
        At = TimeStamp.Now,
        Version = Version + 1,
        Direction = command.Direction,
        EntryType = command.EntryType,
        Price = command.Price,
        Tag = command.Tag,
      });
    }

    private void Handle(FillEntry command)
    {
      if (Version == 0)
        throw new InvalidOperationException("Command cannot be executed on a signal that has not been created.");

      if (Entry is null)
        throw new InvalidOperationException("Cannot fill an entry that doesn't exist.");

      if (EntryFill is not null)
        throw new InvalidOperationException("Cannot fill entry more than once.");

      if (command.Price <= 0)
        throw new InvalidOperationException("Price must be greater than zero.");

      Apply(new EntryFilled
      {
        AggregateId = Id,
        At = command.At,
        Version = Version + 1,
        FillPrice = command.Price,
      });
    }

    private void Handle(SetStopLoss command)
    {
      if (Version == 0)
        throw new InvalidOperationException("Command cannot be executed on a signal that has not been created.");

      if (Entry is null)
        throw new Exception("Cannot set stop loss before setting the entry.");

      if (ExitFill is not null)
        throw new InvalidOperationException("Cannot set stop loss after the exit has been filled.");

      if (command.Price <= 0)
        throw new InvalidOperationException("Price must be positive.");

      if (string.IsNullOrWhiteSpace(command.Tag))
        throw new InvalidOperationException("Tag must be set.");

      Apply(new StopLossSet
      {
        AggregateId = Id,
        At = command.At,
        Version = Version + 1,
        Price = command.Price,
        Tag = command.Tag,
      });
    }

    private void Handle(CancelStopLoss command)
    {
      if (Version == 0)
        throw new InvalidOperationException("Command cannot be executed on a signal that has not been created.");

      if (StopLoss is null)
        throw new InvalidOperationException("Cannot cancel a stop loss that doesn't exist.");

      if (ExitFill is not null)
        throw new InvalidOperationException("Cannot cancel a stop loss after the exit has filled.");

      if (string.IsNullOrWhiteSpace(command.Reason))
        throw new InvalidOperationException("Reason must not be empty.");

      Apply(new StopLossCanceled
      {
        AggregateId = Id,
        At = command.At,
        Version = Version + 1,
        Reason = command.Reason,
      });
    }

    private void Handle(SetTarget command)
    {
      if (Version == 0)
        throw new InvalidOperationException("Command cannot be executed on a signal that has not been created.");

      if (Entry is null)
        throw new Exception("Cannot set target before setting the entry.");

      if (ExitFill is not null)
        throw new InvalidOperationException("Cannot set a target after the exit has filled.");

      if (command.Price <= 0)
        throw new InvalidOperationException("Price must be positive.");

      if (string.IsNullOrWhiteSpace(command.Tag))
        throw new InvalidOperationException("Tag must be set.");

      Apply(new TargetSet
      {
        AggregateId = Id,
        At = command.At,
        Version = Version + 1,
        Price = command.Price,
        Tag = command.Tag,
      });
    }

    private void Handle(CancelTarget command)
    {
      if (Version == 0)
        throw new InvalidOperationException("Command cannot be executed on a signal that has not been created.");

      if (Target is null)
        throw new InvalidOperationException("Cannot cancel a target that doesn't exist.");

      if (ExitFill is not null)
        throw new InvalidOperationException("Cannot cancel a target after the exit has filled.");

      if (string.IsNullOrWhiteSpace(command.Reason))
        throw new InvalidOperationException("Reason must not be empty.");

      Apply(new TargetCanceled
      {
        AggregateId = Id,
        At = command.At,
        Version = Version + 1,
        Reason = command.Reason,
      });
    }

    private void Handle(FillExit command)
    {
      if (Version == 0)
        throw new InvalidOperationException("Command cannot be executed on a signal that has not been created.");

      if (EntryFill is null)
        throw new InvalidOperationException("Cannot fill the exit before the entry.");

      if (ExitFill is not null)
        throw new InvalidOperationException("Cannot fill the exit more than once.");

      Apply(new ExitFilled
      {
        AggregateId = Id,
        At = command.At,
        Version = Version + 1,
        Price = command.Price,
        Reason = command.Reason,
      });
    }

    private void On(SignalCreated @event)
    {
      CreatedAt = @event.At;
      StrategyName = @event.StrategyName;
      SignalName = @event.SignalName;
      Instrument = @event.Instrument;
      Exchange = @event.Exchange;
    }

    private void On(SignalCanceled @event)
    {
      if (Entry is not null)
      {
        CanceledEntries = CanceledEntries.Add(new EntryCancellation
        {
          At = @event.At,
          Reason = "Siganl was canceled.",
          Entry = Entry,
        });
        Entry = null;
      }

      if (StopLoss is not null)
      {
        CanceledStopLosses = CanceledStopLosses.Add(new StopLossCancellation
        {
          At = @event.At,
          StopLoss = StopLoss,
          Reason = "Signal was canceled.",
        });
        StopLoss = null;
      }

      if (Target is not null)
      {
        CanceledTargets = CanceledTargets.Add(new TargetCancellation
        {
          At = @event.At,
          Reason = "Signal was canceled.",
          Target = Target,
        });
        Target = null;
      }

      Cancellation = new SignalCancellation
      {
        At = @event.At,
        Reason = @event.Reason,
      };
    }

    private void On(EntrySet @event)
    {
      if (Entry is not null)
      {
        CanceledEntries = CanceledEntries.Add(new EntryCancellation
        {
          At = @event.At,
          Reason = "Entry was replaced.",
          Entry = Entry,
        });
      }

      Entry = new Entry
      {
        At = @event.At,
        EntryType = @event.EntryType,
        Direction = @event.Direction,
        Price = @event.Price,
        Tag = @event.Tag,
      };
    }

    private void On(EntryFilled @event)
    {
      EntryFill = new Fill
      {
        At = @event.At,
        Price = @event.FillPrice,
      };
    }

    private void On(StopLossSet @event)
    {
      if (StopLoss is not null)
      {
        CanceledStopLosses = CanceledStopLosses.Add(new StopLossCancellation
        {
          At = @event.At,
          Reason = "Stop loss was replaced by another.",
          StopLoss = StopLoss,
        });
      }

      StopLoss = new StopLoss
      {
        At = @event.At,
        Price = @event.Price,
        Tag = @event.Tag,
      };
    }

    private void On(StopLossCanceled @event)
    {
      CanceledStopLosses = CanceledStopLosses.Add(new StopLossCancellation
      {
        At = @event.At,
        StopLoss = StopLoss!,
        Reason = @event.Reason,
      });
      StopLoss = null;
    }

    private void On(TargetSet @event)
    {
      if (Target is not null)
      {
        CanceledTargets = CanceledTargets.Add(new TargetCancellation
        {
          At = @event.At,
          Reason = "Target was replaced.",
          Target = Target,
        });
      }

      Target = new Target
      {
        At = @event.At,
        Price = @event.Price,
        Tag = @event.Tag,
      };
    }

    private void On(TargetCanceled @event)
    {
      CanceledTargets.Add(new TargetCancellation
      {
        At = @event.At,
        Reason = @event.Reason,
        Target = Target!,
      });
      Target = null;
    }

    private void On(ExitFilled @event)
    {
      ExitFill = new Fill
      {
        At = @event.At,
        Price = @event.Price,
        Reason = @event.Reason,
      };
    }
  }
}
