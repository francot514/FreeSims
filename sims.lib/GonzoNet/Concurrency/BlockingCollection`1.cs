// Decompiled with JetBrains decompiler
// Type: GonzoNet.Concurrency.BlockingCollection`1
// Assembly: GonzoNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 75AA73F1-2E7B-40B2-B711-B42047463A5A
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GonzoNet.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

#nullable disable
namespace GonzoNet.Concurrency;

public class BlockingCollection<T> : IEnumerable<T>, ICollection, IEnumerable, IDisposable
{
  private readonly IProducerConsumerCollection<T> underlyingColl;
  private readonly int upperBound;
  private readonly Func<bool> isFull;
  private readonly SpinWait sw = new SpinWait();
  private AtomicBoolean isComplete;

  public BlockingCollection()
    : this((IProducerConsumerCollection<T>) new ConcurrentQueue<T>(), -1)
  {
  }

  public BlockingCollection(int upperBound)
    : this((IProducerConsumerCollection<T>) new ConcurrentQueue<T>(), upperBound)
  {
  }

  public BlockingCollection(IProducerConsumerCollection<T> underlyingColl)
    : this(underlyingColl, -1)
  {
  }

  public BlockingCollection(IProducerConsumerCollection<T> underlyingColl, int upperBound)
  {
    this.underlyingColl = underlyingColl;
    this.upperBound = upperBound;
    this.isComplete = new AtomicBoolean();
    if (upperBound == -1)
      this.isFull = new Func<bool>(BlockingCollection<T>.FalseIsFull);
    else
      this.isFull = new Func<bool>(this.CountBasedIsFull);
  }

  private static bool FalseIsFull() => false;

  private bool CountBasedIsFull() => this.underlyingColl.Count >= this.upperBound;

  public void Add(T item)
  {
    do
    {
      while (this.isFull())
      {
        if (this.isComplete.Value)
          throw new InvalidOperationException("The BlockingCollection<T> has been marked as complete with regards to additions.");
        this.Block();
      }
      if (this.isComplete.Value)
        goto label_6;
    }
    while (this.isFull() || !this.underlyingColl.TryAdd(item));
    goto label_8;
label_6:
    throw new InvalidOperationException("The BlockingCollection<T> has been marked as complete with regards to additions.");
label_8:;
  }

  public T Remove()
  {
    T obj;
    while (this.underlyingColl.Count == 0 || !this.underlyingColl.TryTake(out obj))
    {
      if (this.isComplete.Value)
        throw new OperationCanceledException("The BlockingCollection<T> is empty and has been marked as complete with regards to additions.");
      this.Block();
    }
    return obj;
  }

  public bool TryAdd(T item)
  {
    return !this.isComplete.Value && !this.isFull() && this.underlyingColl.TryAdd(item);
  }

  public bool TryAdd(T item, TimeSpan ts) => this.TryAdd(item, (int) ts.TotalMilliseconds);

