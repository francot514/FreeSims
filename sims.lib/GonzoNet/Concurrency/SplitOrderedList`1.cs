// Decompiled with JetBrains decompiler
// Type: GonzoNet.Concurrency.SplitOrderedList`1
// Assembly: GonzoNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 75AA73F1-2E7B-40B2-B711-B42047463A5A
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GonzoNet.dll

using System;
using System.Collections.Generic;
using System.Threading;

#nullable disable
namespace GonzoNet.Concurrency;

internal class SplitOrderedList<T>
{
  private const int MaxLoad = 5;
  private const uint BucketSize = 512 /*0x0200*/;
  private SplitOrderedList<T>.Node head;
  private SplitOrderedList<T>.Node tail;
  private SplitOrderedList<T>.Node[] buckets = new SplitOrderedList<T>.Node[new IntPtr(512)];
  private int count;
  private int size = 2;
  private SplitOrderedList<T>.SimpleRwLock slim = new SplitOrderedList<T>.SimpleRwLock();
  private static readonly byte[] reverseTable = new byte[256 /*0x0100*/]
  {
    (byte) 0,
    (byte) 128 /*0x80*/,
    (byte) 64 /*0x40*/,
    (byte) 192 /*0xC0*/,
    (byte) 32 /*0x20*/,
    (byte) 160 /*0xA0*/,
    (byte) 96 /*0x60*/,
    (byte) 224 /*0xE0*/,
    (byte) 16 /*0x10*/,
    (byte) 144 /*0x90*/,
    (byte) 80 /*0x50*/,
    (byte) 208 /*0xD0*/,
    (byte) 48 /*0x30*/,
    (byte) 176 /*0xB0*/,
    (byte) 112 /*0x70*/,
    (byte) 240 /*0xF0*/,
    (byte) 8,
    (byte) 136,
    (byte) 72,
    (byte) 200,
    (byte) 40,
    (byte) 168,
    (byte) 104,
    (byte) 232,
    (byte) 24,
    (byte) 152,
    (byte) 88,
    (byte) 216,
    (byte) 56,
    (byte) 184,
    (byte) 120,
    (byte) 248,
    (byte) 4,
    (byte) 132,
    (byte) 68,
    (byte) 196,
    (byte) 36,
    (byte) 164,
    (byte) 100,
    (byte) 228,
    (byte) 20,
    (byte) 148,
    (byte) 84,
    (byte) 212,
    (byte) 52,
    (byte) 180,
    (byte) 116,
    (byte) 244,
    (byte) 12,
    (byte) 140,
    (byte) 76,
    (byte) 204,
    (byte) 44,
    (byte) 172,
    (byte) 108,
    (byte) 236,
    (byte) 28,
    (byte) 156,
    (byte) 92,
    (byte) 220,
    (byte) 60,
    (byte) 188,
    (byte) 124,
    (byte) 252,
    (byte) 2,
    (byte) 130,
    (byte) 66,
    (byte) 194,
    (byte) 34,
    (byte) 162,
    (byte) 98,
    (byte) 226,
    (byte) 18,
    (byte) 146,
    (byte) 82,
    (byte) 210,
    (byte) 50,
    (byte) 178,
    (byte) 114,
    (byte) 242,
    (byte) 10,
    (byte) 138,
    (byte) 74,
    (byte) 202,
    (byte) 42,
    (byte) 170,
    (byte) 106,
    (byte) 234,
    (byte) 26,
    (byte) 154,
    (byte) 90,
    (byte) 218,
    (byte) 58,
    (byte) 186,
    (byte) 122,
    (byte) 250,
    (byte) 6,
    (byte) 134,
    (byte) 70,
    (byte) 198,
    (byte) 38,
    (byte) 166,
    (byte) 102,
    (byte) 230,
    (byte) 22,
    (byte) 150,
    (byte) 86,
    (byte) 214,
    (byte) 54,
    (byte) 182,
    (byte) 118,
    (byte) 246,
    (byte) 14,
    (byte) 142,
    (byte) 78,
    (byte) 206,
    (byte) 46,
    (byte) 174,
    (byte) 110,
    (byte) 238,
    (byte) 30,
    (byte) 158,
    (byte) 94,
    (byte) 222,
    (byte) 62,
    (byte) 190,
    (byte) 126,
    (byte) 254,
    (byte) 1,
    (byte) 129,
    (byte) 65,
    (byte) 193,
    (byte) 33,
    (byte) 161,
    (byte) 97,
    (byte) 225,
    (byte) 17,
    (byte) 145,
    (byte) 81,
    (byte) 209,
    (byte) 49,
    (byte) 177,
    (byte) 113,
    (byte) 241,
    (byte) 9,
    (byte) 137,
    (byte) 73,
    (byte) 201,
    (byte) 41,
    (byte) 169,
    (byte) 105,
    (byte) 233,
    (byte) 25,
    (byte) 153,
    (byte) 89,
    (byte) 217,
    (byte) 57,
    (byte) 185,
    (byte) 121,
    (byte) 249,
    (byte) 5,
    (byte) 133,
    (byte) 69,
    (byte) 197,
    (byte) 37,
    (byte) 165,
    (byte) 101,
    (byte) 229,
    (byte) 21,
    (byte) 149,
    (byte) 85,
    (byte) 213,
    (byte) 53,
    (byte) 181,
    (byte) 117,
    (byte) 245,
    (byte) 13,
    (byte) 141,
    (byte) 77,
    (byte) 205,
    (byte) 45,
    (byte) 173,
    (byte) 109,
    (byte) 237,
    (byte) 29,
    (byte) 157,
    (byte) 93,
    (byte) 221,
    (byte) 61,
    (byte) 189,
    (byte) 125,
    (byte) 253,
    (byte) 3,
    (byte) 131,
    (byte) 67,
    (byte) 195,
    (byte) 35,
    (byte) 163,
    (byte) 99,
    (byte) 227,
    (byte) 19,
    (byte) 147,
    (byte) 83,
    (byte) 211,
    (byte) 51,
    (byte) 179,
    (byte) 115,
    (byte) 243,
    (byte) 11,
    (byte) 139,
    (byte) 75,
    (byte) 203,
    (byte) 43,
    (byte) 171,
    (byte) 107,
    (byte) 235,
    (byte) 27,
    (byte) 155,
    (byte) 91,
    (byte) 219,
    (byte) 59,
    (byte) 187,
    (byte) 123,
    (byte) 251,
    (byte) 7,
    (byte) 135,
    (byte) 71,
    (byte) 199,
    (byte) 39,
    (byte) 167,
    (byte) 103,
    (byte) 231,
    (byte) 23,
    (byte) 151,
    (byte) 87,
    (byte) 215,
    (byte) 55,
    (byte) 183,
    (byte) 119,
    (byte) 247,
    (byte) 15,
    (byte) 143,
    (byte) 79,
    (byte) 207,
    (byte) 47,
    (byte) 175,
    (byte) 111,
    (byte) 239,
    (byte) 31 /*0x1F*/,
    (byte) 159,
    (byte) 95,
    (byte) 223,
    (byte) 63 /*0x3F*/,
    (byte) 191,
    (byte) 127 /*0x7F*/,
    byte.MaxValue
  };
  private static readonly byte[] logTable = new byte[256 /*0x0100*/]
  {
    byte.MaxValue,
    (byte) 0,
    (byte) 1,
    (byte) 1,
    (byte) 2,
    (byte) 2,
    (byte) 2,
    (byte) 2,
    (byte) 3,
    (byte) 3,
    (byte) 3,
    (byte) 3,
    (byte) 3,
    (byte) 3,
    (byte) 3,
    (byte) 3,
    (byte) 4,
    (byte) 4,
    (byte) 4,
    (byte) 4,
    (byte) 4,
    (byte) 4,
    (byte) 4,
    (byte) 4,
    (byte) 4,
    (byte) 4,
    (byte) 4,
    (byte) 4,
    (byte) 4,
    (byte) 4,
    (byte) 4,
    (byte) 4,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 5,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 6,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7,
    (byte) 7
  };

