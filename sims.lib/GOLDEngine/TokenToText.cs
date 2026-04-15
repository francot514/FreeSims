// Decompiled with JetBrains decompiler
// Type: GOLDEngine.TokenToText
// Assembly: GOLDEngine, Version=5.0.6070.308, Culture=neutral, PublicKeyToken=null
// MVID: 9B853C3A-54DD-4545-BD68-DA26645707EB
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GOLDEngine.dll

using System.Text;

#nullable disable
namespace GOLDEngine;

internal class TokenToText : ITokenVisitor
{
  private StringBuilder m_stringBuilder = new StringBuilder();

  public void OnTerminal(Terminal terminal) => this.m_stringBuilder.Append(terminal.Text);

  public void OnReduction(Reduction reduction)
  {
    foreach (Token token in reduction.Tokens)
      token.Visit((ITokenVisitor) this);
  }

  public override string ToString() => this.m_stringBuilder.ToString();
}
