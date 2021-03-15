using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace FFT.Market.DataSeries
{

  /// <summary>
  /// This <see cref="ImmutableLimitedSeries{T}"/> allows engines and providers to create lists that can be enumerated
  /// in a thread-safe manner by other engines and indicators.
  /// Engines update the "immutable" series by replacing it with each update.
  /// <code>
  /// // to create the list
  /// this.DataPoints = ImmutableLimitedSeries<MyDataPoint>.Create();
  /// // to add a new datapoint to the list (replacing the previous list)
  /// this.DataPoints = this.DataPoints.Add(new MyDataPoint());
  /// </code>
  /// Other engines and indicators can enumerate it at any time from any thread with complete safety.
  /// The <see cref="ImmutableLimitedSeries{T}"/> is created with an initial <see cref="MaxLookBack"/> of only 256 items,
  /// to save RAM. Other engines and indicators can request increasing the capacity when necessary in the following manner:
  /// <code>
  /// public MyEngine(ProcessingContext processingContext, MyEngineSettings settings) { 
  ///   // create a dependency engine (or retrieve it from the already-existing engines in the processing context)
  ///   var engineINeed = processingContext.GetSomeEngineINeed(someSettings);
  ///   // request that engine's data points to increase the maximum look back, because we need to access more history (rare circumstance)
  ///   engineINeed.YourDataPoints.IncreaseMaxLookBack(20000);
  /// }
  /// </code>
  /// Conspicous in this code is the absence of any method for modifying a value that has already been added.
  /// If a "set" method were called on an immutable type such as this for every tick of data, there
  /// would be a tremendous amount of work done in allocating and garbage-collecting immutable lists
  /// replaced for every tick of data. Best practise is to add a mutable datapoint and mutate that datapoint's values
  /// in response to incoming tick data.
  /// </summary>
  public sealed class ImmutableLimitedSeries<T>
  {
    private readonly ImmutableList<T> _items;

    private ImmutableLimitedSeries(ImmutableList<T> items, int numItemsRemoved, int maxLookBack)
    {
      _items = items;
      NumItemsRemoved = numItemsRemoved;
      MaxLookBack = maxLookBack;
    }

    public int NumItemsRemoved { get; }

    public int MaxLookBack { get; private set; }

    public static ImmutableLimitedSeries<T> Create()
        => new ImmutableLimitedSeries<T>(ImmutableList<T>.Empty, 0, 256);

    /// <summary>
    /// Gets the total number of items that have been added (but not the total
    /// available for enumeration), as the list has been trimmed to meet the
    /// <see cref="MaxLookBack"/> requirements.
    /// </summary>
    public int Count
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _items.Count + NumItemsRemoved;
    }

    public T Last
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _items[_items.Count - 1];
    }

    /// <summary>
    /// Gets the item stored at the given index, where "index" is treated as the
    /// position if no items were removed. Therefore, if 10 items have been
    /// removed from this list already, and this property is called with index
    /// equal to 9 or less, an IndexOutOfRangeException would be thrown.
    /// </summary>
    public T this[int index]
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _items[index - NumItemsRemoved];
    }

    /// <summary>
    /// This method is intended to be called by all the objects that will need
    /// to read this dataseries, if they need to access a long look-back
    /// history. This method call departs from the usual coding pattern of an
    /// immutable object in the sense that it actually mutates the object!!! But
    /// we've allowed this because we intend that objects should only call this
    /// method during the initialization phase, before any items are added and
    /// before any reading is performed. Once data is being processed, this
    /// method will never again be called. We didn't add any special code to
    /// enforce this, we just expect this method to be used "nicely".
    /// </summary>
    public void IncreaseMaxLookBack(int maxLookBack)
    {
      lock (_items)
      {
        if (maxLookBack > MaxLookBack)
        {
          MaxLookBack = maxLookBack;
        }
      }
    }

    public ImmutableLimitedSeries<T> Add(T item)
    {
      if (_items.Count == MaxLookBack)
      {
        return new ImmutableLimitedSeries<T>(_items.RemoveAt(0).Add(item), NumItemsRemoved + 1, MaxLookBack);
      }
      else
      {
        return new ImmutableLimitedSeries<T>(_items.Add(item), NumItemsRemoved, MaxLookBack);
      }
    }

    public ImmutableLimitedSeries<T> AddRange(IEnumerable<T> items)
    {
      var count = items switch
      {
        Array array => array.Length,
        IList<T> list => list.Count,
        _ => items.Count(),
      };
      var itemsToRemove = _items.Count + count - MaxLookBack;
      if (itemsToRemove <= 0)
      {
        return new ImmutableLimitedSeries<T>(_items.AddRange(items), NumItemsRemoved, MaxLookBack);
      }
      else
      {
        return new ImmutableLimitedSeries<T>(_items.RemoveRange(0, itemsToRemove).AddRange(items), NumItemsRemoved + itemsToRemove, MaxLookBack);
      }
    }

    /// <summary>
    /// Gets a thread-safe enumerable for all the items that have not been
    /// removed.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<T> GetRemainingItems() => _items;

    public IEnumerable<T> GetRange(int from, int to)
    {
      for (var i = from; i <= to; i++)
        yield return this[i];
    }
  }
}
