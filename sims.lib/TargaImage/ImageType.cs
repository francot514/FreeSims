// Decompiled with JetBrains decompiler
// Type: TargaImagePCL.ImageType
// Assembly: TargaImagePCL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EA3D1F96-8D99-4228-A7A5-0DA52D7AC9D1
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\TargaImagePCL.dll

#nullable disable
namespace TargaImagePCL;

public enum ImageType : byte
{
  NO_IMAGE_DATA = 0,
  UNCOMPRESSED_COLOR_MAPPED = 1,
  UNCOMPRESSED_TRUE_COLOR = 2,
  UNCOMPRESSED_BLACK_AND_WHITE = 3,
  RUN_LENGTH_ENCODED_COLOR_MAPPED = 9,
  RUN_LENGTH_ENCODED_TRUE_COLOR = 10, // 0x0A
  RUN_LENGTH_ENCODED_BLACK_AND_WHITE = 11, // 0x0B
}
