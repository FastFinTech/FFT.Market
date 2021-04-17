// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market
{
  using System.Diagnostics;
  using System.Runtime.CompilerServices;

  /// <summary>
  /// Provides extension methods for <see langword="uint"/> and <see
  /// langword="ulong"/> values that allow them to be used as bitwise flags.
  /// </summary>
  public static class FlagsExtensions
  {
    /// <summary>
    /// Returns <c>true</c> if any of the bits set in <paramref name="flags"/>
    /// are also set in <paramref name="value"/>, <c>false</c> otherwise.
    /// </summary>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAnyFlagSet(this uint value, uint flags)
      => (value & flags) > 0;

    /// <summary>
    /// Returns <c>true</c> if any of the bits set in <paramref name="flags"/>
    /// are also set in <paramref name="value"/>, <c>false</c> otherwise.
    /// </summary>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAnyFlagSet(this ulong value, ulong flags)
      => (value & flags) > 0;

    /// <summary>
    /// Returns <c>true</c> if ALL of the bits set in <paramref name="flags"/>
    /// are also set in <paramref name="value"/>, <c>false</c> otherwise.
    /// </summary>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAllFlagsSet(this uint value, uint flags)
      => (value & flags) == flags;

    /// <summary>
    /// Returns <c>true</c> if ALL of the bits set in <paramref name="flags"/>
    /// are also set in <paramref name="value"/>, <c>false</c> otherwise.
    /// </summary>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAllFlagsSet(this ulong value, ulong flags)
      => (value & flags) == flags;

    /// <summary>
    /// Modifies the value contained in <paramref name="value"/>, by setting all
    /// the bits that are set in <paramref name="flags"/>. IMPORTANT! <paramref
    /// name="value"/> is passed byref and is modified. Since this method is an
    /// extension method, the <c>ref</c> keyword is not used, and this may not
    /// be immediately obvious to the developer.
    /// </summary>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetFlags(ref this uint value, uint flags)
      => value |= flags;

    /// <summary>
    /// Modifies the value contained in <paramref name="value"/>, by setting all
    /// the bits that are set in <paramref name="flags"/>. IMPORTANT! <paramref
    /// name="value"/> is passed byref and is modified. Since this method is an
    /// extension method, the <c>ref</c> keyword is not used, and this may not
    /// be immediately obvious to the developer.
    /// </summary>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetFlags(ref this ulong value, ulong flags)
      => value |= flags;

    /// <summary>
    /// Modifies the value contained in <paramref name="value"/>, by unsetting all
    /// the bits that are set in <paramref name="flags"/>. IMPORTANT! <paramref
    /// name="value"/> is passed byref and is modified. Since this method is an
    /// extension method, the <c>ref</c> keyword is not used, and this may not
    /// be immediately obvious to the developer.
    /// </summary>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UnsetFlags(ref this uint value, uint flags)
      => value &= ~flags;

    /// <summary>
    /// Modifies the value contained in <paramref name="value"/>, by unsetting all
    /// the bits that are set in <paramref name="flags"/>. IMPORTANT! <paramref
    /// name="value"/> is passed byref and is modified. Since this method is an
    /// extension method, the <c>ref</c> keyword is not used, and this may not
    /// be immediately obvious to the developer.
    /// </summary>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UnsetFlags(ref this ulong value, ulong flags)
      => value &= ~flags;
  }
}
