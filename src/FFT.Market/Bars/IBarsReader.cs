// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Bars
{
  public interface IBarsReader
  {
    long BarsRemaining { get; }
    IBar PeekNext();
    IBar ReadNext();
  }
}
