using NLua;
using System;
using System.Collections.Generic;
using System.Text;
using IoT_Server.Modules.http;

namespace IoT_Server.Modules
{
    class LuaHttp
    {
        public NLua.Lua env;

        public LuaHttp(NLua.Lua env)
        {
            this.env = env;
        }

        public LuaTable Client()
        {
            HttpClient client = new HttpClient();

            env["tmpHttpClient"] = client;
            client.luaTable = env.DoString(@"local t = {_object = tmpHttpClient} setmetatable(t, {__index=t._object}) tmpHttpClient = nil return t", "chunk")[0] as LuaTable;

            return client.luaTable;
        }

        public LuaTable Server()
        {
            HttpServer server = new HttpServer();

            env["tmpHttpServer"] = server;
            server.luaTable = env.DoString(@"local t = {_object = tmpHttpServer} setmetatable(t, {__index=t._object}) tmpHttpServer = nil return t", "chunk")[0] as LuaTable;

            return server.luaTable;
        }
    }
}
