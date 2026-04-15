// Decompiled with JetBrains decompiler
// Type: GOLDEngine.Position
// Assembly: GOLDEngine, Version=5.0.6070.308, Culture=neutral, PublicKeyToken=null
// MVID: 9B853C3A-54DD-4545-BD68-DA26645707EB
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GOLDEngine.dll

#nullable disable
namespace GOLDEngine;

public struct Position
{
  public readonly int Line;
  public readonly int Column;

  internal Position NextLine => new Position(this.Line + 1, 0);

  internal Position NextColumn => new Position(this.Line, this.Column + 1);

  private Position(int Line, int Column)
  {
    this.Line = Line;
    this.Column = Column;
  }
}
