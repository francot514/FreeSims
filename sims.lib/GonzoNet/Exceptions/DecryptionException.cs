// Decompiled with JetBrains decompiler
// Type: GonzoNet.Exceptions.DecryptionException
// Assembly: GonzoNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 75AA73F1-2E7B-40B2-B711-B42047463A5A
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GonzoNet.dll

using GonzoNet.Events;
using System;

#nullable disable
namespace GonzoNet.Exceptions;

public class DecryptionException(string Description) : Exception(Description)
{
  public EventCodes ErrorCode = EventCodes.PACKET_DECRYPTION_ERROR;
}
