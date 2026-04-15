// Decompiled with JetBrains decompiler
// Type: GonzoNet.Concurrency.IProducerConsumerCollection`1
// Assembly: GonzoNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 75AA73F1-2E7B-40B2-B711-B42047463A5A
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GonzoNet.dll

using System.Collections;
using System.Collections.Generic;

#nullable disable
namespace GonzoNet.Concurrency;

public interface IProducerConsumerCollection<T> : IEnumerable<T>, ICollection, IEnumerable
{
  bool TryAdd(T item);

  bool TryTake(out T item);

  T[] ToArray();
}
