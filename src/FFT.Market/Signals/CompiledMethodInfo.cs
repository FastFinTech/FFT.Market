// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Signals
{
  using System;
  using System.Linq.Expressions;
  using System.Reflection;

  // simplified from
  // https://github.com/gautema/CQRSlite/blob/79b81005e124663a142afa1e170d50df75105467/Framework/CQRSlite/Infrastructure/CompiledMethodInfo.cs

  internal class CompiledActionInfo
  {
    private readonly Action<object, object> _action;

    public CompiledActionInfo(Type targetType, MethodInfo methodInfo)
    {
      var instanceExpression = Expression.Parameter(typeof(object), "instance");
      var convertedInstanceExpression = Expression.Convert(instanceExpression, targetType);

      var argumentExpression = Expression.Parameter(typeof(object), "argument");
      var convertedArgumentExpression = Expression.Convert(argumentExpression, methodInfo.GetParameters()[0].ParameterType);

      var callExpression = Expression.Call(convertedInstanceExpression, methodInfo, convertedArgumentExpression);
      _action = Expression.Lambda<Action<object, object>>(callExpression, instanceExpression, argumentExpression).Compile();
    }

    public void Invoke(object instance, object arg)
    {
      _action(instance, arg);
    }
  }
}
