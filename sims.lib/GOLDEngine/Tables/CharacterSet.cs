// Decompiled with JetBrains decompiler
// Type: GOLDEngine.Tables.CharacterSet
// Assembly: GOLDEngine, Version=5.0.6070.308, Culture=neutral, PublicKeyToken=null
// MVID: 9B853C3A-54DD-4545-BD68-DA26645707EB
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GOLDEngine.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace GOLDEngine.Tables;

internal class CharacterSet : List<CharacterRange>
{
  public bool Contains(ushort CharCode)
  {
    return this.Exists((Predicate<CharacterRange>) (range => (int) CharCode >= (int) range.Start & (int) CharCode <= (int) range.End));
  }
}
