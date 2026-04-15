// Decompiled with JetBrains decompiler
// Type: GonzoNet.Encryption.ARC4Encryptor
// Assembly: GonzoNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 75AA73F1-2E7B-40B2-B711-B42047463A5A
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GonzoNet.dll

using System.IO;
using System.Security.Cryptography;
using System.Text;

#nullable disable
namespace GonzoNet.Encryption;

public class ARC4Encryptor : Encryptor
{
  private DESCryptoServiceProvider m_CryptoService = new DESCryptoServiceProvider();
  private ICryptoTransform m_DecryptTransformer;
  private ICryptoTransform m_EncryptTransformer;
  public byte[] EncryptionKey;

  public ARC4Encryptor(string Password)
    : base(Password)
  {
    this.EncryptionKey = new PasswordDeriveBytes(Encoding.ASCII.GetBytes(Password), Encoding.ASCII.GetBytes("SALT"), "SHA1", 10).GetBytes(8);
    this.m_DecryptTransformer = this.m_CryptoService.CreateDecryptor(this.EncryptionKey, Encoding.ASCII.GetBytes("@1B2c3D4e5F6g7H8"));
    this.m_EncryptTransformer = this.m_CryptoService.CreateEncryptor(this.EncryptionKey, Encoding.ASCII.GetBytes("@1B2c3D4e5F6g7H8"));
  }

  public ARC4Encryptor(string Password, byte[] EncKey)
    : base(Password)
  {
    PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(Encoding.ASCII.GetBytes(Password), Encoding.ASCII.GetBytes("SALT"), "SHA1", 10);
    this.EncryptionKey = EncKey;
    this.m_DecryptTransformer = this.m_CryptoService.CreateDecryptor(this.EncryptionKey, Encoding.ASCII.GetBytes("@1B2c3D4e5F6g7H8"));
    this.m_EncryptTransformer = this.m_CryptoService.CreateEncryptor(this.EncryptionKey, Encoding.ASCII.GetBytes("@1B2c3D4e5F6g7H8"));
  }

  public override DecryptionArgsContainer GetDecryptionArgsContainer()
  {
    DecryptionArgsContainer decryptionArgsContainer = new DecryptionArgsContainer()
    {
      ARC4DecryptArgs = new ARC4DecryptionArgs()
    };
    decryptionArgsContainer.ARC4DecryptArgs.Transformer = this.m_DecryptTransformer;
    return decryptionArgsContainer;
  }

  public override byte[] FinalizePacket(byte PacketID, byte[] PacketData)
  {
    MemoryStream output = new MemoryStream();
    BinaryWriter binaryWriter = new BinaryWriter((Stream) output);
    MemoryStream memoryStream = new MemoryStream();
    CryptoStream cryptoStream = new CryptoStream((Stream) memoryStream, this.m_EncryptTransformer, CryptoStreamMode.Write);
    cryptoStream.Write(PacketData, 0, PacketData.Length);
    cryptoStream.FlushFinalBlock();
    binaryWriter.Write(PacketID);
    binaryWriter.Write((ushort) (7UL + (ulong) memoryStream.Length));
    binaryWriter.Write((ushort) PacketData.Length);
    binaryWriter.Flush();
    binaryWriter.Write(memoryStream.ToArray());
    binaryWriter.Flush();
    byte[] array = output.ToArray();
    binaryWriter.Close();
    return array;
  }

  public override MemoryStream DecryptPacket(
    PacketStream EncryptedPacket,
    DecryptionArgsContainer DecryptionArgs)
  {
    CryptoStream cryptoStream = new CryptoStream((Stream) EncryptedPacket, this.m_DecryptTransformer, CryptoStreamMode.Read);
    byte[] buffer = new byte[(int) DecryptionArgs.UnencryptedLength];
    cryptoStream.Read(buffer, 0, buffer.Length);
    return new MemoryStream(buffer);
  }
}
