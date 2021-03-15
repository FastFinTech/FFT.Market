// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market
{
  using System;

  internal static class ExceptionHelpers
  {
    /// <summary>
    /// Creates a well formatted <see cref="NotImplementedException"/> explaining that the given value
    /// is not known. You'd most likely use it in a switch statement when handling enum values like this:
    /// <code>
    /// public void ProcessValue(SomeEnum value) {
    ///     switch(value) {
    ///         case SomeEnum.Value1: DoSomething(value); break;
    ///         case SomeEnum.Value2: DoSomething(value); break;
    ///         default: throw value.UnknownValue(); // <== <== Use this method to create the exception that needs to be thrown.
    ///     }
    /// }
    /// </code>
    /// </summary>
    public static NotImplementedException UnknownValueException<T>(this T value) where T : Enum
    {
      return new NotImplementedException($"Unknown '{typeof(T).Name}' value: '{value}'.");
    }

    /// <summary>
    /// Creates a well formatted <see cref="NotImplementedException"/> explaining that the given type
    /// is not known. You'd most likely use it in a switch statement when handling enum values like this:
    /// <code>
    /// public void ProcessValue(SomeEnum value) {
    ///     var result = value switch {
    ///         int x => DoSomethingWithInt(x),
    ///         string y => DoSomethingWithString(y);
    ///         default: throw value.UnknownValue(); // <== <== Use this method to create the exception that needs to be thrown.
    ///     }
    /// }
    /// </code>
    /// </summary>
    public static NotImplementedException UnknownTypeException(this Type type)
    {
      return new NotImplementedException($"Unknown type '{type.Name}'.");
    }
  }
}
