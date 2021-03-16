// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.SourceGen
{
  using System;
  using System.Diagnostics;
  using System.Linq;
  using System.Text;
  using System.Threading;
  using Microsoft.CodeAnalysis;

  [Generator]
  public class KnownExchnagesGenerator : ISourceGenerator
  {
    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
#if DEBUG
      if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.DebugSourceGenerators", out var debugValue) &&
          bool.TryParse(debugValue, out var shouldDebug) &&
          shouldDebug)
      {
        Debugger.Launch();
      }
#endif

      var result = @"#pragma warning disable SA1633
namespace FFT.Market.Instruments
{
  [System.CodeDom.Compiler.GeneratedCode("""", """")]
  static partial class KnownExchanges
  {
[fields]  }
}
";

      var sb = new StringBuilder();
      foreach (var line in Properties.Resources.Exchanges.Split('\u2028').Skip(1))
      {
        if (string.IsNullOrEmpty(line)) continue;
        if (line.StartsWith("#")) continue;
        var shortName = line.Trim();
        sb.AppendLine($@"    public static readonly Exchange {shortName} = new Exchange
    {{
      ShortName = ""{shortName}"",
    }};");
      }

      result = result.Replace("[fields]", sb.ToString());
      Thread.Sleep(1);
      context.AddSource("KnownExchanges.cs", result);
    }
  }
}
