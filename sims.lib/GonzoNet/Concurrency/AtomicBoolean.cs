// Decompiled with JetBrains decompiler
// Type: GonzoNet.Concurrency.AtomicBoolean
// Assembly: GonzoNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 75AA73F1-2E7B-40B2-B711-B42047463A5A
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GonzoNet.dll

using System.Threading;

#nullable disable
namespace GonzoNet.Concurrency;

internal struct AtomicBoolean
{
  private const int UnSet = 0;
  private const int Set = 1;
  private int flag;

  public bool CompareAndExchange(bool expected, bool newVal)
  {
    int num = newVal ? 1 : 0;
    int comparand = expected ? 1 : 0;
    return Interlocked.CompareExchange(ref this.flag, num, comparand) == comparand;
  }

  public static AtomicBoolean FromValue(bool value)
  {
    return new AtomicBoolean() { Value = value };
  }

  public bool Exchange(bool newVal) => Interlocked.Exchange(ref this.flag, newVal ? 1 : 0) == 1;

  public bool Value
  {
    get => this.flag == 1;
    set => this.Exchange(value);
  }

  public bool Equals(AtomicBoolean rhs) => this.flag == rhs.flag;

  public override bool Equals(object rhs) => rhs is AtomicBoolean rhs1 && this.Equals(rhs1);

  public override int GetHashCode() => this.flag.GetHashCode();

  public static explicit operator bool(AtomicBoolean rhs) => rhs.Value;

  public static implicit operator AtomicBoolean(bool rhs) => AtomicBoolean.FromValue(rhs);
}
