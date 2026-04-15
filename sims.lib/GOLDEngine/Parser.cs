// Decompiled with JetBrains decompiler
// Type: GOLDEngine.Parser
// Assembly: GOLDEngine, Version=5.0.6070.308, Culture=neutral, PublicKeyToken=null
// MVID: 9B853C3A-54DD-4545-BD68-DA26645707EB
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GOLDEngine.dll

using GOLDEngine.Tables;
using System;
using System.ComponentModel;
using System.IO;

#nullable disable
namespace GOLDEngine;

public class Parser
{
  private const string kVersion = "5.0";
  private SymbolList m_ExpectedSymbols;
  private bool m_TrimReductions;
  private Lexer m_Lexer;
  private LALRStack m_LALRStack;
  private GroupTerminals m_GroupTerminals;
  private TokenStack m_InputTokens = new TokenStack();
  private Position m_CurrentPosition;
  private EGT m_loaded;
  private Converter<char, ushort> m_charToShort = (Converter<char, ushort>) (c => (ushort) c);

  public Parser() => this.Restart();

  [Description("Opens a string for parsing.")]
  public bool Open(string Text) => this.Open((TextReader) new StringReader(Text));

  [Description("Opens a text stream for parsing.")]
  public bool Open(TextReader Reader)
  {
    this.Restart();
    this.m_Lexer = new Lexer(this.m_loaded, Reader, this.m_charToShort);
    this.m_LALRStack = new LALRStack(this.m_loaded, this.m_TrimReductions);
    this.m_GroupTerminals = new GroupTerminals(this.m_loaded, this.m_Lexer);
    return true;
  }

  public Converter<char, ushort> CharConverter
  {
    set
    {
      this.m_charToShort = value;
      if (this.m_Lexer == null)
        return;
      this.m_Lexer.m_charToShort = value;
    }
  }

  [Description("When the Parse() method returns a Reduce, this method will contain the current Reduction.")]
  public Reduction CurrentReduction => this.m_LALRStack.CurrentReduction;

  [Description("Determines if reductions will be trimmed in cases where a production contains a single element.")]
  public bool TrimReductions
  {
    get => this.m_TrimReductions;
    set
    {
      this.m_TrimReductions = value;
      if (this.m_LALRStack == null)
        return;
      this.m_LALRStack.m_TrimReductions = value;
    }
  }

  [Description("Returns information about the current grammar.")]
  public GrammarProperties Grammar()
  {
    return this.m_loaded != null ? this.m_loaded.Grammar : (GrammarProperties) null;
  }

  [Description("Current line and column being read from the source.")]
  public Position CurrentPosition => this.m_CurrentPosition;

  [Description("If the Parse() function returns TokenRead, this method will return that last read token.")]
  public Token CurrentToken => this.m_InputTokens.Peek();

  public TokenStack TokenStack => this.m_InputTokens;

  public LALRStack LALRStack => this.m_LALRStack;

  public Lexer Lexer => this.m_Lexer;

  public bool ExpectsSymbol(short state, Symbol symbol)
  {
    return this.m_loaded.FindLRAction(state, symbol) != null;
  }

  [Description("Library name and version.")]
  public string About => "GOLD Parser Engine; Version 5.0";

  [Description("Loads parse tables from the specified filename. Only EGT (version 5.0) is supported.")]
  public void LoadTables(string Path)
  {
    using (BinaryReader Reader = new BinaryReader((Stream) File.Open(Path, FileMode.Open, FileAccess.Read)))
      this.LoadTables(Reader);
  }

  public void LoadTables(BinaryReader Reader) => this.m_loaded = new EGT(Reader);

  [Description("Returns a list of Symbols recognized by the grammar.")]
  public SymbolList SymbolTable
  {
    get => this.m_loaded != null ? this.m_loaded.SymbolTable : (SymbolList) null;
  }

  [Description("Returns a list of Productions recognized by the grammar.")]
  public ProductionList ProductionTable
  {
    get => this.m_loaded != null ? this.m_loaded.ProductionTable : (ProductionList) null;
  }

  [Description("If the Parse() method returns a SyntaxError, this method will contain a list of the symbols the grammar expected to see.")]
  public SymbolList ExpectedSymbols => this.m_ExpectedSymbols;

  [Description("Restarts the parser. Loaded tables are retained.")]
  public void Restart()
  {
    this.m_CurrentPosition = new Position();
    this.m_ExpectedSymbols = (SymbolList) null;
    this.m_InputTokens.Clear();
    this.m_LALRStack = (LALRStack) null;
    this.m_Lexer = (Lexer) null;
    this.m_GroupTerminals = (GroupTerminals) null;
  }

  [Description("Returns true if parse tables were loaded.")]
  public bool TablesLoaded => this.m_loaded != null;

  [Description("Performs a parse action on the input. This method is typically used in a loop until either grammar is accepted or an error occurs.")]
  public ParseMessage Parse()
  {
    if (!this.TablesLoaded)
      return ParseMessage.NotLoadedError;
    while (this.m_InputTokens.Count != 0)
    {
      Token NextToken = this.m_InputTokens.Peek();
      Position? position = NextToken.Position;
      if (position.HasValue)
      {
        position = NextToken.Position;
        this.m_CurrentPosition = position.Value;
      }
      if (this.m_GroupTerminals.Count != 0)
        return ParseMessage.GroupError;
      if (NextToken.SymbolType == SymbolType.Noise)
      {
        this.m_InputTokens.Pop();
      }
      else
      {
        if (NextToken.SymbolType == SymbolType.Error)
          return ParseMessage.LexicalError;
        switch (this.m_LALRStack.ParseLALR(NextToken))
        {
          case LALRStack.ParseResult.Accept:
            return ParseMessage.Accept;
          case LALRStack.ParseResult.Shift:
            this.m_InputTokens.Pop();
            continue;
          case LALRStack.ParseResult.ReduceNormal:
            return ParseMessage.Reduction;
          case LALRStack.ParseResult.SyntaxError:
            this.m_ExpectedSymbols = this.m_LALRStack.GetExpectedSymbols();
            return ParseMessage.SyntaxError;
          case LALRStack.ParseResult.InternalError:
            return ParseMessage.InternalError;
          default:
            continue;
        }
      }
    }
    this.m_InputTokens.Push(this.m_GroupTerminals.ProduceToken());
    return ParseMessage.TokenRead;
  }
}