  public SplitOrderedList()
  {
    this.head = new SplitOrderedList<T>.Node(0UL, default (T));
    this.tail = new SplitOrderedList<T>.Node(ulong.MaxValue, default (T));
    this.head.Next = this.tail;
    this.SetBucket(0U, this.head);
  }

  public int Count => this.count;

  public T InsertOrUpdate(uint key, Func<T> addGetter, Func<T, T> updateGetter)
  {
    SplitOrderedList<T>.Node current;
    return this.InsertInternal(key, default (T), addGetter, out current) ? current.Data : (current.Data = updateGetter(current.Data));
  }

  public T InsertOrUpdate(uint key, T addValue, T updateValue)
  {
    SplitOrderedList<T>.Node current;
    return this.InsertInternal(key, addValue, (Func<T>) null, out current) ? current.Data : (current.Data = updateValue);
  }

  public bool Insert(uint key, T data)
  {
    return this.InsertInternal(key, data, (Func<T>) null, out SplitOrderedList<T>.Node _);
  }

  public T InsertOrGet(uint key, T data, Func<T> dataCreator)
  {
    SplitOrderedList<T>.Node current;
    this.InsertInternal(key, data, dataCreator, out current);
    return current.Data;
  }

  private bool InsertInternal(
    uint key,
    T data,
    Func<T> dataCreator,
    out SplitOrderedList<T>.Node current)
  {
    SplitOrderedList<T>.Node newNode = new SplitOrderedList<T>.Node(SplitOrderedList<T>.ComputeRegularKey(key), data);
    uint num = key % (uint) this.size;
    SplitOrderedList<T>.Node startPoint;
    if ((startPoint = this.GetBucket(num)) == null)
      startPoint = this.InitializeBucket(num);
    if (!this.ListInsert(newNode, startPoint, out current, dataCreator))
      return false;
    int size = this.size;
    if (Interlocked.Increment(ref this.count) / size > 5 && (size & 1073741824 /*0x40000000*/) == 0)
      Interlocked.CompareExchange(ref this.size, 2 * size, size);
    current = newNode;
    return true;
  }

