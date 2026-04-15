// Decompiled with JetBrains decompiler
// Type: TargaImagePCL.TargaImage
// Assembly: TargaImagePCL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EA3D1F96-8D99-4228-A7A5-0DA52D7AC9D1
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\TargaImagePCL.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#nullable disable
namespace TargaImagePCL;

public class TargaImage : IDisposable
{
  private TargaHeader objTargaHeader;
  private TargaExtensionArea objTargaExtensionArea;
  private TargaFooter objTargaFooter;
  private RawTGABitmap bmpTargaImage;
  private RawTGABitmap bmpImageThumbnail;
  private TGAFormat eTGAFormat;
  private string strFileName = string.Empty;
  private int intStride;
  private int intPadding;
  private List<List<byte>> rows = new List<List<byte>>();
  private List<byte> row = new List<byte>();
  private bool disposed;

  public TargaImage()
  {
    this.objTargaFooter = new TargaFooter();
    this.objTargaHeader = new TargaHeader();
    this.objTargaExtensionArea = new TargaExtensionArea();
    this.bmpTargaImage = (RawTGABitmap) null;
    this.bmpImageThumbnail = (RawTGABitmap) null;
  }

  public TargaHeader Header => this.objTargaHeader;

  public TargaExtensionArea ExtensionArea => this.objTargaExtensionArea;

  public TargaFooter Footer => this.objTargaFooter;

  public TGAFormat Format => this.eTGAFormat;

  public RawTGABitmap Image => this.bmpTargaImage;

  public RawTGABitmap Thumbnail => this.bmpImageThumbnail;

  public string FileName => this.strFileName;

  public int Stride => this.intStride;

  public int Padding => this.intPadding;

  ~TargaImage() => this.Dispose(false);

  public TargaImage(Stream filestream)
    : this()
  {
    if (filestream == null || filestream.Length <= 0L || !filestream.CanSeek)
      throw new Exception("Error loading file");
    BinaryReader binReader;
    using (binReader = new BinaryReader(filestream))
    {
      this.LoadTGAFooterInfo(binReader);
      this.LoadTGAHeaderInfo(binReader);
      this.LoadTGAExtensionArea(binReader);
      this.LoadTGAImage(binReader);
    }
  }

  private string GetStringFromBytes(byte[] dat)
  {
    return Encoding.UTF8.GetString(dat, 0, dat.Length).TrimEnd(new char[1]);
  }

  private void LoadTGAFooterInfo(BinaryReader binReader)
  {
    if (binReader != null && binReader.BaseStream != null && binReader.BaseStream.Length > 0L)
    {
      if (binReader.BaseStream.CanSeek)
      {
        try
        {
          binReader.BaseStream.Seek(-18L, SeekOrigin.End);
          string str = this.GetStringFromBytes(binReader.ReadBytes(16 /*0x10*/)).TrimEnd(new char[1]);
          if (string.Compare(str, "TRUEVISION-XFILE") == 0)
          {
            this.eTGAFormat = TGAFormat.NEW_TGA;
            binReader.BaseStream.Seek(-26L, SeekOrigin.End);
            int intExtensionAreaOffset = binReader.ReadInt32();
            int intDeveloperDirectoryOffset = binReader.ReadInt32();
            binReader.ReadBytes(16 /*0x10*/);
            string strReservedCharacter = this.GetStringFromBytes(binReader.ReadBytes(1)).TrimEnd(new char[1]);
            this.objTargaFooter.SetExtensionAreaOffset(intExtensionAreaOffset);
            this.objTargaFooter.SetDeveloperDirectoryOffset(intDeveloperDirectoryOffset);
            this.objTargaFooter.SetSignature(str);
            this.objTargaFooter.SetReservedCharacter(strReservedCharacter);
            return;
          }
          this.eTGAFormat = TGAFormat.ORIGINAL_TGA;
          return;
        }
        catch (Exception ex)
        {
          this.ClearAll();
          throw ex;
        }
      }
    }
    this.ClearAll();
    throw new Exception("Error loading file, could not read file from disk.");
  }

