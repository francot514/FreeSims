// Decompiled with JetBrains decompiler
// Type: GonzoNet.PacketStream
// Assembly: GonzoNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 75AA73F1-2E7B-40B2-B711-B42047463A5A
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GonzoNet.dll

using GonzoNet.Exceptions;
using System;
using System.IO;
using System.Text;

#nullable disable
namespace GonzoNet;

public class PacketStream : Stream
{
  private byte m_ID;
  protected int m_Length;
  public bool m_VariableLength;
  protected MemoryStream m_BaseStream;
  private bool m_SupportsPeek = false;
  private byte[] m_PeekBuffer;
  protected BinaryReader m_Reader;
  private BinaryWriter m_Writer;
  private long m_Position;

  public PacketStream(byte ID, int Length, byte[] DataBuffer)
  {
    this.m_ID = ID;
    this.m_Length = Length;
    this.m_BaseStream = new MemoryStream(DataBuffer);
    this.m_BaseStream.Position = 0L;
    this.m_SupportsPeek = true;
    this.m_PeekBuffer = new byte[DataBuffer.Length];
    DataBuffer.CopyTo((Array) this.m_PeekBuffer, 0);
    this.m_Reader = new BinaryReader((Stream) this.m_BaseStream);
    this.m_Position = (long) (DataBuffer.Length - 1);
  }

  public PacketStream(byte ID, int Length)
  {
    this.m_ID = ID;
    this.m_Length = Length;
    this.m_SupportsPeek = false;
    this.m_BaseStream = new MemoryStream();
    this.m_Writer = new BinaryWriter((Stream) this.m_BaseStream);
    this.m_Position = 0L;
  }

  public override bool CanRead => true;

  public override bool CanWrite => true;

  public override bool CanSeek => false;

  public bool CanPeek => this.m_SupportsPeek;

  public byte PacketID => this.m_ID;

  public override long Position
  {
    get => this.m_Position;
    set => this.m_Position = value;
  }

  public override long Length => (long) this.m_Length;

  public long BufferLength => this.m_BaseStream != null ? this.m_BaseStream.Length : 0L;

  public override void SetLength(long value) => this.m_Length = (int) value;

  public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

  public override void Flush() => this.m_BaseStream.Flush();

  public byte[] ToArray()
  {
    byte[] array;
    lock (this.m_BaseStream)
      array = this.m_BaseStream.ToArray();
    if (this.m_VariableLength)
    {
      ushort position = (ushort) this.m_Position;
      array[2] = (byte) ((uint) position & (uint) byte.MaxValue);
      array[3] = (byte) ((uint) position >> 8);
    }
    return array;
  }

  public override int Read(byte[] buffer, int offset, int count)
  {
    int num = this.m_BaseStream.Read(buffer, offset, count);
    this.m_Position -= (long) num;
    return num;
  }

  public byte[] ReadBytes(int NumBytes)
  {
    byte[] buffer = new byte[NumBytes];
    this.Read(buffer, 0, NumBytes);
    return buffer;
  }

  public byte PeekByte()
  {
    if (this.m_SupportsPeek)
      return this.m_PeekBuffer[this.m_Position];
    throw new PeekNotSupportedException("Tried peeking from a PacketStream instance that didn't support it!");
  }

  public byte PeekByte(int Position)
  {
    if (this.m_SupportsPeek)
      return this.m_PeekBuffer[Position];
    throw new PeekNotSupportedException("Tried peeking from a PacketStream instance that didn't support it!");
  }

  public ushort PeekUShort(int Position)
  {
    MemoryStream output = new MemoryStream();
    BinaryWriter binaryWriter = new BinaryWriter((Stream) output);
    binaryWriter.Write(this.PeekByte(Position));
    binaryWriter.Write(this.PeekByte(Position + 1));
    binaryWriter.Flush();
    return BitConverter.ToUInt16(output.ToArray(), 0);
  }

  public int PeekInt(int Position)
  {
    MemoryStream output = new MemoryStream();
    BinaryWriter binaryWriter = new BinaryWriter((Stream) output);
    binaryWriter.Write(this.PeekByte(Position));
    binaryWriter.Write(this.PeekByte(Position + 1));
    binaryWriter.Write(this.PeekByte(Position + 2));
    binaryWriter.Write(this.PeekByte(Position + 3));
    binaryWriter.Flush();
    return BitConverter.ToInt32(output.ToArray(), 0);
  }

  public override int ReadByte()
  {
    --this.m_Position;
    return this.m_BaseStream.ReadByte();
  }

