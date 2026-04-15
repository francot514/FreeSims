// Decompiled with JetBrains decompiler
// Type: GOLDEngine.WrapGOLDEngine
// Assembly: GOLDEngine, Version=5.0.6070.308, Culture=neutral, PublicKeyToken=null
// MVID: 9B853C3A-54DD-4545-BD68-DA26645707EB
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GOLDEngine.dll

using System.IO;

#nullable disable
namespace GOLDEngine;

public class WrapGOLDEngine
{
  private Parser m_parser = new Parser();
  private string m_FailMessage;

  public Parser Parser => this.m_parser;

  public void LoadTables(string grammarFilename)
  {
    using (Stream grammar = (Stream) new FileStream(grammarFilename, FileMode.Open))
      this.LoadTables(grammar);
  }

  public void LoadTables(Stream grammar)
  {
    using (BinaryReader Reader = new BinaryReader(grammar))
      this.m_parser.LoadTables(Reader);
  }

  public Reduction ParseString(string contentString)
  {
    using (TextReader content = (TextReader) new StringReader(contentString))
      return this.Parse(content);
  }

  public Reduction ParseFile(string contentFilename)
  {
    using (TextReader content = (TextReader) new StreamReader(contentFilename))
      return this.Parse(content);
  }

  public Reduction Parse(TextReader content)
  {
    this.m_parser.Open(content);
    return !this.doParsing() ? (Reduction) null : this.m_parser.CurrentReduction;
  }

  private bool doParsing()
  {
    bool? nullable;
    do
    {
      nullable = this.parse();
    }
    while (!nullable.HasValue);
    return nullable.Value;
  }

  public string FailMessage => this.m_FailMessage;

  protected virtual bool? OnLexicalError()
  {
    this.m_FailMessage = $"Lexical Error:\nPosition: {(object) this.m_parser.CurrentPosition.Line}, {(object) this.m_parser.CurrentPosition.Column}\nRead: {this.m_parser.CurrentToken.ToString()}";
    return new bool?(false);
  }

  protected virtual bool? OnSyntaxError()
  {
    this.m_FailMessage = $"Syntax Error:\nPosition: {(object) this.m_parser.CurrentPosition.Line}, {(object) this.m_parser.CurrentPosition.Column}\nRead: {this.m_parser.CurrentToken.ToString()}\nExpecting: {this.m_parser.ExpectedSymbols.Text()}";
    return new bool?(false);
  }

  protected virtual bool? OnReduction() => new bool?();

  protected virtual bool? OnAccept() => new bool?(true);

  protected virtual bool? OnTokenRead() => new bool?();

  protected virtual bool? OnError(ParseMessage response, string message)
  {
    this.m_FailMessage = message;
    return new bool?(false);
  }

  private bool? parse()
  {
    ParseMessage response = this.m_parser.Parse();
    switch (response)
    {
      case ParseMessage.TokenRead:
        return this.OnTokenRead();
      case ParseMessage.Reduction:
        return this.OnReduction();
      case ParseMessage.Accept:
        return this.OnAccept();
      case ParseMessage.NotLoadedError:
        return this.OnError(response, "Tables not loaded");
      case ParseMessage.LexicalError:
        return this.OnLexicalError();
      case ParseMessage.SyntaxError:
        return this.OnSyntaxError();
      case ParseMessage.GroupError:
        return this.OnError(response, "Runaway group");
      case ParseMessage.InternalError:
        return this.OnError(response, "Internal error");
      default:
        return this.OnError(response, "Unexpected response");
    }
  }
}
