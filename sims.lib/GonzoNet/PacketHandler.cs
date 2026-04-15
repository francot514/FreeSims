// Decompiled with JetBrains decompiler
// Type: GonzoNet.PacketHandler
// Assembly: GonzoNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 75AA73F1-2E7B-40B2-B711-B42047463A5A
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GonzoNet.dll

#nullable disable
namespace GonzoNet;

public class PacketHandler
{
  private byte m_ID;
  private bool m_Encrypted;
  private ushort m_Length;
  private OnPacketReceive m_Handler;
  private bool m_VarLength;

  public PacketHandler(byte id, bool Encrypted, ushort size, OnPacketReceive handler)
  {
    this.m_ID = id;
    this.m_Length = size;
    this.m_Handler = handler;
    this.m_Encrypted = Encrypted;
    if (size == (ushort) 0)
      this.m_VarLength = true;
    else
      this.m_VarLength = false;
  }

  public byte ID => this.m_ID;

  public bool Encrypted => this.m_Encrypted;

  public ushort Length => this.m_Length;

  public bool VariableLength => this.m_VarLength;

  public OnPacketReceive Handler => this.m_Handler;
}
