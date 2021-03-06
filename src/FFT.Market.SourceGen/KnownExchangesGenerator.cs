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
  public class KnownExchangesGenerator : ISourceGenerator
  {
    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
      DebuggerStuff.LaunchOnce(context);

      var result = @"#pragma warning disable SA1633
namespace FFT.Market.Instruments
{
  [System.CodeDom.Compiler.GeneratedCode("""", """")]
  public static partial class KnownExchanges
  {
[fields]  }
}
";

      var sb = new StringBuilder();
      foreach (var line in Properties.Resources.Exchanges.Split('\n').Skip(1))
      {
        if (string.IsNullOrEmpty(line)) continue;
        if (line.StartsWith("#")) continue;
        var shortName = line.Trim();
        // long names are not yet added to the resources file.
        sb.AppendLine($@"    public static readonly Exchange {shortName} = new Exchange(""{shortName}"", ""{shortName}"");");
      }

      result = result.Replace("[fields]", sb.ToString());
      Thread.Sleep(1);
      context.AddSource("KnownExchanges.cs", result);
    }
  }
}
