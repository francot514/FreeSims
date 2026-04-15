// Decompiled with JetBrains decompiler
// Type: GonzoNet.NetworkClient
// Assembly: GonzoNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 75AA73F1-2E7B-40B2-B711-B42047463A5A
// Assembly location: C:\Users\Santiago\Documents\Visual Studio 2022\Projects\FreeSims\SimsVille\Dependencies\GonzoNet.dll

using GonzoNet.Encryption;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

#nullable disable
namespace GonzoNet;

public class NetworkClient
{
  protected Listener m_Listener;
  private Socket m_Sock;
  private string m_IP;
  private int m_Port;
  private bool m_Connected = false;
  private PacketStream m_TempPacket;
  private List<byte> m_HeaderBuild;
  private byte[] m_RecvBuf;
  private EncryptionMode m_EMode;
  private Encryptor m_ClientEncryptor;
  protected LoginArgsContainer m_LoginArgs;
  private Queue<KeyValuePair<ProcessedPacket, PacketHandler>> packetQueue = new Queue<KeyValuePair<ProcessedPacket, PacketHandler>>();

  public Encryptor ClientEncryptor
  {
    get => this.m_ClientEncryptor;
    set => this.m_ClientEncryptor = value;
  }

  public event NetworkErrorDelegate OnNetworkError;

  public event ReceivedPacketDelegate OnReceivedData;

  public event OnConnectedDelegate OnConnected;

  public event DisconnectedDelegate OnDisconnect;

  public NetworkClient(string IP, int Port, EncryptionMode EMode, bool KeepAlive)
  {
    this.m_Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    if (KeepAlive)
      this.m_Sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
    this.m_IP = IP;
    this.m_Port = Port;
    this.m_EMode = EMode;
    this.m_RecvBuf = new byte[11024];
  }

  public NetworkClient(Socket ClientSocket, Listener Server, EncryptionMode EMode)
  {
    this.m_Sock = ClientSocket;
    this.m_Listener = Server;
    this.m_RecvBuf = new byte[11024];
    this.m_EMode = EMode;
    this.m_Sock.BeginReceive(this.m_RecvBuf, 0, this.m_RecvBuf.Length, SocketFlags.None, new AsyncCallback(this.ReceiveCallback), (object) this.m_Sock);
  }

  public void Connect(LoginArgsContainer LoginArgs)
  {
    this.m_LoginArgs = LoginArgs;
    if (LoginArgs != null)
    {
      this.m_ClientEncryptor = LoginArgs.Enc;
      this.m_ClientEncryptor.Username = LoginArgs.Username;
    }
    if (this.m_Sock.Connected)
      return;
    IPAddress address1 = (IPAddress) null;
    try
    {
      foreach (IPAddress address2 in Dns.GetHostEntry(this.m_IP).AddressList)
      {
        if (address2.AddressFamily == AddressFamily.InterNetwork)
          address1 = address2;
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error trying to get local address {0} ", (object) ex.Message);
    }
    if (address1 == null)
    {
      try
      {
        address1 = IPAddress.Parse(this.m_IP);
      }
      catch
      {
        return;
      }
    }
    this.m_Sock.BeginConnect(address1, this.m_Port, new AsyncCallback(this.ConnectCallback), (object) this.m_Sock);
  }

  public bool Connected => this.m_Sock.Connected;

  public void Send(byte[] Data)
  {
    if (!this.m_Sock.Connected)
      return;
    try
    {
      this.m_Sock.BeginSend(Data, 0, Data.Length, SocketFlags.None, new AsyncCallback(this.OnSend), (object) this.m_Sock);
    }
    catch (SocketException ex)
    {
      this.Disconnect();
    }
  }

  public void SendEncrypted(byte PacketID, byte[] Data)
  {
    if (!this.m_Connected)
      return;
    byte[] buffer;
    lock (this.m_ClientEncryptor)
      buffer = this.m_ClientEncryptor.FinalizePacket(PacketID, Data);
    try
    {
      this.m_Sock.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(this.OnSend), (object) this.m_Sock);
    }
    catch (SocketException ex)
    {
      this.Disconnect();
    }
  }

  protected virtual void OnSend(IAsyncResult AR)
  {
    Socket asyncState = (Socket) AR.AsyncState;
    try
    {
      asyncState.EndSend(AR);
    }
    catch
    {
      this.Disconnect();
    }
  }

  private void BeginReceive()
  {
    this.m_Sock.BeginReceive(this.m_RecvBuf, 0, this.m_RecvBuf.Length, SocketFlags.None, new AsyncCallback(this.ReceiveCallback), (object) this.m_Sock);
  }

  private void ConnectCallback(IAsyncResult AR)
  {
    try
    {
      ((Socket) AR.AsyncState).EndConnect(AR);
      this.m_Connected = true;
      this.BeginReceive();
      if (this.OnConnected == null)
        return;
      this.OnConnected(this.m_LoginArgs);
    }
    catch (SocketException ex)
    {
      if (this.OnDisconnect != null)
        this.OnDisconnect();
      if (this.OnNetworkError == null)
        return;
      this.OnNetworkError(ex);
    }
  }

  public void ProcessPackets()
  {
    lock (this.packetQueue)
    {
      while (this.packetQueue.Count > 0)
      {
        KeyValuePair<ProcessedPacket, PacketHandler> keyValuePair = this.packetQueue.Dequeue();
        keyValuePair.Value.Handler(this, keyValuePair.Key);
      }
    }
  }

