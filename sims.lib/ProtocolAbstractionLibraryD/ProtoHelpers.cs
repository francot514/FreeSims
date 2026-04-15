// Decompiled with JetBrains decompiler
// Type: ProtocolAbstractionLibraryD.ProtoHelpers
// Assembly: GonzoNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 75AA73F1-2E7B-40B2-B711-B42047463A5A
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GonzoNet.dll

using System;
using System.Globalization;

#nullable disable
namespace ProtocolAbstractionLibraryD;

public class ProtoHelpers
{
  public static DateTime ParseDateTime(string DateTimeStr)
  {
    bool flag = false;
    if (DateTimeStr.Contains(" AM") || DateTimeStr.Contains(" PM"))
      flag = true;
    DateTime result;
    if (!flag)
    {
      if (!DateTime.TryParseExact(DateTimeStr, "yyyy/MM/dd hh:mm:ss", (IFormatProvider) CultureInfo.InvariantCulture, DateTimeStyles.None, out result) && !DateTime.TryParseExact(DateTimeStr, "yyyy.MM.dd hh:mm:ss", (IFormatProvider) CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
        DateTime.TryParseExact(DateTimeStr, "yyyy-MM-dd hh:mm:ss", (IFormatProvider) CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
    }
    else if (!DateTime.TryParseExact(DateTimeStr, "yyyy/MM/dd hh:mm:ss", (IFormatProvider) new CultureInfo("en-US"), DateTimeStyles.None, out result) && !DateTime.TryParseExact(DateTimeStr, "yyyy.MM.dd hh:mm:ss", (IFormatProvider) new CultureInfo("en-US"), DateTimeStyles.None, out result))
      DateTime.TryParseExact(DateTimeStr, "yyyy-MM-dd hh:mm:ss", (IFormatProvider) new CultureInfo("en-US"), DateTimeStyles.None, out result);
    return result;
  }

  public static void SetBit(ref byte aByte, int pos, bool value)
  {
    if (value)
      aByte |= (byte) (1 << pos);
    else
      aByte &= (byte) ~(1 << pos);
  }

  public static bool GetBit(byte aByte, int pos) => ((int) aByte & 1 << pos) != 0;
}
