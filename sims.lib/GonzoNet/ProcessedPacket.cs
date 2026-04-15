// Decompiled with JetBrains decompiler
// Type: GonzoNet.ProcessedPacket
// Assembly: GonzoNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 75AA73F1-2E7B-40B2-B711-B42047463A5A
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GonzoNet.dll

using GonzoNet.Encryption;
using GonzoNet.Exceptions;
using System;
using System.IO;

#nullable disable
namespace GonzoNet;

public class ProcessedPacket : PacketStream
{
  public volatile ushort DecryptedLength;
  public volatile bool DecryptedSuccessfully = false;

  public ProcessedPacket(
    byte ID,
    bool Encrypted,
    bool VariableLength,
    int Length,
    Encryptor Enc,
    byte[] DataBuffer)
    : base(ID, Length, DataBuffer)
  {
    byte num = (byte) this.ReadByte();
    if (VariableLength)
      this.m_Length = this.ReadInt32();
    else
      this.m_Length = Length;
    if (!Encrypted)
      return;
    this.DecryptedLength = this.ReadUShort();
    if (this.m_Length - 7 < (int) this.DecryptedLength)
      throw new PacketProcessingException("DecryptedLength didn't match packet's length!\n" + Convert.ToBase64String(this.m_BaseStream.ToArray()));
    DecryptionArgsContainer decryptionArgsContainer = Enc.GetDecryptionArgsContainer();
    decryptionArgsContainer.UnencryptedLength = this.DecryptedLength;
    this.m_BaseStream = Enc.DecryptPacket((PacketStream) this, decryptionArgsContainer);
    if (this.m_BaseStream != null)
    {
      this.DecryptedSuccessfully = true;
      this.m_BaseStream.Position = 0L;
      this.m_Reader = new BinaryReader((Stream) this.m_BaseStream);
    }
    else
      this.DecryptedSuccessfully = false;
  }
}
