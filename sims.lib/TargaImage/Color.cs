// Decompiled with JetBrains decompiler
// Type: TargaImagePCL.Color
// Assembly: TargaImagePCL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EA3D1F96-8D99-4228-A7A5-0DA52D7AC9D1
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\TargaImagePCL.dll

#nullable disable
namespace TargaImagePCL;

public struct Color
{
  public static Color Empty = Color.FromArgb(0, 0, 0, 0);
  public byte R;
  public byte G;
  public byte B;
  public byte A;

  public static Color FromArgb(int a, int r, int g, int b)
  {
    return new Color()
    {
      R = (byte) r,
      G = (byte) g,
      B = (byte) b,
      A = (byte) a
    };
  }

  public static Color FromArgb(int r, int g, int b)
  {
    return new Color()
    {
      R = (byte) r,
      G = (byte) g,
      B = (byte) b,
      A = byte.MaxValue
    };
  }
}
