// Decompiled with JetBrains decompiler
// Type: GonzoNet.PacketHandlers
// Assembly: GonzoNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 75AA73F1-2E7B-40B2-B711-B42047463A5A
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GonzoNet.dll

using System.Collections.Generic;

#nullable disable
namespace GonzoNet;

public class PacketHandlers
{
  private static Dictionary<byte, PacketHandler> m_Handlers = new Dictionary<byte, PacketHandler>();

  public static void Register(byte id, bool Encrypted, ushort size, OnPacketReceive handler)
  {
    PacketHandlers.m_Handlers.Add(id, new PacketHandler(id, Encrypted, size, handler));
  }

  public static PacketHandler Get(byte id)
  {
    return PacketHandlers.m_Handlers.ContainsKey(id) ? PacketHandlers.m_Handlers[id] : (PacketHandler) null;
  }
}
