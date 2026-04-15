// Decompiled with JetBrains decompiler
// Type: GOLDEngine.Tables.Group
// Assembly: GOLDEngine, Version=5.0.6070.308, Culture=neutral, PublicKeyToken=null
// MVID: 9B853C3A-54DD-4545-BD68-DA26645707EB
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GOLDEngine.dll

#nullable disable
namespace GOLDEngine.Tables;

internal class Group
{
  internal short TableIndex;
  internal string Name;
  internal Symbol Container;
  internal Symbol Start;
  internal Symbol End;
  internal Group.AdvanceMode Advance;
  internal Group.EndingMode Ending;
  internal IntegerList Nesting;

  internal bool CanNestGroup(Group otherGroup)
  {
    return this.Nesting.Contains((int) otherGroup.TableIndex);
  }

  internal Group()
  {
    this.Advance = Group.AdvanceMode.Character;
    this.Ending = Group.EndingMode.Closed;
    this.Nesting = new IntegerList();
  }

  public enum AdvanceMode
  {
    Token,
    Character,
  }

  public enum EndingMode
  {
    Open,
    Closed,
  }
}
