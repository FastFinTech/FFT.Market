// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Providers
{
  using System;
  using System.Collections.Generic;
  using System.Threading;
  using System.Threading.Tasks;
  using FFT.Disposables;
  using FFT.Market.UsageTracking;

  /// <summary>
  /// This is a convenience, utility class that provides boiler-plate code used
  /// by many providers. You don't have to inherit this class to create a
  /// provider, if it doesn't suit your provider's requirements. Instead, you
  /// can just implement the <see cref="IProvider{T}"/> interface.
  /// Implementing classes auto-dispose themselves when they reach error state.
  /// If they are disposed by user code, they change to error state.
  /// </summary>
  public abstract class ProviderBase : DisposeBase, IProvider
  {
    private readonly object _sync = new();
    private readonly TaskCompletionSource _readyTCS = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _errorTCS = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly UserTokenMonitor _userTokenMonitor = new();

    public ProviderBase()
    {
      _userTokenMonitor.UserCountZero += () => Dispose(new Exception("User count is zero."));
    }

    public string Name { get; protected set; }

    public ProviderStates State { get; private set; } = ProviderStates.Loading;

    public Task ReadyTask => _readyTCS.Task;

    public Task ErrorTask => _errorTCS.Task;

    public Exception? Exception => DisposalReason;

    public abstract void Start();

    public IDisposable GetUserCountToken() => _userTokenMonitor.GetUserCountToken();

    public abstract IEnumerable<object> GetDependencies();

    public abstract ProviderStatus GetStatus();

    protected void OnReady()
    {
      lock (_sync)
      {
        if (State == ProviderStates.Loading)
        {
          State = ProviderStates.Ready;
          _readyTCS.TrySetResult();
        }
      }
    }

    protected sealed override void CustomDispose()
    {
      OnDisposing();
      lock (_sync)
      {
        State = ProviderStates.Error;
        _errorTCS.TrySetException(DisposalReason!);
        _readyTCS.TrySetException(DisposalReason!);
      }

      OnDisposed();
    }

    protected virtual void OnDisposing() { }
    protected virtual void OnDisposed() { }
  }
}