  public Queue<ProcessedPacket> GetPackets()
  {
    Queue<ProcessedPacket> packets = new Queue<ProcessedPacket>();
    lock (this.packetQueue)
    {
      while (this.packetQueue.Count > 0)
        packets.Enqueue(this.packetQueue.Dequeue().Key);
    }
    return packets;
  }

  private void OnPacket(ProcessedPacket packet, PacketHandler handler)
  {
    if (this.OnReceivedData != null)
      this.OnReceivedData((PacketStream) packet);
    this.packetQueue.Enqueue(new KeyValuePair<ProcessedPacket, PacketHandler>(packet, handler));
  }

  protected virtual void ReceiveCallback(IAsyncResult AR)
  {
    Socket asyncState = (Socket) AR.AsyncState;
    int count;
    try
    {
      count = asyncState.EndReceive(AR);
    }
    catch (SocketException ex)
    {
      this.Disconnect();
      return;
    }
    if (count == 0)
    {
      this.Disconnect();
    }
    else
    {
      byte[] numArray1 = new byte[count];
      Buffer.BlockCopy((Array) this.m_RecvBuf, 0, (Array) numArray1, 0, count);
      int srcOffset = 0;
      while (srcOffset < count)
      {
        if (this.m_TempPacket == null)
        {
          byte ID = numArray1[srcOffset++];
          PacketStream packetStream = new PacketStream(ID, 0);
          packetStream.WriteByte(ID);
          this.m_HeaderBuild = new List<byte>();
          while (srcOffset < count && this.m_HeaderBuild.Count < 4)
            this.m_HeaderBuild.Add(numArray1[srcOffset++]);
          if (this.m_HeaderBuild.Count < 4)
          {
            this.m_TempPacket = packetStream;
          }
          else
          {
            int int32 = BitConverter.ToInt32(this.m_HeaderBuild.ToArray(), 0);
            packetStream.SetLength((long) int32);
            packetStream.WriteInt32(int32);
            this.m_HeaderBuild = (List<byte>) null;
            byte[] numArray2 = new byte[Math.Min((long) (count - srcOffset), packetStream.Length - packetStream.BufferLength)];
            Buffer.BlockCopy((Array) numArray1, srcOffset, (Array) numArray2, 0, numArray2.Length);
            srcOffset += numArray2.Length;
            packetStream.WriteBytes(numArray2);
            this.m_TempPacket = packetStream;
          }
        }
        else
        {
          if (this.m_HeaderBuild != null)
          {
            while (this.m_HeaderBuild.Count < 4)
              this.m_HeaderBuild.Add(numArray1[srcOffset++]);
            int int32 = BitConverter.ToInt32(this.m_HeaderBuild.ToArray(), 0);
            this.m_TempPacket.SetLength((long) int32);
            this.m_TempPacket.WriteInt32(int32);
            this.m_HeaderBuild = (List<byte>) null;
          }
          byte[] numArray3 = new byte[Math.Min((long) (count - srcOffset), this.m_TempPacket.Length - this.m_TempPacket.BufferLength)];
          Buffer.BlockCopy((Array) numArray1, srcOffset, (Array) numArray3, 0, numArray3.Length);
          srcOffset += numArray3.Length;
          this.m_TempPacket.WriteBytes(numArray3);
        }
        if (this.m_TempPacket != null && this.m_TempPacket.BufferLength == this.m_TempPacket.Length)
        {
          PacketHandler packetHandler = this.FindPacketHandler(this.m_TempPacket.PacketID);
          if (packetHandler != null)
            this.OnPacket(new ProcessedPacket(this.m_TempPacket.PacketID, packetHandler.Encrypted, packetHandler.VariableLength, (int) this.m_TempPacket.Length, this.m_ClientEncryptor, this.m_TempPacket.ToArray()), packetHandler);
          this.m_TempPacket = (PacketStream) null;
        }
      }
      try
      {
        this.m_Sock.BeginReceive(this.m_RecvBuf, 0, this.m_RecvBuf.Length, SocketFlags.None, new AsyncCallback(this.ReceiveCallback), (object) this.m_Sock);
      }
      catch (SocketException ex)
      {
        this.Disconnect();
      }
    }
  }

  public string RemoteIP => ((IPEndPoint) this.m_Sock.RemoteEndPoint)?.Address.ToString();

  public int RemotePort
  {
    get
    {
      IPEndPoint remoteEndPoint = (IPEndPoint) this.m_Sock.RemoteEndPoint;
      return remoteEndPoint != null ? remoteEndPoint.Port : 0;
    }
  }

  public void Disconnect()
  {
    try
    {
      this.m_Sock.Shutdown(SocketShutdown.Both);
      this.m_Sock.DisconnectAsync(new SocketAsyncEventArgs());
      if (this.m_Listener != null)
        this.m_Listener.RemoveClient(this);
    }
    catch
    {
    }
    if (!this.m_Connected)
      return;
    if (this.OnDisconnect != null)
      this.OnDisconnect();
    this.m_Connected = false;
  }

  private PacketHandler FindPacketHandler(byte ID)
  {
    return PacketHandlers.Get(ID) ?? (PacketHandler) null;
  }
}
