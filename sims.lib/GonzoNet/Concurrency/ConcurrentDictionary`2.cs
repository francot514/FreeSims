// Decompiled with JetBrains decompiler
// Type: GonzoNet.Concurrency.ConcurrentDictionary`2
// Assembly: GonzoNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 75AA73F1-2E7B-40B2-B711-B42047463A5A
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GonzoNet.dll

using System;
using System.Collections;
using System.Collections.Generic;

#nullable disable
namespace GonzoNet.Concurrency;

public class ConcurrentDictionary<TKey, TValue> : 
  IDictionary<TKey, TValue>,
  ICollection<KeyValuePair<TKey, TValue>>,
  IEnumerable<KeyValuePair<TKey, TValue>>,
  IDictionary,
  ICollection,
  IEnumerable
{
  private IEqualityComparer<TKey> comparer;
  private SplitOrderedList<KeyValuePair<TKey, TValue>> internalDictionary = new SplitOrderedList<KeyValuePair<TKey, TValue>>();

  public ConcurrentDictionary()
    : this((IEqualityComparer<TKey>) EqualityComparer<TKey>.Default)
  {
  }

  public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> values)
    : this(values, (IEqualityComparer<TKey>) EqualityComparer<TKey>.Default)
  {
    foreach (KeyValuePair<TKey, TValue> keyValuePair in values)
      this.Add(keyValuePair.Key, keyValuePair.Value);
  }

  public ConcurrentDictionary(IEqualityComparer<TKey> comparer) => this.comparer = comparer;

  public ConcurrentDictionary(
    IEnumerable<KeyValuePair<TKey, TValue>> values,
    IEqualityComparer<TKey> comparer)
    : this(comparer)
  {
    foreach (KeyValuePair<TKey, TValue> keyValuePair in values)
      this.Add(keyValuePair.Key, keyValuePair.Value);
  }

  public ConcurrentDictionary(int concurrencyLevel, int capacity)
    : this((IEqualityComparer<TKey>) EqualityComparer<TKey>.Default)
  {
  }

  public ConcurrentDictionary(
    int concurrencyLevel,
    IEnumerable<KeyValuePair<TKey, TValue>> values,
    IEqualityComparer<TKey> comparer)
    : this(values, comparer)
  {
  }

  public ConcurrentDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer)
    : this(comparer)
  {
  }

  private void Add(TKey key, TValue value)
  {
    do
      ;
    while (!this.TryAdd(key, value));
  }

  void IDictionary<TKey, TValue>.Add(TKey key, TValue value) => this.Add(key, value);

  public bool TryAdd(TKey key, TValue value)
  {
    return this.internalDictionary.Insert(this.Hash(key), ConcurrentDictionary<TKey, TValue>.Make<TKey, TValue>(key, value));
  }

  void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> pair)
  {
    this.Add(pair.Key, pair.Value);
  }

  public TValue AddOrUpdate(
    TKey key,
    Func<TKey, TValue> addValueFactory,
    Func<TKey, TValue, TValue> updateValueFactory)
  {
    return this.internalDictionary.InsertOrUpdate(this.Hash(key), (Func<KeyValuePair<TKey, TValue>>) (() => ConcurrentDictionary<TKey, TValue>.Make<TKey, TValue>(key, addValueFactory(key))), (Func<KeyValuePair<TKey, TValue>, KeyValuePair<TKey, TValue>>) (e => ConcurrentDictionary<TKey, TValue>.Make<TKey, TValue>(key, updateValueFactory(key, e.Value)))).Value;
  }

  public TValue AddOrUpdate(
    TKey key,
    TValue addValue,
    Func<TKey, TValue, TValue> updateValueFactory)
  {
    return this.AddOrUpdate(key, (Func<TKey, TValue>) (_ => addValue), updateValueFactory);
  }

  private TValue AddOrUpdate(TKey key, TValue addValue, TValue updateValue)
  {
    return this.internalDictionary.InsertOrUpdate(this.Hash(key), ConcurrentDictionary<TKey, TValue>.Make<TKey, TValue>(key, addValue), ConcurrentDictionary<TKey, TValue>.Make<TKey, TValue>(key, updateValue)).Value;
  }

  private TValue GetValue(TKey key)
  {
    TValue obj;
    if (!this.TryGetValue(key, out obj))
      throw new ArgumentException("Not a valid key for this dictionary", nameof (key));
    return obj;
  }

  public bool TryGetValue(TKey key, out TValue value)
  {
    KeyValuePair<TKey, TValue> data;
    bool flag = this.internalDictionary.Find(this.Hash(key), out data);
    value = data.Value;
    return flag;
  }

  public bool TryUpdate(TKey key, TValue newValue, TValue comparand)
  {
    return this.internalDictionary.CompareExchange(this.Hash(key), ConcurrentDictionary<TKey, TValue>.Make<TKey, TValue>(key, newValue), (Func<KeyValuePair<TKey, TValue>, bool>) (e => e.Value.Equals((object) comparand)));
  }

  public TValue this[TKey key]
  {
    get => this.GetValue(key);
    set => this.AddOrUpdate(key, value, value);
  }

  public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
  {
    return this.internalDictionary.InsertOrGet(this.Hash(key), ConcurrentDictionary<TKey, TValue>.Make<TKey, TValue>(key, default (TValue)), (Func<KeyValuePair<TKey, TValue>>) (() => ConcurrentDictionary<TKey, TValue>.Make<TKey, TValue>(key, valueFactory(key)))).Value;
  }

  public TValue GetOrAdd(TKey key, TValue value)
  {
    return this.internalDictionary.InsertOrGet(this.Hash(key), ConcurrentDictionary<TKey, TValue>.Make<TKey, TValue>(key, value), (Func<KeyValuePair<TKey, TValue>>) null).Value;
  }

  public bool TryRemove(TKey key, out TValue value)
  {
    KeyValuePair<TKey, TValue> data;
    bool flag = this.internalDictionary.Delete(this.Hash(key), out data);
    value = data.Value;
    return flag;
  }

  private bool Remove(TKey key) => this.TryRemove(key, out TValue _);

  bool IDictionary<TKey, TValue>.Remove(TKey key) => this.Remove(key);

  bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> pair)
  {
    return this.Remove(pair.Key);
  }

  public bool ContainsKey(TKey key)
  {
    return this.internalDictionary.Find(this.Hash(key), out KeyValuePair<TKey, TValue> _);
  }

  bool IDictionary.Contains(object key) => key is TKey key1 && this.ContainsKey(key1);

  void IDictionary.Remove(object key)
  {
    if (!(key is TKey key1))
      return;
    this.Remove(key1);
  }

  object IDictionary.this[object key]
  {
    get
    {
      return key is TKey key1 ? (object) this[key1] : throw new ArgumentException("key isn't of correct type", nameof (key));
    }
    set
    {
      this[(TKey) key] = key is TKey && value is TValue ? (TValue) value : throw new ArgumentException("key or value aren't of correct type");
    }
  }

  void IDictionary.Add(object key, object value)
  {
    if (!(key is TKey) || !(value is TValue))
      throw new ArgumentException("key or value aren't of correct type");
    this.Add((TKey) key, (TValue) value);
  }

  bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> pair)
  {
    return this.ContainsKey(pair.Key);
  }

  public KeyValuePair<TKey, TValue>[] ToArray()
  {
    return new List<KeyValuePair<TKey, TValue>>((IEnumerable<KeyValuePair<TKey, TValue>>) this).ToArray();
  }

  public void Clear()
  {
    this.internalDictionary = new SplitOrderedList<KeyValuePair<TKey, TValue>>();
  }

  public int Count => this.internalDictionary.Count;

  public bool IsEmpty => this.Count == 0;

  bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

  bool IDictionary.IsReadOnly => false;

  public ICollection<TKey> Keys
  {
    get => this.GetPart<TKey>((Func<KeyValuePair<TKey, TValue>, TKey>) (kvp => kvp.Key));
  }

  public ICollection<TValue> Values
  {
    get => this.GetPart<TValue>((Func<KeyValuePair<TKey, TValue>, TValue>) (kvp => kvp.Value));
  }

  ICollection IDictionary.Keys => (ICollection) this.Keys;

  ICollection IDictionary.Values => (ICollection) this.Values;

  private ICollection<T> GetPart<T>(Func<KeyValuePair<TKey, TValue>, T> extractor)
  {
    List<T> objList = new List<T>();
    foreach (KeyValuePair<TKey, TValue> keyValuePair in this)
      objList.Add(extractor(keyValuePair));
    return (ICollection<T>) objList.AsReadOnly();
  }

  void ICollection.CopyTo(Array array, int startIndex)
  {
    if (!(array is KeyValuePair<TKey, TValue>[] array1))
      return;
    this.CopyTo(array1, startIndex, this.Count);
  }

  private void CopyTo(KeyValuePair<TKey, TValue>[] array, int startIndex)
  {
    this.CopyTo(array, startIndex, this.Count);
  }

  void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(
    KeyValuePair<TKey, TValue>[] array,
    int startIndex)
  {
    this.CopyTo(array, startIndex);
  }

  private void CopyTo(KeyValuePair<TKey, TValue>[] array, int startIndex, int num)
  {
    foreach (KeyValuePair<TKey, TValue> keyValuePair in this)
    {
      array[startIndex++] = keyValuePair;
      if (--num <= 0)
        break;
    }
  }

  public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => this.GetEnumeratorInternal();

  IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this.GetEnumeratorInternal();

  private IEnumerator<KeyValuePair<TKey, TValue>> GetEnumeratorInternal()
  {
    return this.internalDictionary.GetEnumerator();
  }

  IDictionaryEnumerator IDictionary.GetEnumerator()
  {
    return (IDictionaryEnumerator) new ConcurrentDictionary<TKey, TValue>.ConcurrentDictionaryEnumerator(this.GetEnumeratorInternal());
  }

  object ICollection.SyncRoot => (object) this;

  bool IDictionary.IsFixedSize => false;

  bool ICollection.IsSynchronized => true;

  private static KeyValuePair<U, V> Make<U, V>(U key, V value)
  {
    return new KeyValuePair<U, V>(key, value);
  }

  private uint Hash(TKey key) => (uint) this.comparer.GetHashCode(key);

  private class ConcurrentDictionaryEnumerator : IDictionaryEnumerator, IEnumerator
  {
    private IEnumerator<KeyValuePair<TKey, TValue>> internalEnum;

    public ConcurrentDictionaryEnumerator(
      IEnumerator<KeyValuePair<TKey, TValue>> internalEnum)
    {
      this.internalEnum = internalEnum;
    }

    public bool MoveNext() => this.internalEnum.MoveNext();

    public void Reset() => this.internalEnum.Reset();

    public object Current => (object) this.Entry;

    public DictionaryEntry Entry
    {
      get
      {
        KeyValuePair<TKey, TValue> current = this.internalEnum.Current;
        return new DictionaryEntry((object) current.Key, (object) current.Value);
      }
    }

    public object Key => (object) this.internalEnum.Current.Key;

    public object Value => (object) this.internalEnum.Current.Value;
  }
}
