// Decompiled with JetBrains decompiler
// Type: GOLDEngine.ParserException
// Assembly: GOLDEngine, Version=5.0.6070.308, Culture=neutral, PublicKeyToken=null
// MVID: 9B853C3A-54DD-4545-BD68-DA26645707EB
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GOLDEngine.dll

using System;

#nullable disable
namespace GOLDEngine;

public class ParserException : Exception
{
  public string Method;

  internal ParserException(string Message)
    : base(Message)
  {
    this.Method = "";
  }

  internal ParserException(string Message, Exception Inner, string Method)
    : base(Message, Inner)
  {
    this.Method = Method;
  }
}
