// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.TickStreams
{
  using System;
  using System.Linq;
  using FFT.Market.Ticks;

  public sealed class ConcatenatedTickStreamReader : ITickStreamReader
  {
    private readonly ITickStreamReader[] _readers;

    private int _currentReaderIndex;

    private ITickStreamReader _currentReader;

    public ConcatenatedTickStreamReader(params ITickStreamReader[] readers)
    {
      if (readers is not { Length: > 0 }) throw new ArgumentException(nameof(readers));
      Info = readers[0].Info;
      foreach (var reader in readers.Skip(1))
      {
        if (!reader.Info.Equals(Info))
          throw new Exception("Tick stream info does not match.");
      }

      _readers = readers;
      _currentReaderIndex = 0;
      _currentReader = readers[0];
    }

    public TickStreamInfo Info { get; }

    public long BytesRemaining
    {
      get
      {
        var total = 0L;
        for (var i = _currentReaderIndex; i < _readers.Length; i++)
          total += _readers[i].BytesRemaining;
        return total;
      }
    }

    public Tick? PeekNext()
    {
      var tick = _currentReader.PeekNext();
      if (tick is not null) return tick;
      if (!MoveToNextReader()) return null;
      return PeekNext();
    }

    public Tick? ReadNext()
    {
      var tick = _currentReader.ReadNext();
      if (tick is not null) return tick;
      if (!MoveToNextReader()) return null;
      return ReadNext();
    }

    private bool MoveToNextReader()
    {
      if (_currentReaderIndex >= _readers.Length - 1) return false;
      _currentReaderIndex++;
      _currentReader = _readers[_currentReaderIndex];
      return true;
    }
  }
}
