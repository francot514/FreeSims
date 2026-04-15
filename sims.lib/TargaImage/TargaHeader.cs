// Decompiled with JetBrains decompiler
// Type: TargaImagePCL.TargaHeader
// Assembly: TargaImagePCL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EA3D1F96-8D99-4228-A7A5-0DA52D7AC9D1
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\TargaImagePCL.dll

using System.Collections.Generic;

#nullable disable
namespace TargaImagePCL;

public class TargaHeader
{
  private byte bImageIDLength;
  private ColorMapType eColorMapType;
  private ImageType eImageType;
  private short sColorMapFirstEntryIndex;
  private short sColorMapLength;
  private byte bColorMapEntrySize;
  private short sXOrigin;
  private short sYOrigin;
  private short sWidth;
  private short sHeight;
  private byte bPixelDepth;
  private byte bImageDescriptor;
  private VerticalTransferOrder eVerticalTransferOrder = VerticalTransferOrder.UNKNOWN;
  private HorizontalTransferOrder eHorizontalTransferOrder = HorizontalTransferOrder.UNKNOWN;
  private byte bAttributeBits;
  private string strImageIDValue = string.Empty;
  private List<Color> cColorMap = new List<Color>();

  public byte ImageIDLength => this.bImageIDLength;

  protected internal void SetImageIDLength(byte bImageIDLength)
  {
    this.bImageIDLength = bImageIDLength;
  }

  public ColorMapType ColorMapType => this.eColorMapType;

  protected internal void SetColorMapType(ColorMapType eColorMapType)
  {
    this.eColorMapType = eColorMapType;
  }

  public ImageType ImageType => this.eImageType;

  protected internal void SetImageType(ImageType eImageType) => this.eImageType = eImageType;

  public short ColorMapFirstEntryIndex => this.sColorMapFirstEntryIndex;

  protected internal void SetColorMapFirstEntryIndex(short sColorMapFirstEntryIndex)
  {
    this.sColorMapFirstEntryIndex = sColorMapFirstEntryIndex;
  }

  public short ColorMapLength => this.sColorMapLength;

  protected internal void SetColorMapLength(short sColorMapLength)
  {
    this.sColorMapLength = sColorMapLength;
  }

  public byte ColorMapEntrySize => this.bColorMapEntrySize;

  protected internal void SetColorMapEntrySize(byte bColorMapEntrySize)
  {
    this.bColorMapEntrySize = bColorMapEntrySize;
  }

  public short XOrigin => this.sXOrigin;

  protected internal void SetXOrigin(short sXOrigin) => this.sXOrigin = sXOrigin;

  public short YOrigin => this.sYOrigin;

  protected internal void SetYOrigin(short sYOrigin) => this.sYOrigin = sYOrigin;

  public short Width => this.sWidth;

  protected internal void SetWidth(short sWidth) => this.sWidth = sWidth;

  public short Height => this.sHeight;

  protected internal void SetHeight(short sHeight) => this.sHeight = sHeight;

  public byte PixelDepth => this.bPixelDepth;

  protected internal void SetPixelDepth(byte bPixelDepth) => this.bPixelDepth = bPixelDepth;

  protected internal byte ImageDescriptor
  {
    get => this.bImageDescriptor;
    set => this.bImageDescriptor = value;
  }

  public FirstPixelDestination FirstPixelDestination
  {
    get
    {
      if (this.eVerticalTransferOrder == VerticalTransferOrder.UNKNOWN || this.eHorizontalTransferOrder == HorizontalTransferOrder.UNKNOWN)
        return FirstPixelDestination.UNKNOWN;
      if (this.eVerticalTransferOrder == VerticalTransferOrder.BOTTOM && this.eHorizontalTransferOrder == HorizontalTransferOrder.LEFT)
        return FirstPixelDestination.BOTTOM_LEFT;
      if (this.eVerticalTransferOrder == VerticalTransferOrder.BOTTOM && this.eHorizontalTransferOrder == HorizontalTransferOrder.RIGHT)
        return FirstPixelDestination.BOTTOM_RIGHT;
      return this.eVerticalTransferOrder == VerticalTransferOrder.TOP && this.eHorizontalTransferOrder == HorizontalTransferOrder.LEFT ? FirstPixelDestination.TOP_LEFT : FirstPixelDestination.TOP_RIGHT;
    }
  }

  public VerticalTransferOrder VerticalTransferOrder => this.eVerticalTransferOrder;

  protected internal void SetVerticalTransferOrder(VerticalTransferOrder eVerticalTransferOrder)
  {
    this.eVerticalTransferOrder = eVerticalTransferOrder;
  }

  public HorizontalTransferOrder HorizontalTransferOrder => this.eHorizontalTransferOrder;

  protected internal void SetHorizontalTransferOrder(
    HorizontalTransferOrder eHorizontalTransferOrder)
  {
    this.eHorizontalTransferOrder = eHorizontalTransferOrder;
  }

  public byte AttributeBits => this.bAttributeBits;

  protected internal void SetAttributeBits(byte bAttributeBits)
  {
    this.bAttributeBits = bAttributeBits;
  }

  public string ImageIDValue => this.strImageIDValue;

  protected internal void SetImageIDValue(string strImageIDValue)
  {
    this.strImageIDValue = strImageIDValue;
  }

  public List<Color> ColorMap => this.cColorMap;

  public int ImageDataOffset
  {
    get
    {
      int num1 = 18 + (int) this.bImageIDLength;
      int num2 = 0;
      switch (this.bColorMapEntrySize)
      {
        case 15:
          num2 = 2;
          break;
        case 16 /*0x10*/:
          num2 = 2;
          break;
        case 24:
          num2 = 3;
          break;
        case 32 /*0x20*/:
          num2 = 4;
          break;
      }
      return num1 + (int) this.sColorMapLength * num2;
    }
  }

  public int BytesPerPixel => (int) this.bPixelDepth / 8;
}
