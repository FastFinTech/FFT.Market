// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.TickStreams
{
  using System;
  using System.Buffers;
  using System.Linq;
  using FFT.Market;
  using FFT.Market.Ticks;
  using FFT.TimeStamps;
  using MessagePack;
  using Nerdbank.Streams;

  public class ShortTickStream : ITickStream, IDisposable
  {
    private readonly Sequence<byte> _sequence = new Sequence<byte>(ArrayPool<byte>.Shared);
    private readonly double _tickSize;

    private Tick _previousTick;

    public ShortTickStream(TickStreamInfo info)
    {
      Info = info;
      _tickSize = Info.Instrument.TickSize;
    }

    public ShortTickStream(TickStreamInfo info, byte[] bytes)
      : this(info)
    {
      _sequence.Write(bytes);
    }

    public TickStreamInfo Info { get; }
    public int Count { get; private set; }
    public long DataLength => _sequence.Length;

    public void WriteTick(Tick tick)
    {
      var writer = new MessagePackWriter(_sequence);
      if (Count == 0)
      {
        writer.Write(tick.Price);
        writer.Write(tick.Bid);
        writer.Write(tick.Ask);
        writer.Write((ulong)tick.Volume);
        writer.Write((ulong)tick.TimeStamp.TicksUtc);
      }
      else
      {
        writer.Write((tick.Price - _previousTick.Price).ToTicks(_tickSize));
        writer.Write((tick.Bid - _previousTick.Bid).ToTicks(_tickSize));
        writer.Write((tick.Ask - _previousTick.Ask).ToTicks(_tickSize));
        writer.Write((ulong)tick.Volume);
        writer.Write((ulong)(tick.TimeStamp.TicksUtc - _previousTick.TimeStamp.TicksUtc));
      }

      _previousTick = tick;

      // NB: Increment the count property last, after all bytes are written, so that multi-threaded reading, which checks the Count property,
      // doesn't start reading a tick before all fields are completely written to the byte buffer.
      Count++;
    }

    public ReadOnlySequence<byte> AsReadOnlySequence()
      => _sequence.AsReadOnlySequence;

    public ITickStreamReader CreateReader()
      => new Reader(this);

    public ITickStreamReader CreateReaderFrom(TimeStamp timeStamp)
    {
      var reader = CreateReader();
      reader.ReadUntilJustBefore(timeStamp).Count(); // don't forget to actually enumerate the enumerable with .Count() or similar
      return reader;
    }

    public void Dispose()
    {
      _sequence.Reset();
    }

    private class Reader : ITickStreamReader
    {
      private readonly ShortTickStream _parent;
      private readonly double _tickSize;

      private long _position;
      private int _currentIndex = -1;
      private int _peekIndex = -1;
      private Tick _peekTick;
      private Tick _previousTick;

      public Reader(ShortTickStream parent)
      {
        _parent = parent;
        Info = _parent.Info;
        _tickSize = _parent._tickSize;
      }

      public TickStreamInfo Info { get; }

      public long BytesRemaining => _parent._sequence.Length - _position;

      public Tick? PeekNext()
      {
        var index = _currentIndex + 1;
        if (_peekIndex == index) return _peekTick;
        if (index >= _parent.Count) return null;
        _peekTick = Extract();
        _peekIndex = index;
        return _peekTick;
      }

      public Tick? ReadNext()
      {
        var index = _currentIndex + 1;
        if (index == _peekIndex)
        {
          _currentIndex = _peekIndex;
          return _peekTick;
        }

        if (index >= _parent.Count)
          return null;

        _currentIndex = index;
        return Extract();
      }

      private Tick Extract()
      {
        var reader = new MessagePackReader(_parent.AsReadOnlySequence().Slice(_position));
        if (_previousTick is null)
        {
          var price = reader.ReadDouble();
          var bid = reader.ReadDouble();
          var ask = reader.ReadDouble();
          var volume = (double)reader.ReadUInt64();
          var timeStamp = new TimeStamp((long)reader.ReadUInt64());

          _position += reader.Consumed;
          return _previousTick = new Tick
          {
            Info = Info,
            Price = price,
            Bid = bid,
            Ask = ask,
            Volume = volume,
            TimeStamp = timeStamp,
          };
        }
        else
        {
          var price = _previousTick.Price.AddTicks(_tickSize, reader.ReadInt32());
          var bid = _previousTick.Bid.AddTicks(_tickSize, reader.ReadInt32());
          var ask = _previousTick.Ask.AddTicks(_tickSize, reader.ReadInt32());
          var volume = (double)reader.ReadUInt64();
          var timeStamp = _previousTick.TimeStamp.AddTicks((long)reader.ReadUInt64());

          _position += reader.Consumed;
          return _previousTick = new Tick
          {
            Info = Info,
            Price = price,
            Bid = bid,
            Ask = ask,
            Volume = volume,
            TimeStamp = timeStamp,
          };
        }
      }
    }
  }
}
