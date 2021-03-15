// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Apex.Tough.Instruments
{
  using FFT.TimeStamps;

  public sealed class Rollover
  {
    public DateStamp RolloverDate;
    public MonthStamp DeliveryMonth;
    public decimal? Offset;
  }
}
