// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.SourceGen
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading;
  using Microsoft.CodeAnalysis;

  [Generator]
  public class KnownAssetsGenerator : ISourceGenerator
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
  public static partial class KnownAssets
  {
[fields]  }
}
";

      var sb = new StringBuilder();
      foreach (var line in Properties.Resources.Assets.Split('\n').Skip(1))
      {
        if (string.IsNullOrEmpty(line)) continue;
        if (line.StartsWith("#")) continue;
        var parts = line.Split(',');
        var type = parts[0];
        var name = parts[1];
        var nameCode = parts[1].CleanIdentifierName();
        var usualSymbol = parts[2];
        sb.AppendLine($@"    public static readonly Asset {type}_{nameCode} = new Asset(AssetType.{type}, ""{name}"", ""{usualSymbol}"");");
      }

      result = result.Replace("[fields]", sb.ToString());
      Thread.Sleep(1);
      context.AddSource("KnownAssets.cs", result);
    }
  }
}
