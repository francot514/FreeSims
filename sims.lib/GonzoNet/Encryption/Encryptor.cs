// Decompiled with JetBrains decompiler
// Type: GonzoNet.Encryption.Encryptor
// Assembly: GonzoNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 75AA73F1-2E7B-40B2-B711-B42047463A5A
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GonzoNet.dll

using System.IO;

#nullable disable
namespace GonzoNet.Encryption;

public abstract class Encryptor
{
  protected string m_Password;
  public string Username;

  public Encryptor(string Password) => this.m_Password = Password;

  public abstract byte[] FinalizePacket(byte PacketID, byte[] PacketData);

  public abstract DecryptionArgsContainer GetDecryptionArgsContainer();

  public abstract MemoryStream DecryptPacket(
    PacketStream EncryptedPacket,
    DecryptionArgsContainer DecryptionArgs);
}
