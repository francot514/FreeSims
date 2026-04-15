// Decompiled with JetBrains decompiler
// Type: TargaImagePCL.TargaFooter
// Assembly: TargaImagePCL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EA3D1F96-8D99-4228-A7A5-0DA52D7AC9D1
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\TargaImagePCL.dll

#nullable disable
namespace TargaImagePCL;

public class TargaFooter
{
  private int intExtensionAreaOffset;
  private int intDeveloperDirectoryOffset;
  private string strSignature = string.Empty;
  private string strReservedCharacter = string.Empty;

  public int ExtensionAreaOffset => this.intExtensionAreaOffset;

  protected internal void SetExtensionAreaOffset(int intExtensionAreaOffset)
  {
    this.intExtensionAreaOffset = intExtensionAreaOffset;
  }

  public int DeveloperDirectoryOffset => this.intDeveloperDirectoryOffset;

  protected internal void SetDeveloperDirectoryOffset(int intDeveloperDirectoryOffset)
  {
    this.intDeveloperDirectoryOffset = intDeveloperDirectoryOffset;
  }

  public string Signature => this.strSignature;

  protected internal void SetSignature(string strSignature) => this.strSignature = strSignature;

  public string ReservedCharacter => this.strReservedCharacter;

  protected internal void SetReservedCharacter(string strReservedCharacter)
  {
    this.strReservedCharacter = strReservedCharacter;
  }
}
