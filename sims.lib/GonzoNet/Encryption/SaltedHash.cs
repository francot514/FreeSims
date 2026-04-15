// Decompiled with JetBrains decompiler
// Type: GonzoNet.Encryption.SaltedHash
// Assembly: GonzoNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 75AA73F1-2E7B-40B2-B711-B42047463A5A
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GonzoNet.dll

using System;
using System.Security.Cryptography;
using System.Text;

#nullable disable
namespace GonzoNet.Encryption;

public class SaltedHash
{
  private HashAlgorithm HashProvider;
  private int SalthLength;

  public SaltedHash(HashAlgorithm HashAlgorithm, int theSaltLength)
  {
    this.HashProvider = HashAlgorithm;
    this.SalthLength = theSaltLength;
  }

  public SaltedHash()
    : this((HashAlgorithm) new SHA256Managed(), 4)
  {
  }

  private byte[] ComputeHash(byte[] Data, byte[] Salt)
  {
    byte[] numArray = new byte[Data.Length + this.SalthLength];
    Array.Copy((Array) Data, (Array) numArray, Data.Length);
    Array.Copy((Array) Salt, 0, (Array) numArray, Data.Length, this.SalthLength);
    return this.HashProvider.ComputeHash(numArray);
  }

  public byte[] ComputePasswordHash(string Username, string Password)
  {
    byte[] bytes1 = Encoding.ASCII.GetBytes(Username);
    byte[] bytes2 = Encoding.ASCII.GetBytes(Password);
    byte[] numArray = new byte[bytes1.Length + bytes2.Length];
    Array.Copy((Array) bytes2, (Array) numArray, bytes2.Length);
    Array.Copy((Array) bytes1, 0, (Array) numArray, bytes2.Length, bytes1.Length);
    return this.HashProvider.ComputeHash(numArray);
  }

  public void GetHashAndSalt(byte[] Data, out byte[] Hash, out byte[] Salt)
  {
    Salt = new byte[this.SalthLength];
    new RNGCryptoServiceProvider().GetNonZeroBytes(Salt);
    Hash = this.ComputeHash(Data, Salt);
  }

  public void GetHashAndSaltString(string Data, out string Hash, out string Salt)
  {
    byte[] Hash1;
    byte[] Salt1;
    this.GetHashAndSalt(Encoding.UTF8.GetBytes(Data), out Hash1, out Salt1);
    Hash = Convert.ToBase64String(Hash1);
    Salt = Convert.ToBase64String(Salt1);
  }

  public bool VerifyHash(byte[] Data, byte[] Hash, byte[] Salt)
  {
    byte[] hash = this.ComputeHash(Data, Salt);
    if (hash.Length != Hash.Length)
      return false;
    for (int index = 0; index < Hash.Length; ++index)
    {
      if (!Hash[index].Equals(hash[index]))
        return false;
    }
    return true;
  }

  public bool VerifyHashString(string Data, string Hash, string Salt)
  {
    byte[] Hash1 = Convert.FromBase64String(Hash);
    byte[] Salt1 = Convert.FromBase64String(Salt);
    return this.VerifyHash(Encoding.UTF8.GetBytes(Data), Hash1, Salt1);
  }
}
