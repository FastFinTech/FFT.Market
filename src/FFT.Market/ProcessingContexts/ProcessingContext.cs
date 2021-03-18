// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.ProcessingContexts
{
  using System;
  using System.Collections.Generic;
  using System.Collections.Immutable;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using FFT.Disposables;
  using FFT.Market.BarBuilders;
  using FFT.Market.Bars;
  using FFT.Market.DependencyTracking;
  using FFT.Market.Engines;
  using FFT.Market.Providers;
  using FFT.Market.Providers.Ticks;
  using FFT.Market.TickStreams;
  using FFT.TimeStamps;
  using static FFT.Market.Services.ServiceProvider;

  public class ProcessingContext : DisposeBase, IHaveDependencies, IHaveReadyTask, IHaveErrorTask
  {
    private readonly TaskCompletionSource _readyTCS = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _errorTCS = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly object _sync = new();
    private readonly List<BarBuilder> _barBuilders = new();
    private readonly List<EngineBase> _engines = new();
    private readonly List<IProvider> _providers = new();
    private readonly List<ITickProvider> _tickProviders = new();

    private ITickStreamReader _tickReader;

    public ProcessingContext(TimeStamp firstSessionStartTime, string? name = null)
    {
      FirstSessionStartTime = firstSessionStartTime;
      State = ProcessingContextState.Initializing;
      Name = name ?? string.Empty;
    }

    public TimeStamp FirstSessionStartTime { get; }

    public ProcessingContextState State { get; private set; }

    public string Name { get; private set; }

    public Task ReadyTask => _readyTCS.Task;

    public Task ErrorTask => _errorTCS.Task;

    public IBars GetBars(BarsInfo barsInfo)
    {
      if (State != ProcessingContextState.Initializing)
        throw new InvalidOperationException("Must be in initializing state.");

      lock (_sync)
      {
        foreach (var existingBarBuilder in _barBuilders)
        {
          if (existingBarBuilder.BarsInfo.Equals(barsInfo))
            return existingBarBuilder.Bars;
        }

        var barBuilder = BarBuilder.Create(barsInfo);
        _barBuilders.Add(barBuilder);
        return barBuilder.Bars;
      }
    }

    public T GetEngine<T>(Func<T, bool> search, Func<ProcessingContext, T> create)
      where T : EngineBase
    {
      if (State != ProcessingContextState.Initializing)
        throw new InvalidOperationException("Must be in initializing state.");

      lock (_sync)
      {
        foreach (var existingEngine in _engines)
        {
          if (existingEngine is T possibleMatch && search(possibleMatch))
            return possibleMatch;
        }

        var engine = create(this);
        _engines.Add(engine);
        return engine;
      }
    }

    public IEnumerable<object> GetDependencies()
    {
      foreach (var bars in _barBuilders.Select(bb => bb.Bars))
        yield return bars;
      foreach (var engine in _engines)
        yield return engine;
      foreach (var provider in _providers)
        yield return provider;
    }

    public void Start() => Task.Run(WorkAsync);

    public ProviderStatus GetStatus()
    {
      return new ProviderStatus
      {
        ProviderName = State == ProcessingContextState.Initializing
          ? "Initializing"
          : Name,

        StatusMessage = State switch
        {
          ProcessingContextState.Initializing => "Initializing",
          ProcessingContextState.Loading => "Loading",
          ProcessingContextState.ProcessingHistorical => $"Processing historical data ({_tickReader.BytesRemaining} bytes remaining)",
          ProcessingContextState.ProcessingLive => "Processing live data",
          ProcessingContextState.Error => "Error: " + DisposalReason!.GetUnwoundMessage(),
          _ => throw State.UnknownValueException(),
        },

        InternalProviders = State == ProcessingContextState.Loading
          ? _tickProviders.Select(x => x.GetStatus()).Concat(_providers.Select(x => x.GetStatus())).ToImmutableList()
          : ImmutableList<ProviderStatus>.Empty,
      };
    }

    protected override void CustomDispose()
    {
      State = ProcessingContextState.Error;
      _errorTCS.TrySetException(DisposalReason!);
      _readyTCS.TrySetException(DisposalReason!);
    }

    private async Task WorkAsync()
    {
      try
      {
        foreach (var bb in _barBuilders)
        {
          if (bb.BarsInfo.TradingHours.GetActualSessionAt(FirstSessionStartTime.AddTicks(1)).SessionDate != bb.BarsInfo.FirstSessionDate)
          {
            throw new Exception("Bar Builder's 'FirstSessionDate' does not match processing context ProcessFromTime");
          }
        }

        var tickStreams = this.GetNonProviderTickStreamDependenciesRecursive().ToArray();
        if (tickStreams.Length == 0) throw new Exception("At least one tick stream is required.");

        if (string.IsNullOrWhiteSpace(Name))
        {
          var instrumentNames = string.Join(", ", tickStreams.Select(x => x.Instrument.Name).Distinct());
          var processFromDate = FirstSessionStartTime.GetDate(); // rough, since it's just a utc date irrespective of timezone of actual processing start.
          var daysAgo = TradingPlatformTime.Now.GetDate().GetDaysSince(processFromDate);
          Name = $"Processing Context {instrumentNames} from {processFromDate}, {daysAgo} days ago";
        }

        _providers.AddRange(this.GetDependenciesRecursive().Where(x => x is IProvider).Cast<IProvider>());

        _tickProviders.AddRange(
          tickStreams.Select(
            x => TickProviderFactory.GetTickProvider(
              new TickProviderInfo
              {
                From = x.TradingSessions.GetActualSessionAt(FirstSessionStartTime).SessionStart.ToDayFloor().AddTicks(1),
                Instrument = x.Instrument,
                Until = null,
              })));

        State = ProcessingContextState.Loading;

        using var waitSource = new CancellationTokenSource(TimeSpan.FromMinutes(30));
        using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(DisposedToken, waitSource.Token);
        var allProviders = _providers.ToList();
        allProviders.AddRange(_tickProviders);
        await allProviders.WaitForReadyAsync(linkedSource.Token);

        // Combine each of the tick streams into a single reader.
        _tickReader = new CombinedTickStreamReader(_tickProviders.Select(p => p.CreateReader()).ToArray());

        State = ProcessingContextState.ProcessingHistorical;

        var count = 0;
        foreach (var tick in _tickReader.ReadRemaining())
        {
          if (count++ == 1000000)
          {
            DisposedToken.ThrowIfCancellationRequested();
            allProviders.ThrowIfAnyHasError();
            count = 0;
          }

          foreach (var builder in _barBuilders)
            builder.OnTick(tick);
          foreach (var engine in _engines)
            engine.OnTick(tick);
        }

        State = ProcessingContextState.ProcessingLive;
        _readyTCS.TrySetResult();

        while (true)
        {
          allProviders.ThrowIfAnyHasError();
          foreach (var tick in _tickReader.ReadRemaining())
          {
            foreach (var builder in _barBuilders)
              builder.OnTick(tick);
            foreach (var engine in _engines)
              engine.OnTick(tick);
          }

          await Task.Delay(50, DisposedToken);
        }
      }
      catch (Exception x)
      {
        Dispose(x);
      }
    }
  }
}
