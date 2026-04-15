// Decompiled with JetBrains decompiler
// Type: TargaImagePCL.TargaConstants
// Assembly: TargaImagePCL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EA3D1F96-8D99-4228-A7A5-0DA52D7AC9D1
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\TargaImagePCL.dll

#nullable disable
namespace TargaImagePCL;

internal static class TargaConstants
{
  internal const int HeaderByteLength = 18;
  internal const int FooterByteLength = 26;
  internal const int FooterSignatureOffsetFromEnd = 18;
  internal const int FooterSignatureByteLength = 16 /*0x10*/;
  internal const int FooterReservedCharByteLength = 1;
  internal const int ExtensionAreaAuthorNameByteLength = 41;
  internal const int ExtensionAreaAuthorCommentsByteLength = 324;
  internal const int ExtensionAreaJobNameByteLength = 41;
  internal const int ExtensionAreaSoftwareIDByteLength = 41;
  internal const int ExtensionAreaSoftwareVersionLetterByteLength = 1;
  internal const int ExtensionAreaColorCorrectionTableValueLength = 256 /*0x0100*/;
  internal const string TargaFooterASCIISignature = "TRUEVISION-XFILE";
}