  private void LoadTGAHeaderInfo(BinaryReader binReader)
  {
    if (binReader != null && binReader.BaseStream != null && binReader.BaseStream.Length > 0L)
    {
      if (binReader.BaseStream.CanSeek)
      {
        try
        {
          binReader.BaseStream.Seek(0L, SeekOrigin.Begin);
          this.objTargaHeader.SetImageIDLength(binReader.ReadByte());
          this.objTargaHeader.SetColorMapType((ColorMapType) binReader.ReadByte());
          this.objTargaHeader.SetImageType((ImageType) binReader.ReadByte());
          this.objTargaHeader.SetColorMapFirstEntryIndex(binReader.ReadInt16());
          this.objTargaHeader.SetColorMapLength(binReader.ReadInt16());
          this.objTargaHeader.SetColorMapEntrySize(binReader.ReadByte());
          this.objTargaHeader.SetXOrigin(binReader.ReadInt16());
          this.objTargaHeader.SetYOrigin(binReader.ReadInt16());
          this.objTargaHeader.SetWidth(binReader.ReadInt16());
          this.objTargaHeader.SetHeight(binReader.ReadInt16());
          byte bPixelDepth = binReader.ReadByte();
          switch (bPixelDepth)
          {
            case 8:
            case 16 /*0x10*/:
            case 24:
            case 32 /*0x20*/:
              this.objTargaHeader.SetPixelDepth(bPixelDepth);
              byte b = binReader.ReadByte();
              this.objTargaHeader.SetAttributeBits((byte) Utilities.GetBits(b, 0, 4));
              this.objTargaHeader.SetVerticalTransferOrder((VerticalTransferOrder) Utilities.GetBits(b, 5, 1));
              this.objTargaHeader.SetHorizontalTransferOrder((HorizontalTransferOrder) Utilities.GetBits(b, 4, 1));
              if (this.objTargaHeader.ImageIDLength > (byte) 0)
              {
                this.objTargaHeader.SetImageIDValue(this.GetStringFromBytes(binReader.ReadBytes((int) this.objTargaHeader.ImageIDLength)).TrimEnd(new char[1]));
                break;
              }
              break;
            default:
              this.ClearAll();
              throw new Exception("Targa Image only supports 8, 16, 24, or 32 bit pixel depths.");
          }
        }
        catch (Exception ex)
        {
          this.ClearAll();
          throw ex;
        }
        if (this.objTargaHeader.ColorMapType == ColorMapType.COLOR_MAP_INCLUDED)
        {
          if (this.objTargaHeader.ImageType != ImageType.UNCOMPRESSED_COLOR_MAPPED && this.objTargaHeader.ImageType != ImageType.RUN_LENGTH_ENCODED_COLOR_MAPPED)
            return;
          if (this.objTargaHeader.ColorMapLength > (short) 0)
          {
            try
            {
              for (int index = 0; index < (int) this.objTargaHeader.ColorMapLength; ++index)
              {
                switch (this.objTargaHeader.ColorMapEntrySize)
                {
                  case 15:
                    byte[] numArray1 = binReader.ReadBytes(2);
                    this.objTargaHeader.ColorMap.Add(Utilities.GetColorFrom2Bytes(numArray1[1], numArray1[0]));
                    break;
                  case 16 /*0x10*/:
                    byte[] numArray2 = binReader.ReadBytes(2);
                    this.objTargaHeader.ColorMap.Add(Utilities.GetColorFrom2Bytes(numArray2[1], numArray2[0]));
                    break;
                  case 24:
                    int int32_1 = Convert.ToInt32(binReader.ReadByte());
                    int int32_2 = Convert.ToInt32(binReader.ReadByte());
                    this.objTargaHeader.ColorMap.Add(Color.FromArgb(Convert.ToInt32(binReader.ReadByte()), int32_2, int32_1));
                    break;
                  case 32 /*0x20*/:
                    int int32_3 = Convert.ToInt32(binReader.ReadByte());
                    int int32_4 = Convert.ToInt32(binReader.ReadByte());
                    int int32_5 = Convert.ToInt32(binReader.ReadByte());
                    int int32_6 = Convert.ToInt32(binReader.ReadByte());
                    this.objTargaHeader.ColorMap.Add(Color.FromArgb(int32_3, int32_6, int32_5, int32_4));
                    break;
                  default:
                    this.ClearAll();
                    throw new Exception("TargaImage only supports ColorMap Entry Sizes of 15, 16, 24 or 32 bits.");
                }
              }
              return;
            }
            catch (Exception ex)
            {
              this.ClearAll();
              throw ex;
            }
          }
          else
          {
            this.ClearAll();
            throw new Exception("Image Type requires a Color Map and Color Map Length is zero.");
          }
        }
        else
        {
          if (this.objTargaHeader.ImageType != ImageType.UNCOMPRESSED_COLOR_MAPPED && this.objTargaHeader.ImageType != ImageType.RUN_LENGTH_ENCODED_COLOR_MAPPED)
            return;
          this.ClearAll();
          throw new Exception("Image Type requires a Color Map and there was not a Color Map included in the file.");
        }
      }
    }
    this.ClearAll();
    throw new Exception("Error loading file, could not read file from disk.");
  }

