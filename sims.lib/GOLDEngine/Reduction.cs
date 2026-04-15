// Decompiled with JetBrains decompiler
// Type: GOLDEngine.Reduction
// Assembly: GOLDEngine, Version=5.0.6070.308, Culture=neutral, PublicKeyToken=null
// MVID: 9B853C3A-54DD-4545-BD68-DA26645707EB
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GOLDEngine.dll

#nullable disable
namespace GOLDEngine;

public class Reduction : Token
{
  private Production m_production;
  private Token[] m_tokens;
  private object m_Tag;

  public Reduction(Production production, Token[] tokens)
    : base(production.Head(), new GOLDEngine.Position?(), false)
  {
    this.m_tokens = tokens;
    this.m_production = production;
  }

  public int Count => this.m_tokens.Length;

  public Token this[int index] => this.m_tokens[index];

  public Token[] Tokens => this.m_tokens;

  public SymbolList Symbols => this.m_production.Handle();

  public Production Production => this.m_production;

  public object Tag
  {
    get => this.m_Tag;
    set => this.m_Tag = value;
  }

  public override string ToString() => this.m_production.ToString();

  public override void Visit(ITokenVisitor visitor) => visitor.OnReduction(this);
}
