// Decompiled with JetBrains decompiler
// Type: GonzoNet.Concurrency.SpinWait
// Assembly: GonzoNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 75AA73F1-2E7B-40B2-B711-B42047463A5A
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GonzoNet.dll

using System;
using System.Threading;

#nullable disable
namespace GonzoNet.Concurrency;

public struct SpinWait
{
  private const int step = 20;
  private static readonly bool isSingleCpu = Environment.ProcessorCount == 1;
  private int ntime;

  public void SpinOnce()
  {
    if (SpinWait.isSingleCpu)
      this.Yield();
    else if (Interlocked.Increment(ref this.ntime) % 20 == 0)
      this.Yield();
    else
      Thread.SpinWait(2 * (this.ntime + 1));
  }

  private void Yield() => Thread.Sleep(0);

  public void Reset() => this.ntime = 0;

  public bool NextSpinWillYield => SpinWait.isSingleCpu || this.ntime % 20 == 0;

  public int Count => this.ntime;
}
