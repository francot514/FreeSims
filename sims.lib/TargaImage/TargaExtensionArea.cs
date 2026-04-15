// Decompiled with JetBrains decompiler
// Type: TargaImagePCL.TargaExtensionArea
// Assembly: TargaImagePCL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EA3D1F96-8D99-4228-A7A5-0DA52D7AC9D1
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\TargaImagePCL.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace TargaImagePCL;

public class TargaExtensionArea
{
  private int intExtensionSize;
  private string strAuthorName = string.Empty;
  private string strAuthorComments = string.Empty;
  private DateTime dtDateTimeStamp = DateTime.Now;
  private string strJobName = string.Empty;
  private TimeSpan dtJobTime = TimeSpan.Zero;
  private string strSoftwareID = string.Empty;
  private string strSoftwareVersion = string.Empty;
  private Color cKeyColor = Color.Empty;
  private int intPixelAspectRatioNumerator;
  private int intPixelAspectRatioDenominator;
  private int intGammaNumerator;
  private int intGammaDenominator;
  private int intColorCorrectionOffset;
  private int intPostageStampOffset;
  private int intScanLineOffset;
  private int intAttributesType;
  private List<int> intScanLineTable = new List<int>();
  private List<Color> cColorCorrectionTable = new List<Color>();

  public int ExtensionSize => this.intExtensionSize;

  protected internal void SetExtensionSize(int intExtensionSize)
  {
    this.intExtensionSize = intExtensionSize;
  }

  public string AuthorName => this.strAuthorName;

  protected internal void SetAuthorName(string strAuthorName) => this.strAuthorName = strAuthorName;

  public string AuthorComments => this.strAuthorComments;

  protected internal void SetAuthorComments(string strAuthorComments)
  {
    this.strAuthorComments = strAuthorComments;
  }

  public DateTime DateTimeStamp => this.dtDateTimeStamp;

  protected internal void SetDateTimeStamp(DateTime dtDateTimeStamp)
  {
    this.dtDateTimeStamp = dtDateTimeStamp;
  }

  public string JobName => this.strJobName;

  protected internal void SetJobName(string strJobName) => this.strJobName = strJobName;

  public TimeSpan JobTime => this.dtJobTime;

  protected internal void SetJobTime(TimeSpan dtJobTime) => this.dtJobTime = dtJobTime;

  public string SoftwareID => this.strSoftwareID;

  protected internal void SetSoftwareID(string strSoftwareID) => this.strSoftwareID = strSoftwareID;

  public string SoftwareVersion => this.strSoftwareVersion;

  protected internal void SetSoftwareVersion(string strSoftwareVersion)
  {
    this.strSoftwareVersion = strSoftwareVersion;
  }

  public Color KeyColor => this.cKeyColor;

  protected internal void SetKeyColor(Color cKeyColor) => this.cKeyColor = cKeyColor;

  public int PixelAspectRatioNumerator => this.intPixelAspectRatioNumerator;

  protected internal void SetPixelAspectRatioNumerator(int intPixelAspectRatioNumerator)
  {
    this.intPixelAspectRatioNumerator = intPixelAspectRatioNumerator;
  }

  public int PixelAspectRatioDenominator => this.intPixelAspectRatioDenominator;

  protected internal void SetPixelAspectRatioDenominator(int intPixelAspectRatioDenominator)
  {
    this.intPixelAspectRatioDenominator = intPixelAspectRatioDenominator;
  }

  public float PixelAspectRatio
  {
    get
    {
      return this.intPixelAspectRatioDenominator > 0 ? (float) this.intPixelAspectRatioNumerator / (float) this.intPixelAspectRatioDenominator : 0.0f;
    }
  }

  public int GammaNumerator => this.intGammaNumerator;

  protected internal void SetGammaNumerator(int intGammaNumerator)
  {
    this.intGammaNumerator = intGammaNumerator;
  }

  public int GammaDenominator => this.intGammaDenominator;

  protected internal void SetGammaDenominator(int intGammaDenominator)
  {
    this.intGammaDenominator = intGammaDenominator;
  }

  public float GammaRatio
  {
    get
    {
      return this.intGammaDenominator > 0 ? (float) Math.Round((double) this.intGammaNumerator / (double) this.intGammaDenominator, 1) : 1f;
    }
  }

  public int ColorCorrectionOffset => this.intColorCorrectionOffset;

  protected internal void SetColorCorrectionOffset(int intColorCorrectionOffset)
  {
    this.intColorCorrectionOffset = intColorCorrectionOffset;
  }

  public int PostageStampOffset => this.intPostageStampOffset;

  protected internal void SetPostageStampOffset(int intPostageStampOffset)
  {
    this.intPostageStampOffset = intPostageStampOffset;
  }

  public int ScanLineOffset => this.intScanLineOffset;

  protected internal void SetScanLineOffset(int intScanLineOffset)
  {
    this.intScanLineOffset = intScanLineOffset;
  }

  public int AttributesType => this.intAttributesType;

  protected internal void SetAttributesType(int intAttributesType)
  {
    this.intAttributesType = intAttributesType;
  }

  public List<int> ScanLineTable => this.intScanLineTable;

  public List<Color> ColorCorrectionTable => this.cColorCorrectionTable;
}
