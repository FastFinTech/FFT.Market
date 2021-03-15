// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market
{
  using System.Threading.Tasks;

  /// <summary>
  /// Implementing classes provide the <see cref="ErrorTask"/> property with
  /// strict guarantees about how the task is completed.
  /// </summary>
  public interface IHaveErrorTask
  {

    /// <summary>
    /// This task is completed WITH AN EXCEPTION when implementing classes pass into an error state.
    /// When implementing, you must guarantee that:
    /// 1. This task is faulted with an exception, and
    /// 2. This task is faulted when the implementing class is disposed or garbage collected.
    /// </summary>
    Task ErrorTask { get; }
  }
}
