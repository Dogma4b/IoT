using NLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace IoT_Server.Modules.http
{
    class HttpServer
    {
        internal LuaTable luaTable;

        private static IDictionary<string, string> mimeTypes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
        #region MIME types
        {".asf", "video/x-ms-asf"},
        {".asx", "video/x-ms-asf"},
        {".avi", "video/x-msvideo"},
        {".bin", "application/octet-stream"},
        {".cco", "application/x-cocoa"},
        {".crt", "application/x-x509-ca-cert"},
        {".css", "text/css"},
        {".deb", "application/octet-stream"},
        {".der", "application/x-x509-ca-cert"},
        {".dll", "application/octet-stream"},
        {".dmg", "application/octet-stream"},
        {".ear", "application/java-archive"},
        {".eot", "application/octet-stream"},
        {".exe", "application/octet-stream"},
        {".flv", "video/x-flv"},
        {".gif", "image/gif"},
        {".hqx", "application/mac-binhex40"},
        {".htc", "text/x-component"},
        {".htm", "text/html"},
        {".html", "text/html"},
        {".ico", "image/x-icon"},
        {".img", "application/octet-stream"},
        {".iso", "application/octet-stream"},
        {".jar", "application/java-archive"},
        {".jardiff", "application/x-java-archive-diff"},
        {".jng", "image/x-jng"},
        {".jnlp", "application/x-java-jnlp-file"},
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".js", "application/x-javascript"},
        {".mml", "text/mathml"},
        {".mng", "video/x-mng"},
        {".mov", "video/quicktime"},
        {".mp3", "audio/mpeg"},
        {".mpeg", "video/mpeg"},
        {".mpg", "video/mpeg"},
        {".msi", "application/octet-stream"},
        {".msm", "application/octet-stream"},
        {".msp", "application/octet-stream"},
        {".pdb", "application/x-pilot"},
        {".pdf", "application/pdf"},
        {".pem", "application/x-x509-ca-cert"},
        {".pl", "application/x-perl"},
        {".pm", "application/x-perl"},
        {".png", "image/png"},
        {".prc", "application/x-pilot"},
        {".ra", "audio/x-realaudio"},
        {".rar", "application/x-rar-compressed"},
        {".rpm", "application/x-redhat-package-manager"},
        {".rss", "text/xml"},
        {".run", "application/x-makeself"},
        {".sea", "application/x-sea"},
        {".shtml", "text/html"},
        {".sit", "application/x-stuffit"},
        {".swf", "application/x-shockwave-flash"},
        {".tcl", "application/x-tcl"},
        {".tk", "application/x-tcl"},
        {".txt", "text/plain"},
        {".war", "application/java-archive"},
        {".wbmp", "image/vnd.wap.wbmp"},
        {".wmv", "video/x-ms-wmv"},
        {".xml", "text/xml"},
        {".xpi", "application/x-xpinstall"},
        {".zip", "application/zip"},
        #endregion
        };

        private Thread server;
        private string rootPath;
        private HttpListener listener;
        private string ip;
        private int port;

        private List<string> Statics = new List<string>();

        private Dictionary<string, byte[]> cache = new Dictionary<string, byte[]>();

        public void Start(string path, string ip, int port)
        {
            this.rootPath = path;
            this.ip = ip;
            this.port = port;

            foreach (string route in (luaTable["Statics"] as LuaTable).Values)
            {
                Statics.Add(route);
            }

            server = new Thread(Listener);
            server.Start();

            LuaFunction OnStart = luaTable["OnStart"] as LuaFunction;
            if (OnStart != null)
                OnStart.Call(luaTable);
        }

        private void Listener()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://" + ip + ":" + port.ToString() + "/");
            listener.Start();

            while(true)
            {
                try
                {
                    HttpListenerContext context = listener.GetContext();

                    string url = context.Request.Url.AbsolutePath.Substring(1).Trim(new char[] { ' ', '.', '*' });
                    string filePath = rootPath + "/" + url;

                    /*string url = context.Request.Url.AbsolutePath.Substring(1);

                    string[] staticContent = new string[] { "css", "js", "addons" };

                    if (string.IsNullOrEmpty(url) || !Array.Exists(staticContent, folder => url.StartsWith(folder)))
                    {
                        string indexPath = Path.Combine(rootPath, "index.html");

                        if (File.Exists(indexPath)) {
                            filePath = indexPath;
                            Console.WriteLine("index set");
                        }
                    }*/

                    /*LuaFunction router = luaTable["OnRequest"] as LuaFunction;
                    if (router != null)
                        filePath = rootPath + "/" +  router.Call(router, context.Request.Url.AbsolutePath.Substring(1).Trim(new char[] { ' ', '.', '*' }))[0] as string;*/

                    /*Dictionary<string, string> ChangeRoutes = new Dictionary<string, string>();

                    foreach (var route in (luaTable["ChangeRoutes"] as LuaTable))
                    {
                        ChangeRoutes.Add((string)((KeyValuePair<object, object>)route).Key, (string)((KeyValuePair<object, object>)route).Value);
                    }

                    if (string.IsNullOrEmpty(url) || !ChangeRoutes.TryGetValue(url.Split("/")[0], out _))
                    {
                        string indexPath = rootPath + "/index.html";

                        if (File.Exists(indexPath))
                        {
                            filePath = indexPath;
                        }
                    }*/

                    if (string.IsNullOrEmpty(url) || !Statics.Contains(url.Split("/")[0]))
                    {
                        string indexPath = rootPath + "/index.html";

                        if (File.Exists(indexPath))
                        {
                            filePath = indexPath;
                        }
                    }

                    if (File.Exists(filePath))
                    {
                        try
                        {
                            if (!cache.ContainsKey(url))
                            {
                                cache.Add(url, File.ReadAllBytes(filePath));
                            }

                            Stream data = new MemoryStream(cache[url]);

                            string mime;
                            context.Response.ContentType = mimeTypes.TryGetValue(Path.GetExtension(filePath), out mime) ? mime : "application/octet-stream";
                            context.Response.ContentLength64 = data.Length;

                            byte[] buffer = new byte[8192];
                            int bytes;

                            while ((bytes = data.Read(buffer, 0, buffer.Length)) > 0)
                                context.Response.OutputStream.Write(buffer, 0, bytes);
                            data.Close();

                            context.Response.StatusCode = (int)HttpStatusCode.OK;
                            context.Response.OutputStream.Flush();
                        }
                        catch (Exception e)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    }
                }
                catch (Exception)
                {

                }
            }
        }
    }
}
