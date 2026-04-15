// Decompiled with JetBrains decompiler
// Type: GOLDEngine.Production
// Assembly: GOLDEngine, Version=5.0.6070.308, Culture=neutral, PublicKeyToken=null
// MVID: 9B853C3A-54DD-4545-BD68-DA26645707EB
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GOLDEngine.dll

using System.ComponentModel;

#nullable disable
namespace GOLDEngine;

public class Production
{
  private Symbol m_Head;
  private SymbolList m_Handle;
  private short m_TableIndex;

  internal Production(Symbol Head, short TableIndex, SymbolList Handle)
  {
    this.m_Head = Head;
    this.m_Handle = Handle;
    this.m_TableIndex = TableIndex;
  }

  [Description("Returns the head of the production.")]
  public Symbol Head() => this.m_Head;

  [Description("Returns the symbol list containing the handle (body) of the production.")]
  public SymbolList Handle() => this.m_Handle;

  [Description("Returns the index of the production in the Production Table.")]
  public short TableIndex() => this.m_TableIndex;

  public override string ToString() => this.Text();

  [Description("Returns the production in BNF.")]
  public string Text() => this.Text(false);

  public string Text(bool AlwaysDelimitTerminals)
  {
    return $"{this.m_Head.Text()} ::= {this.m_Handle.Text(" ", AlwaysDelimitTerminals)}";
  }

  internal bool ContainsOneNonTerminal()
  {
    bool flag = false;
    if (this.m_Handle.Count == 1 && this.m_Handle[0].Type == SymbolType.Nonterminal)
      flag = true;
    return flag;
  }
}
