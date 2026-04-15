// Decompiled with JetBrains decompiler
// Type: GOLDEngine.Lexer
// Assembly: GOLDEngine, Version=5.0.6070.308, Culture=neutral, PublicKeyToken=null
// MVID: 9B853C3A-54DD-4545-BD68-DA26645707EB
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GOLDEngine.dll

using GOLDEngine.Tables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#nullable disable
namespace GOLDEngine;

public class Lexer
{
  private EGT m_loaded;
  private TextReader m_source;
  private StringBuilder m_buffer = new StringBuilder();
  private Position m_SysPosition;
  internal Converter<char, ushort> m_charToShort = (Converter<char, ushort>) (c => (ushort) c);

  internal Lexer(EGT loaded, TextReader source, Converter<char, ushort> charToShort)
  {
    this.m_loaded = loaded;
    this.m_source = source;
    this.m_charToShort = charToShort;
  }

  public void PushFront(string text) => this.m_buffer.Insert(0, text);

  internal Converter<char, ushort> charToShort
  {
    get => this.m_charToShort;
    set => this.m_charToShort = value;
  }

  public Terminal PeekNextTerminal()
  {
    short CurrentDFA1 = this.m_loaded.InitialDFAState;
    short CurrentDFA2 = -1;
    int num = -1;
    if (!this.Lookahead(0).HasValue)
      return this.CreateTerminal(this.m_loaded.GetFirstSymbolOfType(SymbolType.End), 0);
    int CharIndex = 0;
    while (true)
    {
      char? nullable1 = this.Lookahead(CharIndex);
      short? nullable2 = new short?();
      if (nullable1.HasValue)
      {
        ushort CharCode = this.m_charToShort(nullable1.Value);
        foreach (FAEdge edge in (List<FAEdge>) this.m_loaded.GetFAState(CurrentDFA1).Edges)
        {
          if (edge.Characters.Contains(CharCode))
          {
            nullable2 = new short?(edge.Target);
            break;
          }
        }
      }
      if (nullable2.HasValue)
      {
        short CurrentDFA3 = nullable2.Value;
        if (this.m_loaded.GetFAState(CurrentDFA3).Accept != null)
        {
          CurrentDFA2 = CurrentDFA3;
          num = CharIndex;
        }
        CurrentDFA1 = CurrentDFA3;
        ++CharIndex;
      }
      else
        break;
    }
    return CurrentDFA2 == (short) -1 ? this.CreateTerminal(this.m_loaded.GetFirstSymbolOfType(SymbolType.Error), 1) : this.CreateTerminal(this.m_loaded.GetFAState(CurrentDFA2).Accept, num + 1);
  }

  private Terminal CreateTerminal(Symbol symbol, int Count)
  {
    if (Count > this.m_buffer.Length)
      Count = this.m_buffer.Length;
    string Text = this.m_buffer.ToString(0, Count);
    return new Terminal(symbol, Text, this.m_SysPosition);
  }

  public void ConsumeBuffer(Terminal terminal) => this.ConsumeBuffer(terminal.Text.Length);

  public void ConsumeBuffer(int CharCount)
  {
    if (CharCount > this.m_buffer.Length)
      return;
    for (int index = 0; index <= CharCount - 1; ++index)
    {
      switch (this.m_buffer[index])
      {
        case '\n':
          this.m_SysPosition = this.m_SysPosition.NextLine;
          continue;
        case '\r':
          continue;
        default:
          this.m_SysPosition = this.m_SysPosition.NextColumn;
          continue;
      }
    }
    this.m_buffer.Remove(0, CharCount);
  }

  public char? Lookahead(int CharIndex)
  {
    if (CharIndex >= this.m_buffer.Length)
    {
      int num1 = CharIndex + 1 - this.m_buffer.Length;
      for (int index = 0; index < num1; ++index)
      {
        int num2 = this.m_source.Read();
        if (num2 != -1)
          this.m_buffer.Append((char) num2);
        else
          break;
      }
    }
    return CharIndex < this.m_buffer.Length ? new char?(this.m_buffer[CharIndex]) : new char?();
  }
}
