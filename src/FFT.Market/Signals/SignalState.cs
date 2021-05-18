// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Signals
{
  using System;
  using System.Collections.Immutable;
  using FFT.TimeStamps;

  public sealed record SignalState : IAggregateState
  {
    public Guid Id { get; init; }

    public long Version { get; init; }

    public TimeStamp CreatedAt { get; init; }

    public string StrategyName { get; init; }

    public string SignalName { get; init; }

    public string Instrument { get; init; }

    public string Exchange { get; init; }

    public SignalCancellation? Cancellation { get; init; }

    public Entry? Entry { get; init; }

    public Fill? EntryFill { get; init; }

    public ImmutableList<EntryCancellation> CanceledEntries { get; init; } = ImmutableList<EntryCancellation>.Empty;

    public ImmutableList<StopLossCancellation> CanceledStopLosses { get; init; } = ImmutableList<StopLossCancellation>.Empty;

    public ImmutableList<TargetCancellation> CanceledTargets { get; init; } = ImmutableList<TargetCancellation>.Empty;

    public ImmutableList<DiaryEntry> DiaryEntries { get; init; } = ImmutableList<DiaryEntry>.Empty;

    public StopLoss? StopLoss { get; init; }

    public Target? Target { get; init; }

    public Fill? ExitFill { get; init; }

    public SignalState Handle(ICommand command)
    {
      var state = this;
      foreach (var @event in HandlePreview(command))
        state = state.With(@event);
      return state;
    }

    public IEvent[] HandlePreview(ICommand command)
    {
      ((IAggregateState)this).ValidateCommand(command);
      return command switch
      {
        CreateSignal x => HandlePreview(x),
        CancelSignal x => HandlePreview(x),
        SetEntry x => HandlePreview(x),
        FillEntry x => HandlePreview(x),
        SetStopLoss x => HandlePreview(x),
        CancelStopLoss x => HandlePreview(x),
        SetTarget x => HandlePreview(x),
        CancelTarget x => HandlePreview(x),
        FillExit x => HandlePreview(x),
        AddDiaryEntry x => HandlePreview(x),
        _ => throw new Exception($"Unknown command type '{command.GetType()}'."),
      };
    }

    private IEvent[] HandlePreview(CreateSignal command)
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

      return new IEvent[]
      {
        new SignalCreated
        {
          AggregateId = Id,
          At = command.At,
          Version = Version + 1,
          StrategyName = command.StrategyName,
          SignalName = command.SignalName,
          Instrument = command.Instrument,
          Exchange = command.Exchange,
        },
      };
    }

    private IEvent[] HandlePreview(CancelSignal command)
    {
      if (Version == 0)
        throw new InvalidOperationException("Command cannot be executed on a signal that has not been created.");

      if (EntryFill is not null)
        throw new InvalidOperationException("Cannot cancel a signal after the entry has been filled.");

      if (Cancellation is not null)
        throw new InvalidOperationException("Cannot cancel a signal more than once.");

      if (string.IsNullOrWhiteSpace(command.Reason))
        throw new InvalidOperationException("Signal cancellation reason must be provided.");

      return new IEvent[]
      {
        new SignalCanceled
        {
          AggregateId = Id,
          At = command.At,
          Version = Version + 1,
          Reason = command.Reason,
        },
      };
    }

    private IEvent[] HandlePreview(SetEntry command)
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

      return new IEvent[]
      {
        new EntrySet
        {
          AggregateId = Id,
          At = command.At,
          Version = Version + 1,
          Direction = command.Direction,
          EntryType = command.EntryType,
          Price = command.Price,
          Tag = command.Tag,
        },
      };
    }

    private IEvent[] HandlePreview(FillEntry command)
    {
      if (Version == 0)
        throw new InvalidOperationException("Command cannot be executed on a signal that has not been created.");

      if (Entry is null)
        throw new InvalidOperationException("Cannot fill an entry that doesn't exist.");

      if (EntryFill is not null)
        throw new InvalidOperationException("Cannot fill entry more than once.");

      if (command.Price <= 0)
        throw new InvalidOperationException("Price must be greater than zero.");

      return new IEvent[]
      {
        new EntryFilled
        {
          AggregateId = Id,
          At = command.At,
          Version = Version + 1,
          FillPrice = command.Price,
        },
      };
    }

    private IEvent[] HandlePreview(SetStopLoss command)
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

      return new IEvent[]
      {
        new StopLossSet
        {
          AggregateId = Id,
          At = command.At,
          Version = Version + 1,
          Price = command.Price,
          Tag = command.Tag,
        },
      };
    }

    private IEvent[] HandlePreview(CancelStopLoss command)
    {
      if (Version == 0)
        throw new InvalidOperationException("Command cannot be executed on a signal that has not been created.");

      if (StopLoss is null)
        throw new InvalidOperationException("Cannot cancel a stop loss that doesn't exist.");

      if (ExitFill is not null)
        throw new InvalidOperationException("Cannot cancel a stop loss after the exit has filled.");

      if (string.IsNullOrWhiteSpace(command.Reason))
        throw new InvalidOperationException("Reason must not be empty.");

      return new IEvent[]
      {
        new StopLossCanceled
        {
          AggregateId = Id,
          At = command.At,
          Version = Version + 1,
          Reason = command.Reason,
        },
      };
    }

    private IEvent[] HandlePreview(SetTarget command)
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

      return new IEvent[]
      {
        new TargetSet
        {
          AggregateId = Id,
          At = command.At,
          Version = Version + 1,
          Price = command.Price,
          Tag = command.Tag,
        },
      };
    }

    private IEvent[] HandlePreview(CancelTarget command)
    {
      if (Version == 0)
        throw new InvalidOperationException("Command cannot be executed on a signal that has not been created.");

      if (Target is null)
        throw new InvalidOperationException("Cannot cancel a target that doesn't exist.");

      if (ExitFill is not null)
        throw new InvalidOperationException("Cannot cancel a target after the exit has filled.");

      if (string.IsNullOrWhiteSpace(command.Reason))
        throw new InvalidOperationException("Reason must not be empty.");

      return new IEvent[]
      {
        new TargetCanceled
        {
          AggregateId = Id,
          At = command.At,
          Version = Version + 1,
          Reason = command.Reason,
        },
      };
    }

    private IEvent[] HandlePreview(FillExit command)
    {
      if (Version == 0)
        throw new InvalidOperationException("Command cannot be executed on a signal that has not been created.");

      if (EntryFill is null)
        throw new InvalidOperationException("Cannot fill the exit before the entry.");

      if (ExitFill is not null)
        throw new InvalidOperationException("Cannot fill the exit more than once.");

      return new IEvent[]
      {
        new ExitFilled
        {
          AggregateId = Id,
          At = command.At,
          Version = Version + 1,
          Price = command.Price,
          Reason = command.Reason,
        },
      };
    }

    private IEvent[] HandlePreview(AddDiaryEntry command)
    {
      if (Version == 0)
        throw new InvalidOperationException("Command cannot be executed on a signal that has not been created.");

      return new IEvent[]
      {
        new DiaryEntryAdded
        {
          AggregateId = Id,
          At = command.At,
          Version = Version + 1,
          Message = command.Message,
        },
      };
    }

    public SignalState With(IEvent @event)
    {
      ((IAggregateState)this).ValidateEvent(@event);
      return @event switch
      {
        SignalCreated x => With(x),
        SignalCanceled x => With(x),
        EntrySet x => With(x),
        EntryFilled x => With(x),
        StopLossSet x => With(x),
        StopLossCanceled x => With(x),
        TargetSet x => With(x),
        TargetCanceled x => With(x),
        ExitFilled x => With(x),
        DiaryEntryAdded x => With(x),
        _ => throw new Exception($"Unknown event type '{@event.GetType()}'."),
      };
    }

    private SignalState With(SignalCreated @event)
    {
      return this with
      {
        Id = @event.AggregateId,
        CreatedAt = @event.At,
        StrategyName = @event.StrategyName,
        SignalName = @event.SignalName,
        Instrument = @event.Instrument,
        Exchange = @event.Exchange,
        Version = @event.Version,
      };
    }

    private SignalState With(SignalCanceled @event)
    {
      return this with
      {
        Entry = null,
        CanceledEntries = Entry is null
          ? CanceledEntries
          : CanceledEntries.Add(new EntryCancellation
          {
            At = @event.At,
            Reason = "Signal was canceled.",
            Entry = Entry,
          }),
        CanceledStopLosses = StopLoss is null
          ? CanceledStopLosses
          : CanceledStopLosses.Add(new StopLossCancellation
          {
            At = @event.At,
            StopLoss = StopLoss,
            Reason = "Signal was canceled.",
          }),
        CanceledTargets = Target is null
          ? CanceledTargets
          : CanceledTargets.Add(new TargetCancellation
          {
            At = @event.At,
            Reason = "Signal was canceled.",
            Target = Target,
          }),
        Cancellation = new SignalCancellation
        {
          At = @event.At,
          Reason = @event.Reason,
        },
        Version = @event.Version,
      };
    }

    private SignalState With(EntrySet @event)
    {
      return this with
      {
        CanceledEntries = Entry is null
          ? CanceledEntries
          : CanceledEntries.Add(new EntryCancellation
          {
            At = @event.At,
            Reason = "Entry was replaced.",
            Entry = Entry,
          }),
        Entry = new Entry
        {
          At = @event.At,
          EntryType = @event.EntryType,
          Direction = @event.Direction,
          Price = @event.Price,
          Tag = @event.Tag,
        },
        Version = @event.Version,
      };
    }

    private SignalState With(EntryFilled @event)
    {
      return this with
      {
        EntryFill = new Fill
        {
          At = @event.At,
          Price = @event.FillPrice,
        },
        Version = @event.Version,
      };
    }

    private SignalState With(StopLossSet @event)
    {
      return this with
      {
        CanceledStopLosses = StopLoss is null
          ? CanceledStopLosses
          : CanceledStopLosses.Add(new StopLossCancellation
          {
            At = @event.At,
            Reason = "Stop loss was replaced by another.",
            StopLoss = StopLoss,
          }),
        StopLoss = new StopLoss
        {
          At = @event.At,
          Price = @event.Price,
          Tag = @event.Tag,
        },
        Version = @event.Version,
      };
    }

    private SignalState With(StopLossCanceled @event)
    {
      return this with
      {
        CanceledStopLosses = StopLoss is null
          ? CanceledStopLosses
          : CanceledStopLosses.Add(new StopLossCancellation
          {
            At = @event.At,
            StopLoss = StopLoss!,
            Reason = @event.Reason,
          }),
        StopLoss = null,
        Version = @event.Version,
      };
    }

    private SignalState With(TargetSet @event)
    {
      return this with
      {
        CanceledTargets = Target is null
          ? CanceledTargets
          : CanceledTargets.Add(new TargetCancellation
          {
            At = @event.At,
            Reason = "Target was replaced.",
            Target = Target,
          }),
        Target = new Target
        {
          At = @event.At,
          Price = @event.Price,
          Tag = @event.Tag,
        },
        Version = @event.Version,
      };
    }

    private SignalState With(TargetCanceled @event)
    {
      return this with
      {
        CanceledTargets = Target is null
          ? CanceledTargets
          : CanceledTargets.Add(new TargetCancellation
          {
            At = @event.At,
            Reason = @event.Reason,
            Target = Target!,
          }),
        Target = null,
        Version = @event.Version,
      };
    }

    private SignalState With(ExitFilled @event)
    {
      return this with
      {
        ExitFill = new Fill
        {
          At = @event.At,
          Price = @event.Price,
          Reason = @event.Reason,
        },
        Version = @event.Version,
      };
    }

    private SignalState With(DiaryEntryAdded @event)
    {
      return this with
      {
        DiaryEntries = DiaryEntries.Add(new DiaryEntry
        {
          At = @event.At,
          Message = @event.Message,
        }),
        Version = @event.Version,
      };
    }
  }
}