  public bool Find(uint key, out T data)
  {
    uint num = key % (uint) this.size;
    data = default (T);
    SplitOrderedList<T>.Node startPoint;
    if ((startPoint = this.GetBucket(num)) == null)
      startPoint = this.InitializeBucket(num);
    SplitOrderedList<T>.Node data1;
    if (!this.ListFind(SplitOrderedList<T>.ComputeRegularKey(key), startPoint, out data1))
      return false;
    data = data1.Data;
    return !data1.Marked;
  }

  public bool CompareExchange(uint key, T data, Func<T, bool> check)
  {
    uint num = key % (uint) this.size;
    SplitOrderedList<T>.Node startPoint;
    if ((startPoint = this.GetBucket(num)) == null)
      startPoint = this.InitializeBucket(num);
    SplitOrderedList<T>.Node data1;
    if (!this.ListFind(SplitOrderedList<T>.ComputeRegularKey(key), startPoint, out data1) || !check(data1.Data))
      return false;
    data1.Data = data;
    return true;
  }

  public bool Delete(uint key, out T data)
  {
    uint num = key % (uint) this.size;
    SplitOrderedList<T>.Node startPoint;
    if ((startPoint = this.GetBucket(num)) == null)
      startPoint = this.InitializeBucket(num);
    if (!this.ListDelete(startPoint, SplitOrderedList<T>.ComputeRegularKey(key), out data))
      return false;
    Interlocked.Decrement(ref this.count);
    return true;
  }

  public IEnumerator<T> GetEnumerator()
  {
    for (SplitOrderedList<T>.Node node = this.head.Next; node != this.tail; node = node.Next)
    {
      while (node.Marked || ((long) node.Key & 1L) == 0L)
      {
        node = node.Next;
        if (node == this.tail)
          yield break;
      }
      yield return node.Data;
    }
  }

  private SplitOrderedList<T>.Node InitializeBucket(uint b)
  {
    uint parent = SplitOrderedList<T>.GetParent(b);
    SplitOrderedList<T>.Node startPoint;
    if ((startPoint = this.GetBucket(parent)) == null)
      startPoint = this.InitializeBucket(parent);
    SplitOrderedList<T>.Node node = new SplitOrderedList<T>.Node(SplitOrderedList<T>.ComputeDummyKey(b), default (T));
    SplitOrderedList<T>.Node current;
    return !this.ListInsert(node, startPoint, out current, (Func<T>) null) ? current : this.SetBucket(b, node);
  }