  private void LoadTGAExtensionArea(BinaryReader binReader)
  {
    if (binReader != null && binReader.BaseStream != null && binReader.BaseStream.Length > 0L && binReader.BaseStream.CanSeek)
    {
      if (this.objTargaFooter.ExtensionAreaOffset <= 0)
        return;
      try
      {
        binReader.BaseStream.Seek((long) this.objTargaFooter.ExtensionAreaOffset, SeekOrigin.Begin);
        this.objTargaExtensionArea.SetExtensionSize((int) binReader.ReadInt16());
        this.objTargaExtensionArea.SetAuthorName(this.GetStringFromBytes(binReader.ReadBytes(41)).TrimEnd(new char[1]));
        this.objTargaExtensionArea.SetAuthorComments(this.GetStringFromBytes(binReader.ReadBytes(324)).TrimEnd(new char[1]));
        short num1 = binReader.ReadInt16();
        short num2 = binReader.ReadInt16();
        short num3 = binReader.ReadInt16();
        short num4 = binReader.ReadInt16();
        short num5 = binReader.ReadInt16();
        short num6 = binReader.ReadInt16();
        DateTime result;
        if (DateTime.TryParse($"{$"{num1.ToString()}/{num2.ToString()}/{num3.ToString()} "}{num4.ToString()}:{num5.ToString()}:{num6.ToString()}", out result))
          this.objTargaExtensionArea.SetDateTimeStamp(result);
        this.objTargaExtensionArea.SetJobName(this.GetStringFromBytes(binReader.ReadBytes(41)).TrimEnd(new char[1]));
        this.objTargaExtensionArea.SetJobTime(new TimeSpan((int) binReader.ReadInt16(), (int) binReader.ReadInt16(), (int) binReader.ReadInt16()));
        this.objTargaExtensionArea.SetSoftwareID(this.GetStringFromBytes(binReader.ReadBytes(41)).TrimEnd(new char[1]));
        float num7 = (float) binReader.ReadInt16() / 100f;
        string str = this.GetStringFromBytes(binReader.ReadBytes(1)).TrimEnd(new char[1]);
        this.objTargaExtensionArea.SetSoftwareID(num7.ToString("F2") + str);
        int a1 = (int) binReader.ReadByte();
        int r1 = (int) binReader.ReadByte();
        int b1 = (int) binReader.ReadByte();
        int g1 = (int) binReader.ReadByte();
        this.objTargaExtensionArea.SetKeyColor(Color.FromArgb(a1, r1, g1, b1));
        this.objTargaExtensionArea.SetPixelAspectRatioNumerator((int) binReader.ReadInt16());
        this.objTargaExtensionArea.SetPixelAspectRatioDenominator((int) binReader.ReadInt16());
        this.objTargaExtensionArea.SetGammaNumerator((int) binReader.ReadInt16());
        this.objTargaExtensionArea.SetGammaDenominator((int) binReader.ReadInt16());
        this.objTargaExtensionArea.SetColorCorrectionOffset(binReader.ReadInt32());
        this.objTargaExtensionArea.SetPostageStampOffset(binReader.ReadInt32());
        this.objTargaExtensionArea.SetScanLineOffset(binReader.ReadInt32());
        this.objTargaExtensionArea.SetAttributesType((int) binReader.ReadByte());
        if (this.objTargaExtensionArea.ScanLineOffset > 0)
        {
          binReader.BaseStream.Seek((long) this.objTargaExtensionArea.ScanLineOffset, SeekOrigin.Begin);
          for (int index = 0; index < (int) this.objTargaHeader.Height; ++index)
            this.objTargaExtensionArea.ScanLineTable.Add(binReader.ReadInt32());
        }
        if (this.objTargaExtensionArea.ColorCorrectionOffset <= 0)
          return;
        binReader.BaseStream.Seek((long) this.objTargaExtensionArea.ColorCorrectionOffset, SeekOrigin.Begin);
        for (int index = 0; index < 256 /*0x0100*/; ++index)
        {
          int a2 = (int) binReader.ReadInt16();
          int r2 = (int) binReader.ReadInt16();
          int b2 = (int) binReader.ReadInt16();
          int g2 = (int) binReader.ReadInt16();
          this.objTargaExtensionArea.ColorCorrectionTable.Add(Color.FromArgb(a2, r2, g2, b2));
        }
      }
      catch (Exception ex)
      {
        this.ClearAll();
        throw ex;
      }
    }
    else
    {
      this.ClearAll();
      throw new Exception("Error loading file, could not read file from disk.");
    }
  }

