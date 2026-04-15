// Decompiled with JetBrains decompiler
// Type: GOLDEngine.GrammarProperties
// Assembly: GOLDEngine, Version=5.0.6070.308, Culture=neutral, PublicKeyToken=null
// MVID: 9B853C3A-54DD-4545-BD68-DA26645707EB
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GOLDEngine.dll

#nullable disable
namespace GOLDEngine;

public class GrammarProperties
{
  private const int PropertyCount = 8;
  private string[] m_Property = new string[9];

  internal GrammarProperties()
  {
    for (int index = 0; index <= 7; ++index)
      this.m_Property[index] = "";
  }

  internal void SetValue(int Index, string Value)
  {
    if (!(Index >= 0 & Index < 8))
      return;
    this.m_Property[Index] = Value;
  }

  public string Name => this.m_Property[0];

  public string Version => this.m_Property[1];

  public string Author => this.m_Property[2];

  public string About => this.m_Property[3];

  public string CharacterSet => this.m_Property[4];

  public string CharacterMapping => this.m_Property[5];

  public string GeneratedBy => this.m_Property[6];

  public string GeneratedDate => this.m_Property[7];

  private enum PropertyIndex
  {
    Name,
    Version,
    Author,
    About,
    CharacterSet,
    CharacterMapping,
    GeneratedBy,
    GeneratedDate,
  }
}
