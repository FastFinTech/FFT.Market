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
      var argumentExpression = Expression.Parameter(typeof(object), "argument");

    }

    public void Invoke(object instance, object arg)
    {
      _action(instance, arg);
    }
  }

  internal class CompiledMethodInfo
  {
    private readonly Func<object, object[], object?> _func;

    public CompiledMethodInfo(MethodInfo methodInfo, Type type)
    {
      var instanceExpression = Expression.Parameter(typeof(object), "instance");
      var argumentsExpression = Expression.Parameter(typeof(object[]), "arguments");
      var parameterInfos = methodInfo.GetParameters();

      var argumentExpressions = new Expression[parameterInfos.Length];
      for (var i = 0; i < parameterInfos.Length; ++i)
      {
        var parameterInfo = parameterInfos[i];
        argumentExpressions[i] = Expression.Convert(Expression.ArrayIndex(argumentsExpression, Expression.Constant(i)), parameterInfo.ParameterType);
      }

      var callExpression = Expression.Call(!methodInfo.IsStatic ? Expression.Convert(instanceExpression, type) : null, methodInfo, argumentExpressions);
      if (callExpression.Type == typeof(void))
      {
        var action = Expression.Lambda<Action<object, object[]>>(callExpression, instanceExpression, argumentsExpression).Compile();
        _func = (instance, arguments) =>
        {
          action(instance, arguments);
          return null;
        };
      }
      else
      {
        _func = Expression.Lambda<Func<object, object[], object>>(Expression.Convert(callExpression, typeof(object)), instanceExpression, argumentsExpression).Compile();
      }
    }

    public object? Invoke(object instance, params object[] arguments)
    {
      return _func(instance, arguments);
    }
  }
}
