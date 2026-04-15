// Decompiled with JetBrains decompiler
// Type: TargaImagePCL.Utilities
// Assembly: TargaImagePCL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EA3D1F96-8D99-4228-A7A5-0DA52D7AC9D1
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\TargaImagePCL.dll

#nullable disable
namespace TargaImagePCL;

internal static class Utilities
{
  internal static int GetBits(byte b, int offset, int count)
  {
    return (int) b >> offset & (1 << count) - 1;
  }

  internal static Color GetColorFrom2Bytes(byte one, byte two)
  {
    int r = Utilities.GetBits(one, 2, 5) << 3;
    int g = (Utilities.GetBits(one, 0, 2) << 6) + (Utilities.GetBits(two, 5, 3) << 3);
    int b = Utilities.GetBits(two, 0, 5) << 3;
    return Color.FromArgb(Utilities.GetBits(one, 7, 1) * (int) byte.MaxValue, r, g, b);
  }

  internal static string GetIntBinaryString(int n)
  {
    char[] chArray = new char[32 /*0x20*/];
    int index1 = 31 /*0x1F*/;
    for (int index2 = 0; index2 < 32 /*0x20*/; ++index2)
    {
      chArray[index1] = (n & 1 << index2) == 0 ? '0' : '1';
      --index1;
    }
    return new string(chArray);
  }

  internal static string GetInt16BinaryString(short n)
  {
    char[] chArray = new char[16 /*0x10*/];
    int index1 = 15;
    for (int index2 = 0; index2 < 16 /*0x10*/; ++index2)
    {
      chArray[index1] = ((int) n & 1 << index2) == 0 ? '0' : '1';
      --index1;
    }
    return new string(chArray);
  }
}
