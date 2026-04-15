// Decompiled with JetBrains decompiler
// Type: GOLDEngine.TokenParser
// Assembly: GOLDEngine, Version=5.0.6070.308, Culture=neutral, PublicKeyToken=null
// MVID: 9B853C3A-54DD-4545-BD68-DA26645707EB
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GOLDEngine.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace GOLDEngine;

public class TokenParser : ITokenVisitor
{
  private Predicate<Terminal> m_isNoise;
  private Predicate<Reduction> m_isSynthetic;
  private Dictionary<string, Action<Reduction>> m_reductionHandler = new Dictionary<string, Action<Reduction>>();

  protected Predicate<Terminal> ExpectTerminal { get; set; }

  protected TokenParser(Predicate<Terminal> isNoise, Predicate<Reduction> isSynthetic)
  {
    this.m_isNoise = isNoise;
    this.m_isSynthetic = isSynthetic;
  }

  protected static bool ExpectNoTerminals(Terminal terminal) => false;

  protected void AddHandler(string symbolName, Action<Reduction> action)
  {
    this.m_reductionHandler.Add(symbolName, action);
  }

  protected void OnCreate(Predicate<Terminal> expectTerminal, Reduction reduction)
  {
    this.ExpectTerminal = expectTerminal;
    this.VisitTokens(reduction);
  }

  public void OnTerminal(Terminal terminal)
  {
    if (this.m_isNoise(terminal))
      return;
    TokenParser.Assert(this.ExpectTerminal(terminal));
  }

  public void OnReduction(Reduction reduction)
  {
    if (this.m_isSynthetic(reduction))
    {
      this.VisitTokens(reduction);
    }
    else
    {
      TokenParser.Assert(this.m_reductionHandler.ContainsKey(reduction.SymbolName));
      this.m_reductionHandler[reduction.SymbolName](reduction);
    }
  }

  protected Action<Reduction> this[string symbolName] => this.m_reductionHandler[symbolName];

  private void VisitTokens(Reduction reduction)
  {
    foreach (Token token in reduction.Tokens)
      token.Visit((ITokenVisitor) this);
  }

  protected void RecursiveList(Reduction reduction) => this.VisitTokens(reduction);

  protected Terminal ExpectOneTerminal(Reduction reduction)
  {
    Terminal found = (Terminal) null;
    new TokenParser(this.m_isNoise, this.m_isSynthetic).OnCreate((Predicate<Terminal>) (terminal =>
    {
      if (found != null)
        return false;
      found = terminal;
      return true;
    }), reduction);
    TokenParser.Assert(found != null);
    return found;
  }

  protected List<Terminal> ExpectTerminalList(Reduction reduction)
  {
    List<Terminal> found = new List<Terminal>();
    new TokenParser(this.m_isNoise, this.m_isSynthetic).OnCreate((Predicate<Terminal>) (terminal =>
    {
      found.Add(terminal);
      return true;
    }), reduction);
    TokenParser.Assert(found.Count > 0);
    return found;
  }

  protected static void Assert(bool b)
  {
    if (!b)
      throw new Exception();
  }
}
