// Decompiled with JetBrains decompiler
// Type: ProtocolAbstractionLibraryD.CityInfo
// Assembly: GonzoNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 75AA73F1-2E7B-40B2-B711-B42047463A5A
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GonzoNet.dll

using System.Collections.Generic;

#nullable disable
namespace ProtocolAbstractionLibraryD;

public class CityInfo
{
  public string Name;
  public string Description;
  public ulong Thumbnail;
  public string UUID;
  public ulong Map;
  public bool Online = true;
  public CityInfoStatus Status;
  public string IP;
  public int Port;
  public List<CityInfoMessageOfTheDay> Messages = new List<CityInfoMessageOfTheDay>();

  public CityInfo(bool IsServer)
  {
  }
}
