// Decompiled with JetBrains decompiler
// Type: GOLDEngine.Token
// Assembly: GOLDEngine, Version=5.0.6070.308, Culture=neutral, PublicKeyToken=null
// MVID: 9B853C3A-54DD-4545-BD68-DA26645707EB
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GOLDEngine.dll

using System;

#nullable disable
namespace GOLDEngine;

public abstract class Token
{
  private Symbol m_Parent;
  private GOLDEngine.Position? m_Position;

  protected Token(Symbol Parent, GOLDEngine.Position? position, bool isTerminal)
  {
    if (Parent.Type != 0 ^ isTerminal)
      throw new ParserException("Unexpected SymbolType");
    this.m_Parent = Parent;
    this.m_Position = position;
  }

  public Symbol Symbol => this.m_Parent;

  public string SymbolName => this.m_Parent.Name;

  public SymbolType SymbolType => this.m_Parent.Type;

  public GOLDEngine.Position? Position => this.m_Position;

  public Terminal AsTerminal => this as Terminal;

  public Reduction AsReduction => this as Reduction;

  public abstract void Visit(ITokenVisitor visitor);

  internal void TrimReduction(Symbol newSymbol) => this.m_Parent = newSymbol;

  public string ToText()
  {
    TokenToText visitor = new TokenToText();
    this.Visit((ITokenVisitor) visitor);
    return visitor.ToString();
  }

  public string ToJson()
  {
    TokenToJson visitor = new TokenToJson();
    this.Visit((ITokenVisitor) visitor);
    return visitor.ToString();
  }

  public void ForEachTerminal(Action<Terminal> action)
  {
    this.Visit((ITokenVisitor) new TokenTerminalAction(action));
  }
}