  private byte[] LoadImageBytes(BinaryReader binReader)
  {
    if (binReader != null && binReader.BaseStream != null && binReader.BaseStream.Length > 0L && binReader.BaseStream.CanSeek)
    {
      if (this.objTargaHeader.ImageDataOffset > 0)
      {
        byte[] buffer = new byte[this.intPadding];
        binReader.BaseStream.Seek((long) this.objTargaHeader.ImageDataOffset, SeekOrigin.Begin);
        int num1 = (int) this.objTargaHeader.Width * this.objTargaHeader.BytesPerPixel;
        int num2 = num1 * (int) this.objTargaHeader.Height;
        if (this.objTargaHeader.ImageType == ImageType.RUN_LENGTH_ENCODED_BLACK_AND_WHITE || this.objTargaHeader.ImageType == ImageType.RUN_LENGTH_ENCODED_COLOR_MAPPED || this.objTargaHeader.ImageType == ImageType.RUN_LENGTH_ENCODED_TRUE_COLOR)
        {
          int num3 = 0;
          int num4 = 0;
          while (num3 < num2)
          {
            int b = (int) binReader.ReadByte();
            int bits = Utilities.GetBits((byte) b, 7, 1);
            int num5 = Utilities.GetBits((byte) b, 0, 7) + 1;
            switch (bits)
            {
              case 0:
                int num6 = num5 * this.objTargaHeader.BytesPerPixel;
                for (int index = 0; index < num6; ++index)
                {
                  this.row.Add(binReader.ReadByte());
                  ++num3;
                  ++num4;
                  if (num4 == num1)
                  {
                    this.rows.Add(this.row);
                    this.row = new List<byte>();
                    num4 = 0;
                  }
                }
                continue;
              case 1:
                byte[] numArray = binReader.ReadBytes(this.objTargaHeader.BytesPerPixel);
                for (int index = 0; index < num5; ++index)
                {
                  foreach (byte num7 in numArray)
                    this.row.Add(num7);
                  num4 += numArray.Length;
                  num3 += numArray.Length;
                  if (num4 == num1)
                  {
                    this.rows.Add(this.row);
                    this.row = new List<byte>();
                    num4 = 0;
                  }
                }
                continue;
              default:
                continue;
            }
          }
        }
        else
        {
          for (int index1 = 0; index1 < (int) this.objTargaHeader.Height; ++index1)
          {
            for (int index2 = 0; index2 < num1; ++index2)
              this.row.Add(binReader.ReadByte());
            this.rows.Add(this.row);
            this.row = new List<byte>();
          }
        }
        bool flag1 = false;
        bool flag2 = false;
        switch (this.objTargaHeader.FirstPixelDestination)
        {
          case FirstPixelDestination.UNKNOWN:
          case FirstPixelDestination.BOTTOM_RIGHT:
            flag1 = true;
            flag2 = false;
            break;
          case FirstPixelDestination.TOP_LEFT:
            flag1 = false;
            flag2 = true;
            break;
          case FirstPixelDestination.TOP_RIGHT:
            flag1 = false;
            flag2 = false;
            break;
          case FirstPixelDestination.BOTTOM_LEFT:
            flag1 = true;
            flag2 = true;
            break;
        }
        try
        {
          MemoryStream memoryStream;
          using (memoryStream = new MemoryStream())
          {
            if (flag1)
              this.rows.Reverse();
            for (int index = 0; index < this.rows.Count; ++index)
            {
              if (flag2)
                this.rows[index].Reverse();
              byte[] array = this.rows[index].ToArray();
              memoryStream.Write(array, 0, array.Length);
              memoryStream.Write(buffer, 0, buffer.Length);
            }
            return memoryStream.ToArray();
          }
        }
        catch (Exception ex)
        {
          throw new Exception("something bad happened" + this.rows.Count.ToString());
        }
      }
      else
      {
        this.ClearAll();
        throw new Exception("Error loading file, No image data in file.");
      }
    }
    else
    {
      this.ClearAll();
      throw new Exception("Error loading file, could not read file from disk.");
    }
  }

