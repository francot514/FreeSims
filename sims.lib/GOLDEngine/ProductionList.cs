// Decompiled with JetBrains decompiler
// Type: GOLDEngine.ProductionList
// Assembly: GOLDEngine, Version=5.0.6070.308, Culture=neutral, PublicKeyToken=null
// MVID: 9B853C3A-54DD-4545-BD68-DA26645707EB
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GOLDEngine.dll

using System.Collections.Generic;
using System.ComponentModel;

#nullable disable
namespace GOLDEngine;

public class ProductionList
{
  private List<Production> m_Array;

  internal ProductionList(int Size)
  {
    this.m_Array = new List<Production>(Size);
    for (int index = 0; index <= Size - 1; ++index)
      this.m_Array.Add((Production) null);
  }

  [Description("Returns the production with the specified index.")]
  public Production this[int Index]
  {
    get => this.m_Array[Index];
    internal set => this.m_Array[Index] = value;
  }

  [Description("Returns the total number of productions in the list.")]
  public int Count() => this.m_Array.Count;

  internal void Add(Production Item) => this.m_Array.Add(Item);
}
