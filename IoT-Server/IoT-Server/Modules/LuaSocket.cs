using System;
using System.Collections.Generic;
using System.Text;
using NLua;
using IoT_Server.Modules.Socket;
using IoT_Server.Modules.WebSocket;

namespace IoT_Server.Modules
{
    public class LuaSocket
    {
        public NLua.Lua env;

        public LuaSocket(NLua.Lua env)
        {
            this.env = env;
        }

        public iLuaPacket Packet(uint id)
        {
            iLuaPacket packet = new iLuaPacket(id);

            return packet;
        }

        public LuaTable Client()
        {
            iConnection client = new iConnection();

            System.Threading.Thread updateReceive = new System.Threading.Thread(() =>
            {
                while (true)
                {
                    client.ProcessIncomingPacket();
                    System.Threading.Thread.Sleep(10);
                }
            });
            updateReceive.Start();

            env["tmpSocket"] = client;
            client.luaTable = env.DoString(@"local t = {_object = tmpSocket} setmetatable(t, {__index=t._object}) tmpSocket = nil return t", "chunk")[0] as NLua.LuaTable;

            return client.luaTable;
        }

        public LuaTable Server()
        {
            iServer server = new iServer();

            System.Threading.Thread updateReceive = new System.Threading.Thread(() =>
            {
                while (true)
                {
                    server.ProcessIncomingPacket();
                    System.Threading.Thread.Sleep(10);
                }
            });
            updateReceive.Start();

            env["tmpSocketServer"] = server;
            server.luaTable = env.DoString(@"local t = {_object = tmpSocketServer} setmetatable(t, {__index=t._object}) tmpSocketServer = nil return t", "chunk")[0] as NLua.LuaTable;
            server.env = env;

            return server.luaTable;
        }

        public LuaTable WSS()
        {
            WebSocketServer wsserver = new WebSocketServer();

            env["tmpWSSocketServer"] = wsserver;
            wsserver.luaTable = env.DoString(@"local t = {_object = tmpWSSocketServer} setmetatable(t, {__index=t._object}) tmpWSSocketServer = nil return t", "chunk")[0] as NLua.LuaTable;

            return wsserver.luaTable;
        }
    }
}
