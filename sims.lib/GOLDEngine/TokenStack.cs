// Decompiled with JetBrains decompiler
// Type: GOLDEngine.TokenStack
// Assembly: GOLDEngine, Version=5.0.6070.308, Culture=neutral, PublicKeyToken=null
// MVID: 9B853C3A-54DD-4545-BD68-DA26645707EB
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GOLDEngine.dll

using System.Collections.Generic;

#nullable disable
namespace GOLDEngine;

public class TokenStack : Stack<Token>
{
  public void Enqueue(Token item)
  {
    Stack<Token> tokenStack = new Stack<Token>(this.Count);
    while (this.Count > 0)
      tokenStack.Push(this.Pop());
    this.Push(item);
    while (tokenStack.Count > 0)
      this.Push(tokenStack.Pop());
  }
}
