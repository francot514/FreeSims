// Decompiled with JetBrains decompiler
// Type: GOLDEngine.GroupTerminals
// Assembly: GOLDEngine, Version=5.0.6070.308, Culture=neutral, PublicKeyToken=null
// MVID: 9B853C3A-54DD-4545-BD68-DA26645707EB
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GOLDEngine.dll

using GOLDEngine.Tables;
using System.Collections.Generic;

#nullable disable
namespace GOLDEngine;

internal class GroupTerminals
{
  private Stack<GroupTerminals.GroupTerminal> m_GroupStack = new Stack<GroupTerminals.GroupTerminal>();
  private EGT m_loaded;
  private Lexer m_Lexer;

  internal GroupTerminals(EGT loaded, Lexer lexer)
  {
    this.m_loaded = loaded;
    this.m_Lexer = lexer;
  }

  internal int Count => this.m_GroupStack.Count;

  internal Token ProduceToken()
  {
    Terminal terminal;
    GroupTerminals.GroupTerminal groupTerminal1;
    while (true)
    {
      terminal = this.m_Lexer.PeekNextTerminal();
      bool flag;
      if (terminal.SymbolType == SymbolType.GroupStart)
      {
        if (this.m_GroupStack.Count == 0)
        {
          flag = true;
        }
        else
        {
          Group group = this.m_loaded.GetGroup(terminal.Symbol);
          flag = this.m_GroupStack.Peek().Group.CanNestGroup(group);
        }
      }
      else
        flag = false;
      if (flag)
      {
        this.m_Lexer.ConsumeBuffer(terminal);
        this.m_GroupStack.Push(new GroupTerminals.GroupTerminal(terminal, this.m_loaded.GetGroup(terminal.Symbol)));
      }
      else if (this.m_GroupStack.Count != 0)
      {
        if (this.m_GroupStack.Peek().Group.End == terminal.Symbol)
        {
          groupTerminal1 = this.m_GroupStack.Pop();
          if (groupTerminal1.Group.Ending == Group.EndingMode.Closed)
          {
            groupTerminal1.Text += terminal.Text;
            this.m_Lexer.ConsumeBuffer(terminal);
          }
          if (this.m_GroupStack.Count != 0)
            this.m_GroupStack.Peek().Text += groupTerminal1.Text;
          else
            goto label_14;
        }
        else if (terminal.SymbolType != SymbolType.End)
        {
          GroupTerminals.GroupTerminal groupTerminal2 = this.m_GroupStack.Peek();
          if (groupTerminal2.Group.Advance == Group.AdvanceMode.Token)
          {
            groupTerminal2.Text += terminal.Text;
            this.m_Lexer.ConsumeBuffer(terminal);
          }
          else
          {
            groupTerminal2.Text += terminal.Text[0].ToString();
            this.m_Lexer.ConsumeBuffer(1);
          }
        }
        else
          goto label_17;
      }
      else
        break;
    }
    this.m_Lexer.ConsumeBuffer(terminal);
    return (Token) terminal;
label_14:
    return (Token) groupTerminal1.CreateTerminal();
label_17:
    return (Token) terminal;
  }

  private class GroupTerminal
  {
    private readonly Terminal Terminal;
    internal readonly Group Group;
    internal string Text;

    internal GroupTerminal(Terminal terminal, Group Group)
    {
      this.Terminal = terminal;
      this.Group = Group;
      this.Text = terminal.Text;
    }

    internal Terminal CreateTerminal()
    {
      return new Terminal(this.Group.Container, this.Text, this.Terminal.Position.Value);
    }
  }
}
