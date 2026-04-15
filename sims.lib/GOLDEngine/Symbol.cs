// Decompiled with JetBrains decompiler
// Type: GOLDEngine.Symbol
// Assembly: GOLDEngine, Version=5.0.6070.308, Culture=neutral, PublicKeyToken=null
// MVID: 9B853C3A-54DD-4545-BD68-DA26645707EB
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GOLDEngine.dll

using System.ComponentModel;

#nullable disable
namespace GOLDEngine;

public class Symbol
{
  private string m_Name;
  private SymbolType m_Type;
  private short m_TableIndex;

  internal Symbol(string Name, SymbolType Type, short TableIndex)
  {
    this.m_Name = Name;
    this.m_Type = Type;
    this.m_TableIndex = TableIndex;
  }

  [Description("Returns the type of the symbol.")]
  public SymbolType Type => this.m_Type;

  [Description("Returns the index of the symbol in the Symbol Table,")]
  public short TableIndex() => this.m_TableIndex;

  [Description("Returns the name of the symbol.")]
  public string Name => this.m_Name;

  [Description("Returns the text representing the text in BNF format.")]
  public string Text(bool AlwaysDelimitTerminals)
  {
    string str;
    switch (this.m_Type)
    {
      case SymbolType.Nonterminal:
        str = $"<{this.m_Name}>";
        break;
      case SymbolType.Content:
        str = this.LiteralFormat(this.m_Name, AlwaysDelimitTerminals);
        break;
      default:
        str = $"({this.m_Name})";
        break;
    }
    return str;
  }

  [Description("Returns the text representing the text in BNF format.")]
  public string Text() => this.Text(false);

  private string LiteralFormat(string Source, bool ForceDelimit)
  {
    if (Source == "'")
      return "''";
    for (short index = 0; (int) index < Source.Length & !ForceDelimit; ++index)
    {
      char c = Source[(int) index];
      ForceDelimit = !(char.IsLetter(c) | c == '.' | c == '_' | c == '-');
    }
    return ForceDelimit ? $"'{Source}'" : Source;
  }

  public override string ToString() => this.Text();
}
