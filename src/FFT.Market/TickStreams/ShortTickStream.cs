// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.TickStreams
{
  using System;
  using System.Buffers;
  using System.Linq;
  using FFT.Market;
  using FFT.Market.Instruments;
  using FFT.Market.Ticks;
  using FFT.TimeStamps;
  using MessagePack;
  using Nerdbank.Streams;

  public sealed class ShortTickStream : ITickStream, IDisposable
  {
    private readonly double _tickSize;
    private readonly Sequence<byte> _sequence;

    private Tick? _previousTick;

    public ShortTickStream(IInstrument instrument)
    {
      _tickSize = instrument.TickSize;
      _sequence = new Sequence<byte>(ArrayPool<byte>.Shared);
      Instrument = instrument;
    }

    public ShortTickStream(IInstrument instrument, ReadOnlySpan<byte> bytes)
    {
      _tickSize = instrument.TickSize;
      _sequence = new Sequence<byte>(ArrayPool<byte>.Shared);
      _sequence.Write(bytes);
      Instrument = instrument;
      DataLength = _sequence.Length;
      var reader = CreateReader();
      for (var tick = reader.ReadNext(); tick is not null; tick = reader.ReadNext())
        _previousTick = tick;
    }

    public ShortTickStream(IInstrument instrument, Sequence<byte> sequence)
    {
      _tickSize = instrument.TickSize;
      _sequence = sequence;
      Instrument = instrument;
      DataLength = _sequence.Length;
      var reader = CreateReader();
      for (var tick = reader.ReadNext(); tick is not null; tick = reader.ReadNext())
        _previousTick = tick;
    }

    /// <inheritdoc />
    public IInstrument Instrument { get; }

    /// <inheritdoc />
    public long DataLength { get; private set; }

    /// <inheritdoc />
    public void WriteTick(Tick tick)
    {
      var writer = new MessagePackWriter(_sequence);
      if (_previousTick is null)
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

      writer.Flush();

      _previousTick = tick;

      // NB: Increment the DataLength property last, after all bytes are
      // written, so that multi-threaded reading, which checks the DataLength
      // property, doesn't start reading a tick before all fields are completely
      // written to the sequence.
      DataLength = _sequence.Length;
    }

    /// <summary>
    /// Gets a ReadOnlySequence for the underlying data. The ReadOnlySequence
    /// will only be valid until instance is disposed, when the underlying
    /// buffers are recycled to their memory pool. Call this method again to get
    /// new data when more ticks are written to the stream.
    /// </summary>
    /// <remarks>Don't call this method multi-threaded with the WriteTick
    /// method, as you may receive a sequnce that has a partially-written tick
    /// at the end.</remarks>
    public ReadOnlySequence<byte> AsReadOnlySequence()
      => _sequence.AsReadOnlySequence;

    /// <inheritdoc />
    public ITickStreamReader CreateReader()
      => new Reader(this);

    /// <inheritdoc />
    public ITickStreamReader CreateReaderFrom(TimeStamp timeStamp)
    {
      // This will EXCLUDE ticks at exactly "timeStamp".
      var reader = CreateReader();
      reader.ReadUntil(timeStamp).Count(); // don't forget to actually enumerate the enumerable with .Count() or similar
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

      private long _position = 0;
      private int _currentIndex = -1;
      private int _peekIndex = -1;
      private Tick _peekTick;
      private Tick _previousTick;

      public Reader(ShortTickStream parent)
      {
        _parent = parent;
        _tickSize = _parent._tickSize;
        Instrument = _parent.Instrument;
      }

      public IInstrument Instrument { get; }

      public long BytesRemaining => _parent.DataLength - _position;

      public Tick? PeekNext()
      {
        var index = _currentIndex + 1;
        if (_peekIndex == index) return _peekTick;
        if (BytesRemaining == 0) return null;
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

        if (BytesRemaining == 0)
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
            Instrument = Instrument,
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
            Instrument = Instrument,
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
