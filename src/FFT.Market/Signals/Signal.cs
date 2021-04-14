// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Signals
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using FFT.TimeStamps;

  public sealed class Signal : AggregateBase<Signal>
  {
    private readonly List<SignalEntryData> _canceledEntries = new();
    private readonly List<SignalStopLossData> _canceledStopLosses = new();
    private readonly List<SignalTargetData> _canceledTargets = new();

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

    public SignalEntryData? Entry { get; private set; }

    public SignalFill? EntryFill { get; private set; }

    public IEnumerable<SignalEntryData> CanceledEntries => _canceledEntries;

    public SignalStopLossData? StopLoss { get; private set; }

    public IEnumerable<SignalStopLossData> CanceledStopLosses => _canceledStopLosses;

    public SignalTargetData? Target { get; private set; }

    public IEnumerable<SignalTargetData> CanceledTargets => _canceledTargets;

    public SignalFill? ExitFill { get; private set; }

    private void Handle(CreateSignal command)
    {
      if (Version != 0)
        throw new InvalidOperationException("A signal can only be created once.");

      if (string.IsNullOrEmpty(command.StrategyName))
        throw new InvalidOperationException("Strategy name cannot be empty.");

      if (string.IsNullOrEmpty(command.SignalName))
        throw new InvalidOperationException("Signal name cannot be empty.");

      if (string.IsNullOrEmpty(command.Instrument))
        throw new InvalidOperationException("Instrument cannot be empty.");

      if (string.IsNullOrEmpty(command.Exchange))
        throw new InvalidOperationException("Exchange cannot be empty.");

      Apply(new SignalCreated
      {
        AggregateId = Id,
        At = TimeStamp.Now,
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
        At = TimeStamp.Now,
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

      if (Entry is not null && Entry.Direction != command.Direction)
        throw new InvalidOperationException("Cannot change the direction of a signal.");

      Apply(new SignalEntrySet
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

      Apply(new SignalEntryFilled
      {
        AggregateId = Id,
        At = TimeStamp.Now,
        Version = Version + 1,
        FillPrice = command.FillPrice,
      });
    }

    private void Handle(SetStopLoss command)
    {
      if (Version == 0)
        throw new InvalidOperationException("Command cannot be executed on a signal that has not been created.");

      if (ExitFill is not null)
        throw new InvalidOperationException("Cannot set stop loss after the exit has been filled.");

      if (command.Price <= 0)
        throw new InvalidOperationException("Price must be positive.");

      if (string.IsNullOrWhiteSpace(command.Tag))
        throw new InvalidOperationException("Tag must be set.");

      Apply(new SignalStopLossSet
      {
        AggregateId = Id,
        At = TimeStamp.Now,
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

      Apply(new SignalStopLossCanceled
      {
        AggregateId = Id,
        At = TimeStamp.Now,
        Version = Version + 1,
      });
    }

    private void Handle(SetTarget command)
    {
      if (Version == 0)
        throw new InvalidOperationException("Command cannot be executed on a signal that has not been created.");

      if (ExitFill is not null)
        throw new InvalidOperationException("Cannot set a target after the exit has filled.");

      if (command.Price <= 0)
        throw new InvalidOperationException("Price must be positive.");

      if (string.IsNullOrWhiteSpace(command.Tag))
        throw new InvalidOperationException("Tag must be set.");

      Apply(new SignalTargetSet
      {
        AggregateId = Id,
        At = TimeStamp.Now,
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

      Apply(new SignalTargetCanceled
      {
        AggregateId = Id,
        At = TimeStamp.Now,
        Version = Version + 1,
      });
    }

    private void Handle(FillTarget command)
    {
      if (Version == 0)
        throw new InvalidOperationException("Command cannot be executed on a signal that has not been created.");

      if (Target is null)
        throw new InvalidOperationException("Cannot fill a target that does not exist.");

      if (EntryFill is null)
        throw new InvalidOperationException("Cannot fill the exit before the entry.");

      if (ExitFill is not null)
        throw new InvalidOperationException("Cannot fill the exit more than once.");

      Apply(new SignalTargetFilled
      {
        AggregateId = Id,
        At = TimeStamp.Now,
        Version = Version + 1,
        Price = command.Price,
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
        _canceledEntries.Add(Entry);
        Entry = null;
      }

      if (StopLoss is not null)
      {
        _canceledStopLosses.Add(StopLoss);
        StopLoss = null;
      }

      Cancellation = new SignalCancellation
      {
        At = @event.At,
        Reason = @event.Reason,
      };
    }

    private void On(SignalEntrySet @event)
    {
      if (Entry is not null)
        _canceledEntries.Add(Entry);

      Entry = new SignalEntryData
      {
        At = @event.At,
        EntryType = @event.EntryType,
        Direction = @event.Direction,
        Price = @event.Price,
        Tag = @event.Tag,
      };
    }

    private void On(SignalEntryFilled @event)
    {
      EntryFill = new SignalFill
      {
        At = @event.At,
        Price = @event.FillPrice,
      };
    }

    private void On(SignalStopLossSet @event)
    {
      if (StopLoss is not null)
        _canceledStopLosses.Add(StopLoss);

      StopLoss = new SignalStopLossData
      {
        At = @event.At,
        Price = @event.Price,
        Tag = @event.Tag,
      };
    }

    private void On(SignalStopLossCanceled @event)
    {
      _canceledStopLosses.Add(StopLoss!);
      StopLoss = null;
    }

    private void On(SignalTargetSet @event)
    {
      if (Target is not null)
        _canceledTargets.Add(Target);

      Target = new SignalTargetData
      {
        At = @event.At,
        Price = @event.Price,
        Tag = @event.Tag,
      };
    }

    private void On(SignalTargetCanceled @event)
    {
      _canceledTargets.Add(Target!);
      Target = null;
    }

    private void On(SignalTargetFilled @event)
    {
      ExitFill = new SignalFill
      {
        At = @event.At,
        Price = @event.Price,
      };
    }
  }
}
