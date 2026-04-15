// Decompiled with JetBrains decompiler
// Type: GonzoNet.Encryption.AES
// Assembly: GonzoNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 75AA73F1-2E7B-40B2-B711-B42047463A5A
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GonzoNet.dll

using System.Security.Cryptography;
using System.Text;

#nullable disable
namespace GonzoNet.Encryption;

internal class AES
{
  private SymmetricAlgorithm AESProvider;
  private ICryptoTransform encryptor;
  private ICryptoTransform decryptor;

  public AES(byte[] key, byte[] IV)
  {
    this.AESProvider = (SymmetricAlgorithm) new AesCryptoServiceProvider();
    this.AESProvider.Mode = CipherMode.CBC;
    this.AESProvider.Key = key;
    this.AESProvider.IV = IV;
    this.encryptor = this.AESProvider.CreateEncryptor();
    this.decryptor = this.AESProvider.CreateDecryptor();
  }

  public string Encrypt(string plainText)
  {
    byte[] bytes = Encoding.Unicode.GetBytes(plainText);
    return Encoding.Unicode.GetString(this.encryptor.TransformFinalBlock(bytes, 0, bytes.Length));
  }

  public byte[] Encrypt(byte[] plainBytes)
  {
    return this.encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
  }

  public string Decrypt(string secureText)
  {
    byte[] bytes = Encoding.Unicode.GetBytes(secureText);
    return Encoding.Unicode.GetString(this.decryptor.TransformFinalBlock(bytes, 0, bytes.Length));
  }

  public byte[] Decrypt(byte[] secureBytes)
  {
    return this.decryptor.TransformFinalBlock(secureBytes, 0, secureBytes.Length);
  }
}
