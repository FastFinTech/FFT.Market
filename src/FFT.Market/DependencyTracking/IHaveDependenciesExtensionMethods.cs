// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.DependencyTracking
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using FFT.Market.Providers;
  using FFT.Market.TickStreams;

  public static class IHaveDependenciesExtensionMethods
  {

    /// <summary>
    /// Gets all of the dependencies of the given <paramref name="target"/>
    /// and all the dependencies' dependencies in a recursive manner. You
    /// can optionally supply the <paramref name="include"/> predicate to
    /// filter out the dependencies (and their dependencies) that you don't
    /// want included.
    /// </summary>
    public static HashSet<object> GetDependenciesRecursive(this IHaveDependencies target, Func<object, bool>? include = null)
    {
      var items = new HashSet<object>();
      if (target is null) return items;
      include ??= x => true;
      Recurse(target, include, items);
      return items;

      static void Recurse(IHaveDependencies target, Func<object, bool> include, HashSet<object> items)
      {
        foreach (var item in target.GetDependencies())
        {
          if (item is not null && include(item) && items.Add(item) && item is IHaveDependencies nextGeneration)
            Recurse(nextGeneration, include, items);
        }
      }
    }

    /// <summary>
    /// A quick and easy way to find out all the tick stream dependences of the given <paramref name="target"/>,
    /// excluding any required by an <see cref="IProvider"/>.
    /// A typical use of this method is to find out what tickstreams are required when initializing a processing context,
    /// when we want to exclude the tickstreams needed by providers (as the providers take care of their own tick streams).
    /// </summary>
    public static IEnumerable<TickStreamInfo> GetNonProviderTickStreamDependenciesRecursive(this IHaveDependencies target)
      => target.GetDependenciesRecursive(d => !(d is IProvider)).OfType<TickStreamInfo>();
  }
}
