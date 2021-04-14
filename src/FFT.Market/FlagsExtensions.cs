// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market
{
  using System.Diagnostics;
  using System.Runtime.CompilerServices;

  public static class FlagsExtensions
  {
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAnyFlagSet(this uint value, uint flags)
      => (value & flags) > 0;

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAnyFlagSet(this ulong value, ulong flags)
      => (value & flags) > 0;

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAllFlagsSet(this uint value, uint flags)
      => (value & flags) == flags;

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAllFlagsSet(this ulong value, ulong flags)
      => (value & flags) == flags;

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetFlags(ref this uint value, uint flags)
      => value |= flags;

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetFlags(ref this ulong value, ulong flags)
      => value |= flags;

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UnsetFlags(ref this uint value, uint flags)
      => value &= ~flags;

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UnsetFlags(ref this ulong value, ulong flags)
      => value &= ~flags;
  }
}
