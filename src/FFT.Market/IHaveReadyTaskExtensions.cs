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

  public static class IHaveReadyTaskExtensions
  {
    /// <summary>
    /// Asynchonously waits for the given <paramref name="provider"/> to reach
    /// its ready state.
    /// If the cancellation token is cancelled first, its task will throw an OperationCanceledException.
    /// If the provider reaches error state first, its task will throw the exception that caused the provider error.
    /// If the provider reaches ready state first, no exception will be thrown.
    /// </summary>
    [DebuggerStepThrough]
    public static async Task WaitForReadyAsync(this IHaveReadyTask provider, CancellationToken ct)
    {
      using var cts = new CancellationTokenTaskSource<object>(ct);
      await await Task.WhenAny(cts.Task, provider.ReadyTask);
    }

    /// <summary>
    /// Asynchonously waits for all the given providers to reach their ready state.
    /// If the cancellation token is cancelled first, its task will throw an OperationCanceledException.
    /// If any provider reaches error state first, its task will throw the exception that caused the provider error.
    /// If all providers reache ready state first, no exception will be thrown.
    /// </summary>
    [DebuggerStepThrough]
    public static async Task WaitForReadyAsync(this IEnumerable<IHaveReadyTask> providers, CancellationToken ct)
    {
      using var cts = new CancellationTokenTaskSource<object>(ct);
      var tasks = providers.Select(p => p.ReadyTask).Append(cts.Task).ToList();

      // Wait until all the tasks have completed, immediately throwing an
      // exception if any of the tasks fails. The cancellation task will be the
      // last task remaining, if it is not cancelled, so we wait for all but the
      // last task to complete.
      while (tasks.Count > 1)
      {
        // the required exception throwing immediacy is the reason we dont use await Task.WhenAll
        var completedTask = await Task.WhenAny(tasks);
        await completedTask; // throws the exception if it exists (including OperationCanceledException from the cancellation token)
        tasks.Remove(completedTask);
      }
    }
  }
}
