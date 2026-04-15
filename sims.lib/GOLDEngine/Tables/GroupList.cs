// Decompiled with JetBrains decompiler
// Type: GOLDEngine.Tables.GroupList
// Assembly: GOLDEngine, Version=5.0.6070.308, Culture=neutral, PublicKeyToken=null
// MVID: 9B853C3A-54DD-4545-BD68-DA26645707EB
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GOLDEngine.dll

using System.Collections.Generic;

#nullable disable
namespace GOLDEngine.Tables;

internal class GroupList : List<Group>
{
  internal GroupList(int Size)
    : base(Size)
  {
    for (int index = 0; index < Size; ++index)
      this.Add((Group) null);
  }
}