  private void LoadTGAImage(BinaryReader binReader)
  {
    this.intStride = ((int) this.objTargaHeader.Width * (int) this.objTargaHeader.PixelDepth + 31 /*0x1F*/ & -32) >> 3;
    this.intPadding = this.intStride - ((int) this.objTargaHeader.Width * (int) this.objTargaHeader.PixelDepth + 7) / 8;
    byte[] data = this.LoadImageBytes(binReader);
    TGAPixelFormat pixelFormat = this.GetPixelFormat();
    this.bmpTargaImage = new RawTGABitmap((int) this.objTargaHeader.Width, (int) this.objTargaHeader.Height, data, pixelFormat);
    this.LoadThumbnail(binReader, pixelFormat);
  }

  private TGAPixelFormat GetPixelFormat()
  {
    TGAPixelFormat pixelFormat = TGAPixelFormat.Undefined;
    switch (this.objTargaHeader.PixelDepth)
    {
      case 8:
        pixelFormat = TGAPixelFormat.Grayscale_8bpp;
        break;
      case 16 /*0x10*/:
        if (this.Format == TGAFormat.NEW_TGA)
        {
          switch (this.objTargaExtensionArea.AttributesType)
          {
            case 0:
            case 1:
            case 2:
              pixelFormat = TGAPixelFormat.RGB555_16bpp;
              break;
            case 3:
              pixelFormat = TGAPixelFormat.ARGB1555_16bpp;
              break;
          }
        }
        else
        {
          pixelFormat = TGAPixelFormat.RGB555_16bpp;
          break;
        }
        break;
      case 24:
        pixelFormat = TGAPixelFormat.RGB_24bpp;
        break;
      case 32 /*0x20*/:
        if (this.Format == TGAFormat.NEW_TGA)
        {
          switch (this.objTargaExtensionArea.AttributesType)
          {
            case 0:
            case 3:
              pixelFormat = TGAPixelFormat.ARGB_32bpp;
              break;
            case 1:
            case 2:
              pixelFormat = TGAPixelFormat.RGB_32bpp;
              break;
            case 4:
              pixelFormat = TGAPixelFormat.ARGB_32bpp;
              break;
          }
        }
        else
        {
          pixelFormat = TGAPixelFormat.RGB_32bpp;
          break;
        }
        break;
    }
    return pixelFormat;
  }

