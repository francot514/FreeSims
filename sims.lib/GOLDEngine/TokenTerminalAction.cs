// Decompiled with JetBrains decompiler
// Type: GOLDEngine.TokenTerminalAction
// Assembly: GOLDEngine, Version=5.0.6070.308, Culture=neutral, PublicKeyToken=null
// MVID: 9B853C3A-54DD-4545-BD68-DA26645707EB
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GOLDEngine.dll

using System;

#nullable disable
namespace GOLDEngine;

internal class TokenTerminalAction : ITokenVisitor
{
  private Action<Terminal> m_action;

  public TokenTerminalAction(Action<Terminal> action) => this.m_action = action;

  public void OnTerminal(Terminal terminal) => this.m_action(terminal);

  public void OnReduction(Reduction reduction)
  {
    foreach (Token token in reduction.Tokens)
      token.Visit((ITokenVisitor) this);
  }
}
