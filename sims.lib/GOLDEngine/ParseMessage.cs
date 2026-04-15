// Decompiled with JetBrains decompiler
// Type: GOLDEngine.ParseMessage
// Assembly: GOLDEngine, Version=5.0.6070.308, Culture=neutral, PublicKeyToken=null
// MVID: 9B853C3A-54DD-4545-BD68-DA26645707EB
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GOLDEngine.dll

#nullable disable
namespace GOLDEngine;

public enum ParseMessage
{
  TokenRead,
  Reduction,
  Accept,
  NotLoadedError,
  LexicalError,
  SyntaxError,
  GroupError,
  InternalError,
}
