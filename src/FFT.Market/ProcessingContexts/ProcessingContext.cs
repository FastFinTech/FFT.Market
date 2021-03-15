// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.ProcessingContexts
{
  using System;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using FFT.Market.BarBuilders;
  using FFT.Market.Bars;
  using FFT.Market.DependencyTracking;
  using FFT.Market.Engines;
  using FFT.Market.TickStreams;
  using FFT.TimeStamps;

  public class ProcessingContext : IHaveDependencies, IHaveReadyTask, IHaveErrorTask
  {
    private readonly CancellationTokenSource _disposed = new();
    private readonly TaskCompletionSource _readyTCS = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _errorTCS = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly CancellationToken _disposedToken;

    private readonly object _sync = new();
    private List<BarBuilder> _barBuilders = new();
    private List<EngineBase> _engines = new();

    public ProcessingContext(TimeStamp processFromTime, string? name = null)
    {
      _disposedToken = _disposed.Token;
      ProcessFromTime = processFromTime;
      State = ProcessingContextState.Initializing;
      Name = name ?? string.Empty;
    }

    public TimeStamp ProcessFromTime { get; }

    public ProcessingContextState State { get; private set; }
    public Exception Exception { get; private set; }
    public bool IsDisposed { get; private set; }
    public string Name { get; private set; }

    /// <inheritdoc/>
    public Task ReadyTask => _readyTCS.Task;

    /// <inheritdoc/>
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



    IProvider[] _providers;
    ILiveTickProvider[] _tickProviders;
    ITickStreamReader _tickReader;

    /// <inheritdoc/>
    public IEnumerable<object> GetDependencies()
    {
      foreach (var bars in BarsProvider.BarBuilders.Select(bb => bb.Bars))
        yield return bars;
      foreach (var engine in EngineProvider.Engines)
        yield return engine;
    }

    public void Start() => Task.Run(Work);

    async Task Work()
    {
      var barBuilders = _barBuilders.Values.ToArray();
      try
      {
        foreach (var bb in barBuilders)
        {
          if (bb.BarsInfo.TradingHours.GetActualSessionAt(ProcessFromTime.AddTicks(1)).SessionDate != bb.BarsInfo.FirstSessionDate)
          {
            throw new Exception("Bar Builder's 'FirstSessionDate' does not match processing context ProcessFromTime");
          }
        }

        var tickStreams = this.GetNonProviderTickStreamDependenciesRecursive().ToArray();
        if (tickStreams.Length == 0) throw new Exception("At least one tick stream is required.");

        if (string.IsNullOrWhiteSpace(Name))
        {
          var instrumentNames = string.Join(", ", tickStreams.Select(x => x.Instrument.NinjaTraderSymbol()).Distinct());
          var processFromDate = ProcessFromTime.GetDate(); // rough, since it's just a utc date irrespective of timezone of actual processing start.
          var daysAgo = TradingPlatformTime.Now.GetDate().GetDaysSince(processFromDate);
          Name = $"Processing Context {instrumentNames} from {processFromDate}, {daysAgo} days ago";
        }

        _providers = this.GetDependenciesRecursive().Where(x => x is IProvider).Cast<IProvider>().ToArray();

        /// This limit is required to prevent an exception when the live tick provider places a restriction on the "first session date"
        /// property in its info object.
        var firstSessionDateLimit = TradingPlatformTime.Now.GetDate(TradingPlatformTime.TimeZone).AddDays(-1);
        _tickProviders = tickStreams.Select(x => Services.LiveTickProviderFactory.Get(new LiveTickProviderInfo
        {
          FirstSessionDate = x.TradingSessions.GetActualSessionsFrom(ProcessFromTime.AddTicks(1)).First().SessionDate.OrValueIfLesser(firstSessionDateLimit),
          Instrument = x.Instrument,
          TradingSessions = x.TradingSessions,
        })).ToArray();

        State = ProcessingContextState.Loading;

        await _providers
            .Concat(_tickProviders)
            .WaitForReadyAsync(TimeSpan.FromMinutes(30)).ConfigureAwait(false);

        /// Combine each of the tick streams into a single reader.
        _tickReader = new CombinedTickStreamReader(_tickProviders.Select(p => p.CreateReader()));

        /// Since the streams all have different start times due to different session templates,
        /// we need to initialize the reader by fast-forwarding it to the processing context's start time.
        _tickReader.ReadUntil(ProcessFromTime);

        State = ProcessingContextState.ProcessingHistorical;

        var count = 0;
        foreach (var tick in _tickReader.ReadRemaining())
        {
          if (count++ == 1000000)
          {
            _disposedToken.ThrowIfCancellationRequested();
            count = 0;
          }
          BarsProvider.ProcessTick(tick);
          EngineProvider.ProcessTick(tick);
        }

        State = ProcessingContextState.ProcessingLive;
        FireAndForget(() => _readyTCS.TrySetResult());

        while (true)
        {
          _providers.ThrowIfAnyHasError();
          _tickProviders.ThrowIfAnyHasError();
          foreach (var tick in _tickReader.ReadRemaining())
          {
            BarsProvider.ProcessTick(tick);
            EngineProvider.ProcessTick(tick);
          }
          await Task.Delay(50, _disposedToken).ConfigureAwait(false);
        }
      }
      catch (OperationCanceledException)
      {
      }
      catch (Exception x)
      {
        OnError(x);
      }
    }

    void OnError(Exception x)
    {
      if (Interlocked.CompareExchange(ref _errorSet, 1, 0) == 1) return;
      Exception = x;
      State = ProcessingContextState.Error;
      _errorTCS.TrySetException(Exception);
      _readyTCS.TrySetException(Exception);
      _readyTCS = new TaskCompletionSource();
      _readyTCS.TrySetException(Exception);
      Dispose();
    }

    public void Dispose()
    {
      if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1) return;
      _disposed.Cancel();
      _disposed.Dispose();
      OnError(new Exception("Disposed"));
    }

    public ProviderStatus GetStatus()
    {
      var status = new ProviderStatus();

      status.ProviderName = State == ProcessingContextState.Initializing
          ? "Initializing"
          : Name;

      status.StatusMessage = State switch
      {
        ProcessingContextState.Initializing => "Initializing",
        ProcessingContextState.Loading => "Loading",
        ProcessingContextState.ProcessingHistorical => $"Processing historical data ({_tickReader.BytesRemaining} bytes remaining)",
        ProcessingContextState.ProcessingLive => "Processing live data",
        ProcessingContextState.Error => "Error: " + Exception.GetUnwoundMessage("==>"),
        _ => throw new Exception($"Unknown {nameof(ProcessingContextState)} '{State}'.")
      };

      if (State == ProcessingContextState.Loading)
      {
        status.InternalProviders = _tickProviders.Select(x => x.GetStatus()).Concat(_providers.Select(x => x.GetStatus())).ToArray();
      }

      return status;
    }
  }
}
