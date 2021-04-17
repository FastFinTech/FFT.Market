// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market
{
  using System;
  using System.Text;

  public static class ExceptionExtensions
  {
    public static string GetUnwoundMessage(this Exception x, string delimiter = " ==> ")
    {
      var sb = new StringBuilder(x.Message);
      for (var inner = x.InnerException; inner is not null; inner = x.InnerException)
      {
        sb.Append(delimiter);
        sb.Append(inner.Message);
      }

      return sb.ToString();
    }
  }
}
