using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using NLua;

namespace IoT_Server.Modules.Socket
{
    public class iConnection: IDisposable
    {
        public bool IsActive { private set; get; }
        internal LuaTable luaTable;
        public int id { set; get; }

        ConcurrentQueue<iLuaPacket> sendQueue = new ConcurrentQueue<iLuaPacket>();
        ConcurrentQueue<iLuaPacket> receiveQueue = new ConcurrentQueue<iLuaPacket>();

        Thread send, receive;

        TcpClient client;
        System.Net.Sockets.Socket socket;
        iStream stream;

        public event Action<iConnection> onClose;

        public bool Connect(string host, int port)
        {
            IsActive = true;
            client = new TcpClient();

            client.Connect(host, port);
            if (client.Connected)
            {
                socket = client.Client;
                stream = new iStream(client.GetStream());

                send = new Thread(SendThread);
                receive = new Thread(ReceiveThread);

                send.Start();
                receive.Start();

                SocketConnections.ConnectionAdd(this);

                return true;
            }

            return false;
        }

        public void ServerConnect(TcpClient client)
        {
            IsActive = true;
            this.client = client;
            if (client.Connected)
            {
                socket = client.Client;
                stream = new iStream(client.GetStream());

                send = new Thread(SendThread);
                receive = new Thread(ReceiveThread);

                send.Start();
                receive.Start();

                SocketConnections.ConnectionAdd(this);
            }
        }

        public void Close()
        {
            IsActive = false;

            if (socket != null && socket.Connected)
                socket.Close();
            if (client != null && client.Connected)
                client.Close();
            send = null;
            receive = null;

            onClose?.Invoke(this);

            sendQueue = new ConcurrentQueue<iLuaPacket>();
            receiveQueue = new ConcurrentQueue<iLuaPacket>();

            SocketConnections.ConnectionRemove(this);
        }

        public void Send(iLuaPacket packet)
        {
            if (IsActive)
            {
                MemoryStream memoryStream = packet.stream.baseStream as MemoryStream;
                packet.data = memoryStream.ToArray();
                sendQueue.Enqueue(packet);
            }
        }

        public void SendPrepared(iLuaPacket packet)
        {
            sendQueue.Enqueue(packet);
        }

        public void SendThread()
        {
            while (IsActive && socket.Connected)
            {
                if (sendQueue.Count > 0)
                {
                    for (int i = 0; i < 1000 && sendQueue.Count > 0; i++)
                    {
                        iLuaPacket packet;
                        if (sendQueue.TryDequeue(out packet))
                        {
                            try
                            {
                                stream.Write(packet.id);
                                stream.Write(packet.data.Length);
                                stream.WriteRaw(packet.data);
                            }
                            catch (Exception ex)
                            {
                                LuaFunction OnError = luaTable["OnError"] as LuaFunction;
                                if (OnError != null)
                                    OnError.Call(luaTable, ex.Message);
                                //Console.WriteLine("Error: " + ex.Message);
                                Close();
                            }
                        }
                    }
                }
                Thread.Sleep(1);
            }
        }

        byte[] ReadWait(Stream s, int len)
        {
            int total = 0;
            byte[] data = new byte[len];
            int timeout_limit = 100;
            while (total < len)
            {
                var recvc = s.Read(data, total, (int)len - total);
                total += recvc;
                if (recvc == 0)
                {
                    timeout_limit--;
                    if (timeout_limit <= 0)
                    {
                        Console.WriteLine("Read data timeout");
                        Close();
                    }
                }
            }
            return data;
        }

        public void ReceiveThread()
        {
            while (IsActive && socket.Connected)
            {
                try
                {
                    byte[] buffer = new byte[4];
                    for (int i = 0; i < 10; i++)
                    {
                        uint pid = BitConverter.ToUInt32(ReadWait(stream.baseStream, 4), 0);
                        uint len = BitConverter.ToUInt32(ReadWait(stream.baseStream, 4), 0);

                        byte[] data = ReadWait(stream.baseStream, (int)len);

                        iLuaPacket packet = new iLuaPacket(pid);
                        packet.data = data;
                        packet.sender = this;
                        receiveQueue.Enqueue(packet);
                    }
                }
                catch (Exception)
                {
                    //LuaFunction OnError = luaTable["OnError"] as LuaFunction;
                    //if (OnError != null)
                        //OnError.Call(luaTable, ex.Message);
                    //Console.WriteLine("Error: " + ex.Message);
                }
                Thread.Sleep(1);
            }
        }

        public void ProcessIncomingPacket()
        {
            if (IsActive && socket != null && socket.Connected)
            {
                if (receiveQueue.Count > 0)
                {
                    for (int i = 0; i < 200 && receiveQueue.Count > 0; i++)
                    {
                        iLuaPacket packet;
                        if (receiveQueue.TryDequeue(out packet))
                        {
                            try
                            {
                                MemoryStream memoryStream = new MemoryStream(packet.data);
                                iStream stream = new iStream(memoryStream);
                                packet.stream = stream;

                                if (luaTable != null)
                                {
                                    LuaFunction OnReceive = luaTable["OnReceive"] as LuaFunction;
                                    if (OnReceive != null)
                                        OnReceive.Call(luaTable, packet);
                                    else
                                        Console.WriteLine("Packet receive id: " + packet.id);
                                } else
                                {
                                    Console.WriteLine("Client: " + id + ". Lua table not found");
                                }

                                /*DynValue OnReceive = luaTable.Get("OnReceive");
                                if (OnReceive.Function != null)
                                    OnReceive.Function.Call(luaTable, packet);
                                else
                                    plugin.log.Info("qlay->socket", "Packet receive id: " + packet.id);*/
                            }
                            catch (System.AccessViolationException e)
                            {
                                Console.WriteLine(e.Message);
                            }
                            catch (Exception ex)
                            {
                                LuaFunction OnError = luaTable["OnError"] as LuaFunction;
                                if (OnError != null)
                                    OnError.Call(luaTable, ex.Message);
                                //Console.WriteLine("Error: " + ex.Message);
                                Close();
                            }
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            
        }
    }

    public static class SocketConnections
    {
        private static List<iConnection> connections = new List<iConnection>();

        public static void ConnectionAdd(iConnection connection)
        {
            connections.Add(connection);
        }

        public static void ConnectionRemove(iConnection connection)
        {
            connections.Remove(connection);
        }

        public static void UpdateConnectionsReceive()
        {
            foreach (iConnection connection in connections)
            {
                connection.ProcessIncomingPacket();
            }
        }
    }
}
