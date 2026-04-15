// Decompiled with JetBrains decompiler
// Type: GonzoNet.Listener
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

public class Listener
{
  public List<NetworkClient> Clients;
  private Socket m_ListenerSock;
  private IPEndPoint m_LocalEP;
  private EncryptionMode m_EMode;

  public event OnDisconnectedDelegate OnDisconnected;

  public event OnDisconnectedDelegate OnConnected;

  public Listener(EncryptionMode Mode)
  {
    this.m_ListenerSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    this.m_ListenerSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
    this.Clients = new List<NetworkClient>();
    this.m_EMode = Mode;
  }

  public virtual void Initialize(IPEndPoint LocalEP)
  {
    this.m_LocalEP = LocalEP;
    try
    {
      this.m_ListenerSock.Bind((EndPoint) LocalEP);
      this.m_ListenerSock.Listen(10000);
    }
    catch (SocketException ex)
    {
      throw ex;
    }
    this.m_ListenerSock.BeginAccept(new AsyncCallback(this.OnAccept), (object) this.m_ListenerSock);
  }

  public NetworkClient GetClient(string RemoteIP, int RemotePort)
  {
    lock (this.Clients)
    {
      foreach (NetworkClient client in this.Clients)
      {
        if (RemoteIP.Equals(client.RemoteIP, StringComparison.CurrentCultureIgnoreCase) && RemotePort == client.RemotePort)
          return client;
      }
    }
    return (NetworkClient) null;
  }

  public virtual void OnAccept(IAsyncResult AR)
  {
    Socket ClientSocket = this.m_ListenerSock.EndAccept(AR);
    if (ClientSocket != null)
    {
      ClientSocket.LingerState = new LingerOption(true, 5);
      NetworkClient Client = new NetworkClient(ClientSocket, this, this.m_EMode);
      this.Clients.Add(Client);
      if (this.OnConnected != null)
        this.OnConnected(Client);
    }
    this.m_ListenerSock.BeginAccept(new AsyncCallback(this.OnAccept), (object) this.m_ListenerSock);
  }

  public virtual void RemoveClient(NetworkClient Client)
  {
    this.Clients.Remove(Client);
    if (this.OnDisconnected == null)
      return;
    this.OnDisconnected(Client);
  }

  public virtual void Close()
  {
    try
    {
      this.m_ListenerSock.Shutdown(SocketShutdown.Both);
      this.m_ListenerSock.Close();
    }
    catch
    {
    }
  }

  public int NumConnectedClients => this.Clients.Count;
}