  private void LoadThumbnail(BinaryReader binReader, TGAPixelFormat pfPixelFormat)
  {
    byte[] data = (byte[]) null;
    if (binReader == null || binReader.BaseStream == null || binReader.BaseStream.Length <= 0L || !binReader.BaseStream.CanSeek || this.ExtensionArea.PostageStampOffset <= 0)
      return;
    binReader.BaseStream.Seek((long) this.ExtensionArea.PostageStampOffset, SeekOrigin.Begin);
    int width = (int) binReader.ReadByte();
    int height = (int) binReader.ReadByte();
    int length = ((width * (int) this.objTargaHeader.PixelDepth + 31 /*0x1F*/ & -32) >> 3) - (width * (int) this.objTargaHeader.PixelDepth + 7) / 8;
    List<List<byte>> byteListList = new List<List<byte>>();
    List<byte> byteList = new List<byte>();
    byte[] buffer = new byte[length];
    bool flag1 = false;
    bool flag2 = false;
    MemoryStream memoryStream;
    using (memoryStream = new MemoryStream())
    {
      int num = width * ((int) this.objTargaHeader.PixelDepth / 8);
      for (int index1 = 0; index1 < height; ++index1)
      {
        for (int index2 = 0; index2 < num; ++index2)
          byteList.Add(binReader.ReadByte());
        byteListList.Add(byteList);
        byteList = new List<byte>();
      }
      switch (this.objTargaHeader.FirstPixelDestination)
      {
        case FirstPixelDestination.UNKNOWN:
        case FirstPixelDestination.BOTTOM_RIGHT:
          flag2 = true;
          flag1 = false;
          break;
        case FirstPixelDestination.TOP_RIGHT:
          flag2 = false;
          flag1 = false;
          break;
      }
      if (flag2)
        byteListList.Reverse();
      for (int index = 0; index < byteListList.Count; ++index)
      {
        if (flag1)
          byteListList[index].Reverse();
        byte[] array = byteListList[index].ToArray();
        memoryStream.Write(array, 0, array.Length);
        memoryStream.Write(buffer, 0, buffer.Length);
      }
      data = memoryStream.ToArray();
    }
    if (data == null || data.Length == 0)
      return;
    this.bmpImageThumbnail = new RawTGABitmap(width, height, data, pfPixelFormat);
  }

  private void ClearAll()
  {
    this.objTargaHeader = new TargaHeader();
    this.objTargaExtensionArea = new TargaExtensionArea();
    this.objTargaFooter = new TargaFooter();
    this.eTGAFormat = TGAFormat.UNKNOWN;
    this.intStride = 0;
    this.intPadding = 0;
    this.rows.Clear();
    this.row.Clear();
    this.strFileName = string.Empty;
  }

  public void Dispose()
  {
    this.Dispose(true);
    GC.SuppressFinalize((object) this);
  }

  protected virtual void Dispose(bool disposing)
  {
    int num = this.disposed ? 1 : 0;
    this.disposed = true;
  }
}