  private static uint GetParent(uint v)
  {
    uint index1;
    uint index2;
    uint index3;
    int num = (index1 = v >> 16 /*0x10*/) > 0U ? ((index3 = index1 >> 8) > 0U ? 24 + (int) SplitOrderedList<T>.logTable[(IntPtr) index3] : 16 /*0x10*/ + (int) SplitOrderedList<T>.logTable[(IntPtr) index1]) : ((index2 = v >> 8) > 0U ? 8 + (int) SplitOrderedList<T>.logTable[(IntPtr) index2] : (int) SplitOrderedList<T>.logTable[(IntPtr) v]);
    return (uint) ((ulong) v & (ulong) ~(1 << num));
  }

  private static ulong ComputeRegularKey(uint key)
  {
    return SplitOrderedList<T>.ComputeDummyKey(key) | 1UL;
  }

  private static ulong ComputeDummyKey(uint key)
  {
    return (ulong) ((uint) ((int) SplitOrderedList<T>.reverseTable[(IntPtr) (key & (uint) byte.MaxValue)] << 24 | (int) SplitOrderedList<T>.reverseTable[(IntPtr) (key >> 8 & (uint) byte.MaxValue)] << 16 /*0x10*/ | (int) SplitOrderedList<T>.reverseTable[(IntPtr) (key >> 16 /*0x10*/ & (uint) byte.MaxValue)] << 8) | (uint) SplitOrderedList<T>.reverseTable[(IntPtr) (key >> 24 & (uint) byte.MaxValue)]) << 1;
  }

  private SplitOrderedList<T>.Node GetBucket(uint index)
  {
    return (long) index >= (long) this.buckets.Length ? (SplitOrderedList<T>.Node) null : this.buckets[(IntPtr) index];
  }

  private SplitOrderedList<T>.Node SetBucket(uint index, SplitOrderedList<T>.Node node)
  {
    try
    {
      this.slim.EnterReadLock();
      this.CheckSegment(index, true);
      Interlocked.CompareExchange<SplitOrderedList<T>.Node>(ref this.buckets[(IntPtr) index], node, (SplitOrderedList<T>.Node) null);
      return this.buckets[(IntPtr) index];
    }
    finally
    {
      this.slim.ExitReadLock();
    }
  }

  private void CheckSegment(uint segment, bool readLockTaken)
  {
    if ((long) segment < (long) this.buckets.Length)
      return;
    if (readLockTaken)
      this.slim.ExitReadLock();
    try
    {
      this.slim.EnterWriteLock();
      while ((long) segment >= (long) this.buckets.Length)
        Array.Resize<SplitOrderedList<T>.Node>(ref this.buckets, this.buckets.Length * 2);
    }
    finally
    {
      this.slim.ExitWriteLock();
    }
    if (!readLockTaken)
      return;
    this.slim.EnterReadLock();
  }

  private SplitOrderedList<T>.Node ListSearch(
    ulong key,
    ref SplitOrderedList<T>.Node left,
    SplitOrderedList<T>.Node h)
  {
    SplitOrderedList<T>.Node comparand = (SplitOrderedList<T>.Node) null;
    SplitOrderedList<T>.Node node1;
    while (true)
    {
      SplitOrderedList<T>.Node node2 = h;
      SplitOrderedList<T>.Node next = node2.Next;
      do
      {
        if (!next.Marked)
        {
          left = node2;
          comparand = next;
        }
        node2 = next.Marked ? next.Next : next;
        if (node2 != this.tail)
          next = node2.Next;
        else
          break;
      }
      while (next.Marked || node2.Key < key);
      node1 = node2;
      if (comparand == node1)
      {
        if (node1 == this.tail || !node1.Next.Marked)
          break;
      }
      else if (Interlocked.CompareExchange<SplitOrderedList<T>.Node>(ref left.Next, node1, comparand) == comparand && (node1 == this.tail || !node1.Next.Marked))
        goto label_10;
    }
    return node1;
label_10:
    return node1;
  }

