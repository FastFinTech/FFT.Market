// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market
{
  using System.Threading.Tasks;

  /// <summary>
  /// Implementing classes provide the <see cref="ReadyTask"/> property.
  /// </summary>
  public interface IHaveReadyTask
  {
    /// <summary>
    /// This task is completed when the implementing class switches over to
    /// "ready" state. Implementing classes must guarantee the following:
    /// <para>
    /// 1. If the implementing class switches over to "error" state before it
    /// becomes ready, this task will become faulted with the same exception
    /// that caused the error state.
    /// </para>
    /// <para>
    /// 2. If the implementing class reaches "ready" state and then switches to
    /// "error" state, this task will remain succesfully completed.
    /// </para>
    /// <para>
    /// 3. Disposal of the implementing class is an error state.
    /// </para>
    /// </summary>
    Task ReadyTask { get; }
  }
}
