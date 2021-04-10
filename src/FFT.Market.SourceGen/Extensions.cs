// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.SourceGen
{
  using System.Linq;

  internal static class Extensions
  {
    public static string CleanIdentifierName(this string value)
    {
      var result = new string(value.Where(IsValidChar).ToArray());
      while (result.Contains("__"))
        result = result.Replace("__", "_");
      while (result.EndsWith("_"))
        result = result.Substring(0, result.Length - 1);
      while (result.StartsWith("_"))
        result = result.Substring(1);
      return result;
    }

    private static bool IsValidChar(this char value)
      => value is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or (>= '0' and <= '9') or '_';
  }
}
