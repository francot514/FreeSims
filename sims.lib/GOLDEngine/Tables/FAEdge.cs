// Decompiled with JetBrains decompiler
// Type: GOLDEngine.Tables.FAEdge
// Assembly: GOLDEngine, Version=5.0.6070.308, Culture=neutral, PublicKeyToken=null
// MVID: 9B853C3A-54DD-4545-BD68-DA26645707EB
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GOLDEngine.dll

#nullable disable
namespace GOLDEngine.Tables;

internal class FAEdge
{
  public CharacterSet Characters;
  public short Target;

  public FAEdge(CharacterSet CharSet, short Target)
  {
    this.Characters = CharSet;
    this.Target = Target;
  }

  public FAEdge()
  {
  }
}
