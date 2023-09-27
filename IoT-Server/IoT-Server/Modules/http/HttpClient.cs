using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NLua;

namespace IoT_Server.Modules.http
{
    class HttpClient
    {
        internal LuaTable luaTable;

        public string Fetch(string url)
        {
            try
            {
                using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                {
                    using (HttpResponseMessage res = client.GetAsync(url).Result)
                    {
                        using (HttpContent content = res.Content)
                        {
                            return content.ReadAsStringAsync().Result;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //LuaFunction OnError = luaTable["OnError"] as LuaFunction;
                //if (OnError != null)
                    //OnError.Call(luaTable, e.Message);
            }

            return "null";
        }
    }
}
