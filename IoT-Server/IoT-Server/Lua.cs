using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NLua;
using NLua.Exceptions;

namespace IoT_Server
{
    public class Lua
    {
        public NLua.Lua env;
        private string luaPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/lua/";
        FileSystemWatcher watcher;
        public Lua()
        {
            try
            {
                env = new NLua.Lua();
                env.State.Encoding = Encoding.UTF8;

                env["_msg"] = new Action<string>((message) => _msg(message));
                env["_msgc"] = new Action<string, string>((message, color) => _msg(message, color));
                env["ioNET"] = new Modules.IO();
                env["socket"] = new Modules.LuaSocket(env);
                env["http"] = new Modules.LuaHttp(env);
                env["crypto"] = new Modules.Crypto();
                env["timer"] = Modules.Timer.singleton;
                LuaTable base64 = CreateTable();
                base64["Encode"] = new Func<string, string>((data) => Convert.ToBase64String(Encoding.UTF8.GetBytes(data)));
                base64["Decode"] = new Func<string, string>((data) => Encoding.UTF8.GetString(Convert.FromBase64String(data)));
                env["base64"] = base64;
                env["include"] = new Action<string>((path) => LuaRunFile(luaPath + path, path));

                if (Directory.Exists(luaPath + "autorun"))
                {
                    if (Directory.Exists(luaPath + "autorun/lib"))
                    {
                        foreach (string file in Directory.GetFiles(luaPath + "autorun/lib"))
                        {
                            LuaRunFile(luaPath + "autorun/lib/" + Path.GetFileName(file), "autorun/lib" + Path.GetFileName(file));
                        }
                    }
                    foreach (string file in Directory.GetFiles(luaPath + "autorun"))
                    {
                        LuaRunFile(luaPath + "autorun/" + Path.GetFileName(file), "autorun/" + Path.GetFileName(file));
                    }
                }
                foreach (string dir in Directory.GetDirectories(luaPath))
                {
                    string path = luaPath + Path.GetFileName(dir) + "/init.lua";
                    if (File.Exists(path))
                    {
                        LuaRunFile(luaPath + Path.GetFileName(dir) + "/init.lua", Path.GetFileName(dir) + "/init.lua");
                    }
                }
            }
            catch (LuaScriptException ex)
            {
                HookCall("LuaErrorHandler", ex.Message, ex.Source);
            }

            watcher = new FileSystemWatcher(luaPath, "*.lua");
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Changed += Watcher_Changed;
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

        }

        private void Watcher_Changed(object sender, FileSystemEventArgs ev)
        {
            try
            {
                _ = Execute(new Action(() => HookCall("OnFileChanged", ev.Name, ev.FullPath)), 100);
                watcher.EnableRaisingEvents = false;
            }
            finally
            {
                _ = Execute(new Action(() => watcher.EnableRaisingEvents = true), 1000);
            }
        }

        private void _msg (string message)
        {
            Console.WriteLine(message);
        }

        private void _msg(string message, string color)
        {
            switch (color)
            {
                case "red":
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case "green":
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case "blue":
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case "cyan":
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
            }
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public async System.Threading.Tasks.Task Execute(Action action, int tms)
        {
            await System.Threading.Tasks.Task.Delay(tms);
            action();
        }

        public void LuaRunFile(string file, string chunkName = "default")
        {
            try
            {
                string s = File.ReadAllText(file);
                env.DoString(Encoding.UTF8.GetBytes(s), chunkName != "default" ? chunkName : file.Substring(Math.Max(0, file.Length - 40)));
            }
            catch (LuaScriptException e)
            {
                HookCall("LuaErrorHandler", e.Message, chunkName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
            }
        }

        public void LuaRun(string lua, string chunkName="chunk")
        {
            try
            {
                env.DoString(Encoding.UTF8.GetBytes(lua), chunkName);
            }
            catch (LuaScriptException e)
            {
                HookCall("LuaErrorHandler", e.Message, chunkName);
            }
        }

        public void HookCall(params object[] args)
        {
            (env["hook.Call"] as LuaFunction).Call(args);
        }

        public LuaTable CreateTable()
        {
            return env.DoString(@"return {}", "chunk")[0] as LuaTable;
        }
    }
}
