// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.SourceGen
{
  using System.Diagnostics;
  using Microsoft.CodeAnalysis;

  internal static class DebuggerStuff
  {
    private static readonly object _sync = new();
    private static bool _runOnce = false;

    public static void LaunchOnce(GeneratorExecutionContext context)
    {
#if DEBUG
      lock (_sync)
      {
        if (_runOnce) return;
        _runOnce = true;
        if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.DebugSourceGenerators", out var debugValue) &&
            bool.TryParse(debugValue, out var shouldDebug) &&
            shouldDebug)
        {
          Debugger.Launch();
        }
      }
#endif
    }
  }
}
