// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Apex.Tough.Instruments
{
  using FFT.TimeStamps;

  public interface IFutureContract : IInstrument
  {
    IFutureMaster FutureMaster { get; }

    DateStamp RolloverDate { get; }

    MonthStamp DeliveryMonth { get; }

    bool IsContinousContract { get; }
  }
}
