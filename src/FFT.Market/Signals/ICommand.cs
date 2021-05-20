// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Signals
{
  using System;
  using FFT.TimeStamps;

  /// <summary>
  /// Implement this interface to create commands that act on your
  /// Event-Sourcing domain objects.
  /// </summary>
  public interface ICommand
  {
    /// <summary>
    /// The id of the domain object that the command relates to.
    /// </summary>
    Guid AggregateId { get; }

    /// <summary>
    /// The version of the domain object as expected by the entity issuing the
    /// command. If this version does not match the actual version of the domain
    /// object, the command will be rejected and the entity issuing the command
    /// should refresh its view of the domain object's state.
    /// </summary>
    long ExpectedVersion { get; }

    /// <summary>
    /// The time that the command is issued.
    /// </summary>
    TimeStamp At { get; }
  }
}
