// Decompiled with JetBrains decompiler
// Type: GonzoNet.Concurrency.ConcurrentQueue`1
// Assembly: GonzoNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 75AA73F1-2E7B-40B2-B711-B42047463A5A
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GonzoNet.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

#nullable disable
namespace GonzoNet.Concurrency;

public class ConcurrentQueue<T> : 
  IProducerConsumerCollection<T>,
  IEnumerable<T>,
  ICollection,
  IEnumerable,
  ISerializable,
  IDeserializationCallback
{
  private ConcurrentQueue<T>.Node head = new ConcurrentQueue<T>.Node();
  private ConcurrentQueue<T>.Node tail;
  private int count;
  private object syncRoot = new object();

  public ConcurrentQueue() => this.tail = this.head;

  public ConcurrentQueue(IEnumerable<T> enumerable)
    : this()
  {
    foreach (T obj in enumerable)
      this.Enqueue(obj);
  }

  public void Enqueue(T item)
  {
    Interlocked.Increment(ref this.count);
    ConcurrentQueue<T>.Node node = new ConcurrentQueue<T>.Node();
    node.Value = item;
    ConcurrentQueue<T>.Node comparand = (ConcurrentQueue<T>.Node) null;
    bool flag = false;
    while (!flag)
    {
      comparand = this.tail;
      ConcurrentQueue<T>.Node next = comparand.Next;
      if (this.tail == comparand)
      {
        if (next == null)
          flag = Interlocked.CompareExchange<ConcurrentQueue<T>.Node>(ref this.tail.Next, node, (ConcurrentQueue<T>.Node) null) == null;
        else
          Interlocked.CompareExchange<ConcurrentQueue<T>.Node>(ref this.tail, next, comparand);
      }
    }
    Interlocked.CompareExchange<ConcurrentQueue<T>.Node>(ref this.tail, node, comparand);
  }

  bool IProducerConsumerCollection<T>.TryAdd(T item)
  {
    this.Enqueue(item);
    return true;
  }

  public bool TryDequeue(out T value)
  {
    value = default (T);
    bool flag = false;
    while (!flag)
    {
      ConcurrentQueue<T>.Node head = this.head;
      ConcurrentQueue<T>.Node tail = this.tail;
      ConcurrentQueue<T>.Node next = head.Next;
      if (head == this.head)
      {
        if (head == tail)
        {
          if (next != null)
            Interlocked.CompareExchange<ConcurrentQueue<T>.Node>(ref this.tail, next, tail);
          value = default (T);
          return false;
        }
        value = next.Value;
        flag = Interlocked.CompareExchange<ConcurrentQueue<T>.Node>(ref this.head, next, head) == head;
      }
    }
    Interlocked.Decrement(ref this.count);
    return true;
  }

  public bool TryPeek(out T value)
  {
    if (this.IsEmpty)
    {
      value = default (T);
      return false;
    }
    ConcurrentQueue<T>.Node next = this.head.Next;
    value = next.Value;
    return true;
  }

  public void Clear()
  {
    this.count = 0;
    this.tail = this.head = new ConcurrentQueue<T>.Node();
  }

  IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this.InternalGetEnumerator();

  IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.InternalGetEnumerator();

  public IEnumerator<T> GetEnumerator() => this.InternalGetEnumerator();

  private IEnumerator<T> InternalGetEnumerator()
  {
    ConcurrentQueue<T>.Node my_head = this.head;
    while ((my_head = my_head.Next) != null)
      yield return my_head.Value;
  }

  void ICollection.CopyTo(Array array, int index)
  {
    if (!(array is T[] dest))
      return;
    this.CopyTo(dest, index);
  }

  public void CopyTo(T[] dest, int index)
  {
    IEnumerator<T> enumerator = this.InternalGetEnumerator();
    int num = index;
    while (enumerator.MoveNext())
      dest[num++] = enumerator.Current;
  }

  public T[] ToArray()
  {
    T[] dest = new T[this.count];
    this.CopyTo(dest, 0);
    return dest;
  }

  public void GetObjectData(SerializationInfo info, StreamingContext context)
  {
    throw new NotImplementedException();
  }

  bool ICollection.IsSynchronized => true;

  public void OnDeserialization(object sender) => throw new NotImplementedException();

  bool IProducerConsumerCollection<T>.TryTake(out T item) => this.TryDequeue(out item);

  object ICollection.SyncRoot => this.syncRoot;

  public int Count => this.count;

  public bool IsEmpty => this.count == 0;

  private class Node
  {
    public T Value;
    public ConcurrentQueue<T>.Node Next;
  }
}
