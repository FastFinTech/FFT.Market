// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Instruments
{
  using System.Diagnostics;

  [DebuggerDisplay("{Name}")]
  public sealed record Asset
  {
    public Asset(AssetType type, string name, string usualSymbol)
      => (Type, Name, UsualSymbol) = (type, name, usualSymbol);

    public AssetType Type { get; init; }
    public string Name { get; init; }
    public string UsualSymbol { get; init; }
  }
}
