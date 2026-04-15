// Decompiled with JetBrains decompiler
// Type: GOLDEngine.Terminal
// Assembly: GOLDEngine, Version=5.0.6070.308, Culture=neutral, PublicKeyToken=null
// MVID: 9B853C3A-54DD-4545-BD68-DA26645707EB
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GOLDEngine.dll

#nullable disable
namespace GOLDEngine;

public class Terminal : Token
{
  private readonly string m_Text;

  public static Terminal CreateVirtual(Symbol Parent, string Text) => new Terminal(Parent, Text);

  private Terminal(Symbol Parent, string Text)
    : base(Parent, new GOLDEngine.Position?(), true)
  {
    this.m_Text = Text;
  }

  internal Terminal(Symbol Parent, string Text, GOLDEngine.Position sysPosition)
    : base(Parent, new GOLDEngine.Position?(sysPosition), true)
  {
    this.m_Text = Text;
  }

  public string Text => this.m_Text;

  public override string ToString() => this.m_Text;

  public override void Visit(ITokenVisitor visitor) => visitor.OnTerminal(this);
}
