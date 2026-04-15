// Decompiled with JetBrains decompiler
// Type: GOLDEngine.TokenToJson
// Assembly: GOLDEngine, Version=5.0.6070.308, Culture=neutral, PublicKeyToken=null
// MVID: 9B853C3A-54DD-4545-BD68-DA26645707EB
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GOLDEngine.dll

using System.Text;

#nullable disable
namespace GOLDEngine;

internal class TokenToJson : ITokenVisitor
{
  private StringBuilder m_stringBuilder = new StringBuilder();
  private int m_nesting;

  public void OnTerminal(Terminal terminal)
  {
    this.TokenStart((Token) terminal);
    this.m_stringBuilder.Append(TokenToJson.Escape(terminal.Text));
    this.TokenEnd();
  }

  public void OnReduction(Reduction reduction)
  {
    this.TokenStart((Token) reduction);
    int length = reduction.Tokens.Length;
    if (length == 0)
    {
      this.m_stringBuilder.Append("[]");
    }
    else
    {
      this.m_stringBuilder.AppendLine("[");
      ++this.m_nesting;
      for (int index = 0; index < length; ++index)
      {
        reduction.Tokens[index].Visit((ITokenVisitor) this);
        if (index != length - 1)
          this.m_stringBuilder.AppendLine(",");
      }
      --this.m_nesting;
      this.m_stringBuilder.AppendLine();
      this.AppendIndent();
      this.m_stringBuilder.Append("]");
    }
    this.TokenEnd();
  }

  private void AppendIndent() => this.m_stringBuilder.Append(' ', this.m_nesting * 4);

  private void TokenStart(Token token)
  {
    this.AppendIndent();
    this.m_stringBuilder.AppendLine("{");
    ++this.m_nesting;
    this.AppendIndent();
    this.m_stringBuilder.Append(TokenToJson.Escape(token.SymbolName));
    this.m_stringBuilder.Append(": ");
  }

  private void TokenEnd()
  {
    --this.m_nesting;
    this.m_stringBuilder.AppendLine();
    this.AppendIndent();
    this.m_stringBuilder.Append("}");
  }

  private static string Escape(string text)
  {
    StringBuilder stringBuilder = new StringBuilder();
    stringBuilder.Append("\"");
    foreach (char ch in text)
    {
      switch (ch)
      {
        case '\b':
          stringBuilder.Append("\\b");
          break;
        case '\t':
          stringBuilder.Append("\\t");
          break;
        case '\n':
          stringBuilder.Append("\\n");
          break;
        case '\f':
          stringBuilder.Append("\\f");
          break;
        case '\r':
          stringBuilder.Append("\\r");
          break;
        case '"':
          stringBuilder.Append("\\\"");
          break;
        case '\'':
          stringBuilder.Append("\\'");
          break;
        case '\\':
          stringBuilder.Append("\\\\");
          break;
        default:
          stringBuilder.Append(ch);
          break;
      }
    }
    stringBuilder.Append("\"");
    return stringBuilder.ToString();
  }

  public override string ToString() => this.m_stringBuilder.ToString();
}
