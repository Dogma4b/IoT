using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Linq;
using NLua;

namespace IoT_Server.Modules.Socket
{
    class iServer
    {
        public bool IsActive { private set; get; }
        internal LuaTable luaTable;
        public NLua.Lua env;

        ConcurrentDictionary<int,iConnection> clients = new ConcurrentDictionary<int,iConnection>();

        TcpListener server;
        Thread connect;
        int ttcount = 0;

        public void Send(iLuaPacket packet, params iConnection[] whitelist)
        {
            MemoryStream memoryStream = packet.stream.baseStream as MemoryStream;
            packet.data = memoryStream.ToArray();

            foreach (var cli in whitelist)
            {
                cli.SendPrepared(packet);
            }
        }
        public void Broadcast(iLuaPacket packet, params iConnection[] blacklist)
        {
            MemoryStream memoryStream = packet.stream.baseStream as MemoryStream;
            packet.data = memoryStream.ToArray();

            if (blacklist != null && blacklist.Length > 0)
            {
                foreach (var cli in clients)
                {
                    if (!blacklist.Contains(cli.Value))
                    {
                        cli.Value.SendPrepared(packet);
                    }
                }
            }
            else
            {
                foreach (var cli in clients)
                {
                    cli.Value.SendPrepared(packet);
                }
            }
        }
        public void ReCast(iLuaPacket packet)
        {
            foreach (var cli in clients)
            {
                if (cli.Value != packet.sender)
                {
                    cli.Value.SendPrepared(packet);
                }
            }
        }

        [Obsolete]
        public void Start(int port)
        {
            IsActive = true;

            server = new TcpListener(port);
            server.Start();


            connect = new Thread(connect_thread);
            connect.Start();

            LuaFunction OnStart = luaTable["OnStart"] as LuaFunction;
            if (OnStart != null)
                OnStart.Call(luaTable);
        }
        public void Stop()
        {
            IsActive = false;
            foreach (var cli in clients)
            {
                cli.Value.Close();
            }

            LuaFunction OnStop = luaTable["OnStop"] as LuaFunction;
            if (OnStop != null)
                OnStop.Call(luaTable);
        }

        void connect_thread()
        {
            while (IsActive)
            {
                var cli = server.AcceptTcpClient();
                iConnection conn = new iConnection();
                conn.ServerConnect(cli);
                conn.onClose += conn_onClose;

                ttcount++;
                int id = ttcount;
                conn.id = id;
                if(clients.TryAdd(id, conn))
                {
                    try
                    {
                        LuaFunction OnConnect = luaTable["OnConnect"] as LuaFunction;
                        if (OnConnect != null)
                        {
                            env["tmpClientSocket"] = conn;
                            conn.luaTable = env.DoString(@"local t = {_object = tmpClientSocket} setmetatable(t, {__index=t._object}) tmpClientSocket = nil return t", "chunk")[0] as NLua.LuaTable;
                            OnConnect.Call(luaTable, conn.luaTable);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                //clients.Add(conn);

                    // sConsole.WriteLine("Incoming Client connection id: " + id);

                    //OnClientConnect.InvokeDelayed(null, ETaskTargetThread.Window, TimeSpan.Zero, conn, id);

                Thread.Sleep(10);
            }
        }

        void conn_onClose(iConnection obj)
        {
            //clients.Remove(obj);
            clients.TryRemove(obj.id, out _);

            LuaFunction OnClose = obj.luaTable["OnClose"] as LuaFunction;
            if (OnClose != null)
                OnClose.Call(luaTable, obj.luaTable);

            //string name = obj.tag.get<string>((int)variable.name);

            //sConsole.WriteLine("Client: " + name + "[" + id + "] disconnected");
            //OnClientDisconnect.InvokeDelayed(null, ETaskTargetThread.Window, TimeSpan.Zero, obj, id);
        }

        public void ProcessIncomingPacket()
        {
            foreach (var cli in clients)
            {
                cli.Value.ProcessIncomingPacket();
            }
        }
    }
}
