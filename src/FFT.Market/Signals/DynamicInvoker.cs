// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.Signals
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;
  using static System.Reflection.BindingFlags;

  // simplified from
  // https://github.com/gautema/CQRSlite/blob/79b81005e124663a142afa1e170d50df75105467/Framework/CQRSlite/Infrastructure/DynamicInvoker.cs

  internal static class DynamicInvoker
  {
    private static readonly object _sync = new();

    private static volatile Dictionary<int, CompiledActionInfo> _cachedMembers = new();

    internal static void Invoke(this object target, string methodName, object arg)
    {
      var targetType = target.GetType();
      var argType = arg.GetType();
      var hash = Hash(targetType, methodName, argType);
      if (!_cachedMembers.TryGetValue(hash, out var method))
      {
        lock (_sync)
        {
          if (!_cachedMembers.TryGetValue(hash, out method))
          {
            var methodInfo = GetMethodInfo(targetType, methodName, argType);
            if (methodInfo is null)
              throw new Exception($"Unable to find method name '{methodName}' with arg type '{argType}' on '{targetType}'.");
            method = new CompiledActionInfo(targetType, methodInfo);
            _cachedMembers[hash] = method;
          }
        }
      }

      method.Invoke(target, arg);
    }

    private static int Hash(Type type, string methodname, Type argType)
    {
      HashCode hash = default;
      hash.Add(type);
      hash.Add(methodname);
      hash.Add(argType);
      return hash.ToHashCode();
    }

    private static MethodInfo? GetMethodInfo(Type type, string methodName, Type argType)
    {
      if (type is null)
        return null;

      foreach (var method in type.GetMethods(Instance | Public | NonPublic).Where(m => m.Name == methodName))
      {
        var methodParameters = method.GetParameters();
        if (methodParameters.Length == 1 && methodParameters[0].ParameterType == argType)
        {
          return method;
        }
      }

      if (type.BaseType is null)
        return null;

      return GetMethodInfo(type.BaseType, methodName, argType);
    }
  }
}
