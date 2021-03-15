// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Providers
{
  using System.Collections.Immutable;
  using System.Linq;
  using System.Text;

  /// <summary>
  /// A class used for presenting IProvider status / debug information to the
  /// user. This class is NOT supposed to be used by code for checking the state
  /// of a provider, because the method to create it is inefficient, and the
  /// IProvider has a "State" property that is more suitable.
  /// </summary>
  public sealed record ProviderStatus
  {
    public string ProviderName { get; init; }
    public string StatusMessage { get; init; }
    public ImmutableList<ProviderStatus>? InternalProviders { get; init; }

    public override string ToString()
    {
      var sb = new StringBuilder();
      sb.AppendLine("Name: " + ProviderName);
      sb.AppendLine("Status: " + StatusMessage);
      if (InternalProviders is not null)
      {
        foreach (var provider in InternalProviders)
        {
          var status = provider.ToString();
          foreach (var line in status.Split('\n'))
          {
            sb.AppendLine("|  " + line.Trim());
          }
        }
      }

      return sb.ToString();
    }
  }
}
