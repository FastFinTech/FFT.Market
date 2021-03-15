// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market
{
  using System;
  using System.Runtime.Serialization;

  [Serializable]
  public class ValidationException : Exception
  {
#pragma warning disable SA1502 // Element should not be on a single line
#pragma warning disable SA1128 // Put constructor initializers on their own line

    public ValidationException() { }

    public ValidationException(string message) : base(message) { }

    public ValidationException(string message, Exception inner) : base(message, inner) { }

    protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
