// Decompiled with JetBrains decompiler
// Type: TargaImagePCL.RawTGABitmap
// Assembly: TargaImagePCL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EA3D1F96-8D99-4228-A7A5-0DA52D7AC9D1
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\TargaImagePCL.dll

using System;

#nullable disable
namespace TargaImagePCL;

public class RawTGABitmap
{
  public int Width;
  public int Height;
  public byte[] Data;
  public TGAPixelFormat Format;

  public RawTGABitmap(int width, int height, byte[] data, TGAPixelFormat format)
  {
    this.Width = width;
    this.Height = height;
    this.Data = data;
    this.Format = format;
  }

  public byte[] ToBGRA(bool premultiply)
  {
    byte[] bgra = (byte[]) null;
    if (this.Format == TGAPixelFormat.RGB_32bpp || this.Format == TGAPixelFormat.ARGB_32bpp)
    {
      bool flag = this.Format == TGAPixelFormat.ARGB_32bpp;
      bgra = new byte[this.Data.Length];
      for (int index = 0; index < this.Data.Length; index += 4)
      {
        bgra[index + 3] = flag ? this.Data[index + 3] : byte.MaxValue;
        float num = premultiply ? (float) this.Data[index + 3] / (float) byte.MaxValue : 1f;
        bgra[index + 2] = (byte) ((double) this.Data[index] * (double) num);
        bgra[index + 1] = (byte) ((double) this.Data[index + 1] * (double) num);
        bgra[index] = (byte) ((double) this.Data[index + 2] * (double) num);
      }
    }
    else if (this.Format == TGAPixelFormat.RGB_24bpp)
    {
      bgra = new byte[this.Width * this.Height * 4];
      int index1 = 0;
      for (int index2 = 0; index2 < this.Data.Length; index2 += 3)
      {
        byte maxValue = this.Data[index2] <= (byte) 253 || this.Data[index2 + 1] >= (byte) 3 || this.Data[index2 + 2] <= (byte) 253 ? byte.MaxValue : (byte) 0;
        bgra[index1 + 3] = maxValue;
        bgra[index1 + 2] = (byte) ((uint) this.Data[index2] & (uint) maxValue);
        bgra[index1 + 1] = (byte) ((uint) this.Data[index2 + 1] & (uint) maxValue);
        bgra[index1] = (byte) ((uint) this.Data[index2 + 2] & (uint) maxValue);
        index1 += 4;
      }
    }
    else
    {
      if (this.Format == TGAPixelFormat.ARGB1555_16bpp || this.Format == TGAPixelFormat.RGB555_16bpp)
      {
        int format = (int) this.Format;
        byte[] numArray = new byte[this.Width * this.Height * 4];
        throw new NotImplementedException("16-bit TGA not yet implemented.");
      }
      if (this.Format == TGAPixelFormat.Grayscale_8bpp)
      {
        bgra = new byte[this.Width * this.Height * 4];
        for (int index = 0; index < this.Data.Length; ++index)
        {
          byte num = this.Data[index];
          bgra[index + 3] = byte.MaxValue;
          bgra[index + 2] = num;
          bgra[index + 1] = num;
          bgra[index] = num;
        }
      }
    }
    return bgra;
  }
}
