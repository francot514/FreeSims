// Decompiled with JetBrains decompiler
// Type: GOLDEngine.SymbolList
// Assembly: GOLDEngine, Version=5.0.6070.308, Culture=neutral, PublicKeyToken=null
// MVID: 9B853C3A-54DD-4545-BD68-DA26645707EB
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GOLDEngine.dll

using System;
using System.Collections.Generic;
using System.ComponentModel;

#nullable disable
namespace GOLDEngine;

public class SymbolList
{
  private List<Symbol> m_Array;

  internal SymbolList(List<Symbol> symbols) => this.m_Array = symbols;

  internal SymbolList(int Size)
  {
    this.m_Array = new List<Symbol>(Size);
    for (int index = 0; index <= Size - 1; ++index)
      this.m_Array.Add((Symbol) null);
  }

  public Symbol Find(Predicate<Symbol> match) => this.m_Array.Find(match);

  [Description("Returns the symbol with the specified index.")]
  public Symbol this[int Index]
  {
    get => Index >= 0 & Index < this.m_Array.Count ? this.m_Array[Index] : (Symbol) null;
    internal set => this.m_Array[Index] = value;
  }

  [Description("Returns the total number of symbols in the list.")]
  public int Count => this.m_Array.Count;

  internal Symbol GetFirstOfType(SymbolType Type)
  {
    return this.m_Array.Find((Predicate<Symbol>) (symbol => symbol.Type == Type));
  }

  public override string ToString() => this.Text();

  [Description("Returns a list of the symbol names in BNF format.")]
  public string Text(string Separator, bool AlwaysDelimitTerminals)
  {
    string str = "";
    for (int index = 0; index <= this.m_Array.Count - 1; ++index)
    {
      Symbol symbol = this.m_Array[index];
      str = str + (index == 0 ? "" : Separator) + symbol.Text(AlwaysDelimitTerminals);
    }
    return str;
  }

  [Description("Returns a list of the symbol names in BNF format.")]
  public string Text() => this.Text(", ", false);
}
