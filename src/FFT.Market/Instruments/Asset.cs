// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Instruments
{
  using System.Diagnostics;

  [DebuggerDisplay("{Name}")]
  public sealed record Asset
  {
    public Asset(AssetType type, string name)
      => (Type, Name) = (type, name);

    public AssetType Type { get; init; }
    public string Name { get; init; }

    public static readonly Asset BitCoin = new Asset(AssetType.Crypto, "BitCoin");
  }
}