  public double ReadDouble()
  {
    this.m_Position -= 8L;
    return this.m_Reader.ReadDouble();
  }

  public ushort ReadUShort()
  {
    this.m_Position -= 2L;
    return this.ReadUInt16();
  }

  public string ReadString()
  {
    string str;
    try
    {
      str = this.m_Reader.ReadString();
      this.m_Position -= (long) str.Length;
    }
    catch (EndOfStreamException ex)
    {
      return string.Empty;
    }
    return str;
  }

  public string ReadString(int NumChars)
  {
    byte[] numArray = new byte[NumChars];
    this.m_Reader.Read(numArray, 0, NumChars);
    this.m_Position -= (long) NumChars;
    return Encoding.UTF8.GetString(numArray);
  }

  public int ReadInt32()
  {
    this.m_Position -= 4L;
    return this.m_Reader.ReadInt32();
  }

  public long ReadInt64()
  {
    this.m_Position -= 8L;
    return this.m_Reader.ReadInt64();
  }

  public ushort ReadUInt16()
  {
    this.m_Position -= 2L;
    return this.m_Reader.ReadUInt16();
  }

  public ulong ReadUInt64()
  {
    this.m_Position -= 8L;
    return this.m_Reader.ReadUInt64();
  }

  public override void Write(byte[] buffer, int offset, int count)
  {
    lock (this.m_BaseStream)
    {
      this.m_BaseStream.Write(buffer, offset, count);
      this.m_Position += (long) count;
      this.m_BaseStream.Flush();
    }
  }

  public void WriteDouble(double Value)
  {
    lock (this.m_Writer)
    {
      this.m_Writer.Write(Value);
      this.m_Position += 8L;
      this.m_Writer.Flush();
    }
  }

  public void WriteBytes(byte[] Buffer)
  {
    lock (this.m_BaseStream)
    {
      this.m_BaseStream.Write(Buffer, 0, Buffer.Length);
      this.m_Position += (long) Buffer.Length;
      this.m_BaseStream.Flush();
    }
  }

  public override void WriteByte(byte Value)
  {
    lock (this.m_Writer)
    {
      try
      {
        this.m_Writer.Write(Value);
        ++this.m_Position;
        this.m_Writer.Flush();
      }
      catch (IOException ex)
      {
        this.m_Writer.Write(Value);
        ++this.m_Position;
        this.m_Writer.Flush();
      }
    }
  }

  public void WriteInt32(int Value)
  {
    lock (this.m_Writer)
    {
      try
      {
        this.m_Writer.Write(Value);
        this.m_Position += 4L;
        this.m_Writer.Flush();
      }
      catch (IOException ex)
      {
        this.m_Writer.Write(Value);
        this.m_Position += 4L;
        this.m_Writer.Flush();
      }
    }
  }

  public void WriteUInt16(ushort Value)
  {
    lock (this.m_Writer)
    {
      try
      {
        this.m_Writer.Write(Value);
        this.m_Position += 2L;
        this.m_Writer.Flush();
      }
      catch (IOException ex)
      {
        this.m_Writer.Write(Value);
        this.m_Position += 2L;
        this.m_Writer.Flush();
      }
    }
  }

  public void WriteInt64(long Value)
  {
    lock (this.m_Writer)
    {
      try
      {
        this.m_Writer.Write(Value);
        this.m_Position += 8L;
        this.m_Writer.Flush();
      }
      catch (IOException ex)
      {
        this.m_Writer.Write(Value);
        this.m_Position += 8L;
        this.m_Writer.Flush();
      }
    }
  }

  public void WriteUInt64(ulong Value)
  {
    lock (this.m_Writer)
    {
      try
      {
        this.m_Writer.Write(Value);
        this.m_Position += 8L;
        this.m_Writer.Flush();
      }
      catch (IOException ex)
      {
        this.m_Writer.Write(Value);
        this.m_Position += 8L;
        this.m_Writer.Flush();
      }
    }
  }

  public void WriteString(string Str)
  {
    lock (this.m_Writer)
    {
      try
      {
        this.m_Writer.Write(Str);
        this.m_Position += (long) (Str.Length + 1);
        this.m_Writer.Flush();
      }
      catch (IOException ex)
      {
        this.m_Writer.Write(Str);
        this.m_Position += (long) (Str.Length + 1);
        this.m_Writer.Flush();
      }
    }
  }

  public void WriteHeader()
  {
    lock (this.m_Writer)
    {
      this.WriteByte(this.m_ID);
      ++this.m_Position;
    }
  }
}
