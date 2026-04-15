// Decompiled with JetBrains decompiler
// Type: GOLDEngine.LALRStack
// Assembly: GOLDEngine, Version=5.0.6070.308, Culture=neutral, PublicKeyToken=null
// MVID: 9B853C3A-54DD-4545-BD68-DA26645707EB
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GOLDEngine.dll

using GOLDEngine.Tables;
using System.Collections.Generic;

#nullable disable
namespace GOLDEngine;

public class LALRStack
{
  private EGT m_loaded;
  internal bool m_TrimReductions;
  private short m_CurrentLALR;
  private Stack<LALRStack.TokenState> m_Stack = new Stack<LALRStack.TokenState>();

  internal LALRStack(EGT loaded, bool trimReductions)
  {
    this.m_loaded = loaded;
    this.m_TrimReductions = trimReductions;
    this.m_CurrentLALR = this.m_loaded.InitialLRState;
    this.m_Stack.Push(new LALRStack.TokenState((Token) null, this.m_loaded.InitialLRState, (short) -1));
  }

  public LALRStack.TokenState Peek() => this.m_Stack.Peek();

  public LALRStack.TokenState Pop()
  {
    LALRStack.TokenState tokenState = this.m_Stack.Pop();
    this.m_CurrentLALR = tokenState.PrevState;
    return tokenState;
  }

  public void PushExtra(Token token)
  {
    this.m_Stack.Push(new LALRStack.TokenState(token, this.Peek().GotoState, (short) -1));
  }

  public int Count => this.m_Stack.Count;

  internal Reduction CurrentReduction => this.m_Stack.Peek().Token as Reduction;

  private void Push(Token token, short gotoState)
  {
    this.m_Stack.Push(new LALRStack.TokenState(token, gotoState, this.m_CurrentLALR));
    this.m_CurrentLALR = gotoState;
  }

  internal LALRStack.ParseResult ParseLALR(Token NextToken)
  {
    LRAction lrAction1 = this.m_loaded.FindLRAction(this.m_CurrentLALR, NextToken.Symbol);
    if (lrAction1 == null)
      return LALRStack.ParseResult.SyntaxError;
    switch (lrAction1.Type)
    {
      case LRActionType.Shift:
        this.Push(NextToken, lrAction1.Value);
        return LALRStack.ParseResult.Shift;
      case LRActionType.Reduce:
        Production production = this.m_loaded.GetProduction(lrAction1);
        Token token;
        LALRStack.ParseResult lalr;
        if (this.m_TrimReductions & production.ContainsOneNonTerminal())
        {
          token = this.m_Stack.Pop().Token;
          token.TrimReduction(production.Head());
          lalr = LALRStack.ParseResult.ReduceEliminated;
        }
        else
        {
          List<Token> tokenList = new List<Token>(production.Handle().Count);
          for (int index = production.Handle().Count - 1; index >= 0; --index)
          {
            LALRStack.TokenState tokenState;
            for (tokenState = this.Pop(); tokenState.IsExtra; tokenState = this.Pop())
              tokenList.Insert(0, tokenState.Token);
            tokenList.Insert(0, tokenState.Token);
          }
          token = (Token) new Reduction(production, tokenList.ToArray());
          lalr = LALRStack.ParseResult.ReduceNormal;
        }
        LRAction lrAction2 = this.m_loaded.FindLRAction(this.m_Stack.Peek().GotoState, production.Head());
        if (lrAction2 == null || lrAction2.Type != LRActionType.Goto)
          return LALRStack.ParseResult.InternalError;
        this.Push(token, lrAction2.Value);
        return lalr;
      case LRActionType.Accept:
        return LALRStack.ParseResult.Accept;
      default:
        return LALRStack.ParseResult.InternalError;
    }
  }

  internal SymbolList GetExpectedSymbols() => this.m_loaded.GetExpectedSymbols(this.m_CurrentLALR);

  public enum ParseResult
  {
    Accept = 1,
    Shift = 2,
    ReduceNormal = 3,
    ReduceEliminated = 4,
    SyntaxError = 5,
    InternalError = 6,
  }

  public struct TokenState
  {
    public readonly Token Token;
    public readonly short GotoState;
    internal readonly short PrevState;

    internal TokenState(Token token, short gotoState, short prevState)
    {
      this.Token = token;
      this.GotoState = gotoState;
      this.PrevState = prevState;
    }

    internal bool IsExtra => this.PrevState == (short) -1 && this.Token != null;
  }
}