  private bool ListDelete(SplitOrderedList<T>.Node startPoint, ulong key, out T data)
  {
    SplitOrderedList<T>.Node left = (SplitOrderedList<T>.Node) null;
    data = default (T);
    SplitOrderedList<T>.Node comparand;
    SplitOrderedList<T>.Node next;
    while (true)
    {
      comparand = this.ListSearch(key, ref left, startPoint);
      if (comparand != this.tail && (long) comparand.Key == (long) key)
      {
        data = comparand.Data;
        next = comparand.Next;
        if (!next.Marked && Interlocked.CompareExchange<SplitOrderedList<T>.Node>(ref comparand.Next, new SplitOrderedList<T>.Node(next), next) == next)
          goto label_5;
      }
      else
        break;
    }
    return false;
label_5:
    if (Interlocked.CompareExchange<SplitOrderedList<T>.Node>(ref left.Next, next, comparand) != next)
      this.ListSearch(comparand.Key, ref left, startPoint);
    return true;
  }

  private bool ListInsert(
    SplitOrderedList<T>.Node newNode,
    SplitOrderedList<T>.Node startPoint,
    out SplitOrderedList<T>.Node current,
    Func<T> dataCreator)
  {
    ulong key = newNode.Key;
    SplitOrderedList<T>.Node left = (SplitOrderedList<T>.Node) null;
    while (true)
    {
      SplitOrderedList<T>.Node comparand = current = this.ListSearch(key, ref left, startPoint);
      if (comparand == this.tail || (long) comparand.Key != (long) key)
      {
        newNode.Next = comparand;
        if (dataCreator != null)
          newNode.Data = dataCreator();
        if (Interlocked.CompareExchange<SplitOrderedList<T>.Node>(ref left.Next, newNode, comparand) == comparand)
          goto label_6;
      }
      else
        break;
    }
    return false;
label_6:
    return true;
  }

  private bool ListFind(
    ulong key,
    SplitOrderedList<T>.Node startPoint,
    out SplitOrderedList<T>.Node data)
  {
    SplitOrderedList<T>.Node left = (SplitOrderedList<T>.Node) null;
    data = (SplitOrderedList<T>.Node) null;
    SplitOrderedList<T>.Node node = this.ListSearch(key, ref left, startPoint);
    data = node;
    return node != this.tail && (long) node.Key == (long) key;
  }

  private class Node
  {
    public readonly bool Marked;
    public readonly ulong Key;
    public T Data;
    public SplitOrderedList<T>.Node Next;

    public Node(ulong key, T data)
    {
      this.Key = key;
      this.Data = data;
    }

    public Node(SplitOrderedList<T>.Node wrapped)
    {
      this.Marked = true;
      this.Next = wrapped;
    }
  }

  private struct SimpleRwLock
  {
    private const int RwWait = 1;
    private const int RwWrite = 2;
    private const int RwRead = 4;
    private int rwlock;

    public void EnterReadLock()
    {
      SpinWait spinWait = new SpinWait();
      while (true)
      {
        while ((this.rwlock & 3) > 0)
          spinWait.SpinOnce();
        if ((Interlocked.Add(ref this.rwlock, 4) & 1) != 0)
          Interlocked.Add(ref this.rwlock, -4);
        else
          break;
      }
    }

    public void ExitReadLock() => Interlocked.Add(ref this.rwlock, -4);

    public void EnterWriteLock()
    {
      SpinWait spinWait = new SpinWait();
      while (true)
      {
        int rwlock = this.rwlock;
        if (rwlock < 2)
        {
          if (Interlocked.CompareExchange(ref this.rwlock, 2, rwlock) != rwlock)
            rwlock = this.rwlock;
          else
            break;
        }
        while ((rwlock & 1) == 0 && Interlocked.CompareExchange(ref this.rwlock, rwlock | 1, rwlock) != rwlock)
          rwlock = this.rwlock;
        while (this.rwlock > 1)
          spinWait.SpinOnce();
      }
    }

    public void ExitWriteLock() => Interlocked.Add(ref this.rwlock, -2);
  }
}