  public bool TryAdd(T item, int millisecondsTimeout)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    while (this.isFull())
    {
      if (this.isComplete.Value || stopwatch.ElapsedMilliseconds > (long) millisecondsTimeout)
      {
        stopwatch.Stop();
        return false;
      }
      this.Block();
    }
    return this.TryAdd(item);
  }

  public bool TryRemove(out T item) => this.underlyingColl.TryTake(out item);

  public bool TryRemove(out T item, TimeSpan ts)
  {
    return this.TryRemove(out item, (int) ts.TotalMilliseconds);
  }

  public bool TryRemove(out T item, int millisecondsTimeout)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    while (this.underlyingColl.Count == 0)
    {
      if (this.isComplete.Value || stopwatch.ElapsedMilliseconds > (long) millisecondsTimeout)
      {
        item = default (T);
        return false;
      }
      this.Block();
    }
    return this.TryRemove(out item);
  }

  private static void CheckArray(BlockingCollection<T>[] collections)
  {
    if (collections == null)
      throw new ArgumentNullException(nameof (collections));
    if (collections.Length == 0 || BlockingCollection<T>.IsThereANullElement(collections))
      throw new ArgumentException("The collections argument is a 0-length array or contains a null element.", nameof (collections));
  }

  private static bool IsThereANullElement(BlockingCollection<T>[] collections)
  {
    foreach (BlockingCollection<T> collection in collections)
    {
      if (collection == null)
        return true;
    }
    return false;
  }

  public static int AddAny(BlockingCollection<T>[] collections, T item)
  {
    BlockingCollection<T>.CheckArray(collections);
    int num = 0;
    foreach (BlockingCollection<T> collection in collections)
    {
      try
      {
        collection.Add(item);
        return num;
      }
      catch
      {
      }
      ++num;
    }
    return -1;
  }

  public static int TryAddAny(BlockingCollection<T>[] collections, T item)
  {
    BlockingCollection<T>.CheckArray(collections);
    int num = 0;
    foreach (BlockingCollection<T> collection in collections)
    {
      if (collection.TryAdd(item))
        return num;
      ++num;
    }
    return -1;
  }

  public static int TryAddAny(BlockingCollection<T>[] collections, T item, TimeSpan ts)
  {
    BlockingCollection<T>.CheckArray(collections);
    int num = 0;
    foreach (BlockingCollection<T> collection in collections)
    {
      if (collection.TryAdd(item, ts))
        return num;
      ++num;
    }
    return -1;
  }

  public static int TryAddAny(BlockingCollection<T>[] collections, T item, int millisecondsTimeout)
  {
    BlockingCollection<T>.CheckArray(collections);
    int num = 0;
    foreach (BlockingCollection<T> collection in collections)
    {
      if (collection.TryAdd(item, millisecondsTimeout))
        return num;
      ++num;
    }
    return -1;
  }

  public static int RemoveAny(BlockingCollection<T>[] collections, out T item)
  {
    item = default (T);
    BlockingCollection<T>.CheckArray(collections);
    int num = 0;
    foreach (BlockingCollection<T> collection in collections)
    {
      try
      {
        item = collection.Remove();
        return num;
      }
      catch
      {
      }
      ++num;
    }
    return -1;
  }

  public static int TryRemoveAny(BlockingCollection<T>[] collections, out T item)
  {
    item = default (T);
    BlockingCollection<T>.CheckArray(collections);
    int num = 0;
    foreach (BlockingCollection<T> collection in collections)
    {
      if (collection.TryRemove(out item))
        return num;
      ++num;
    }
    return -1;
  }

  public static int TryRemoveAny(BlockingCollection<T>[] collections, out T item, TimeSpan ts)
  {
    item = default (T);
    BlockingCollection<T>.CheckArray(collections);
    int num = 0;
    foreach (BlockingCollection<T> collection in collections)
    {
      if (collection.TryRemove(out item, ts))
        return num;
      ++num;
    }
    return -1;
  }

  public static int TryRemoveAny(
    BlockingCollection<T>[] collections,
    out T item,
    int millisecondsTimeout)
  {
    item = default (T);
    BlockingCollection<T>.CheckArray(collections);
    int num = 0;
    foreach (BlockingCollection<T> collection in collections)
    {
      if (collection.TryRemove(out item, millisecondsTimeout))
        return num;
      ++num;
    }
    return -1;
  }

  public void CompleteAdding() => this.isComplete.Value = true;

  void ICollection.CopyTo(Array array, int index) => this.underlyingColl.CopyTo(array, index);

  public void CopyTo(T[] array, int index) => this.underlyingColl.CopyTo((Array) array, index);

  public IEnumerable<T> GetConsumingEnumerable()
  {
    T item;
    while (this.underlyingColl.TryTake(out item))
      yield return item;
  }

  IEnumerator IEnumerable.GetEnumerator() => this.underlyingColl.GetEnumerator();

  IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.underlyingColl.GetEnumerator();

  public IEnumerator<T> GetEnumerator() => this.underlyingColl.GetEnumerator();

  public void Dispose()
  {
  }

  public T[] ToArray() => this.underlyingColl.ToArray();

  private void Block() => this.sw.SpinOnce();

  public int BoundedCapacity => this.upperBound;

  public int Count => this.underlyingColl.Count;

  public bool IsAddingCompleted => this.isComplete.Value;

  public bool IsCompleted => this.isComplete.Value && this.underlyingColl.Count == 0;

  object ICollection.SyncRoot => this.underlyingColl.SyncRoot;

  bool ICollection.IsSynchronized => this.underlyingColl.IsSynchronized;
}
