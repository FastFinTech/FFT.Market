// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using Nito.AsyncEx;

  public static class IHaveErrorTaskExtensions
  {
    /// <summary>
    /// Asynchonously waits until the provider has an error or the cancellation token is canceled.
    /// If the provider reaches the error state, an exception will be thrown.
    /// Since <see cref="IHaveErrorTask"/> implementations are guaranteed to fault their <see cref="IHaveErrorTask.ErrorTask"/>
    /// tasks with an exception, this method is also guaranteed to throw an exception. The only question is whether the exception 
    /// will be <see cref="OperationCanceledException"/> due to token cancellation, or the exception that faulted the given <paramref name="provider"/>.
    /// </summary>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="ct"/> is canceled.</exception>
    [DebuggerStepThrough]
    public static async Task WaitForErrorAsync(this IHaveErrorTask provider, CancellationToken ct)
    {
      using var cts = new CancellationTokenTaskSource<object>(ct);
      var completedTask = await Task.WhenAny(cts.Task, provider.ErrorTask);
      await completedTask; // Actually throws the exception.
    }

    /// <summary>
    /// Asynchonously waits until at least one of the providers has an error or
    /// the cancellation token is canceled. If any provider reaches the error
    /// state, an exception will be thrown. Since <see cref="IHaveErrorTask"/>
    /// implementations are guaranteed to fault their <see
    /// cref="IHaveErrorTask.ErrorTask"/> tasks with an exception, this method
    /// is also guaranteed to throw an exception. The only question is whether
    /// the exception will be <see cref="OperationCanceledException"/> due to
    /// token cancellation, or the exception that faulted the given <paramref
    /// name="provider"/>.
    /// </summary>
    /// <exception cref="OperationCanceledException">Thrown when <paramref
    /// name="ct"/> is canceled.</exception>
    [DebuggerStepThrough]
    public static async Task WaitForErrorAsync(this IEnumerable<IHaveErrorTask> providers, CancellationToken ct)
    {
      // NB: Every single task here is guaranteed to be faulted with an exception.
      // Therefore we don't need to wait for all tasks to complete. The first completed task will do.
      using var cts = new CancellationTokenTaskSource<object>(ct);
      await await Task.WhenAny(providers.Select(p => p.ErrorTask).Append(cts.Task));
    }
  }
}
