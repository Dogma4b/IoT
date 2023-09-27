using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using NLua;
using System.Collections.Concurrent;

namespace IoT_Server.Modules.WebSocket
{
    class WebSocketServer
    {
        private HttpListener listener;
        internal LuaTable luaTable;

        private CancellationTokenSource socketTaskToken;
        private CancellationTokenSource listenerTaskToken;

        private int socketCounter = 0;

        private bool IsActive = true;

        private ConcurrentDictionary<int, WebSocketClient> clients = new ConcurrentDictionary<int, WebSocketClient>();

        public void Start(string ip, int port)
        {
            IsActive = true;

            socketTaskToken = new CancellationTokenSource();
            listenerTaskToken = new CancellationTokenSource();

            listener = new HttpListener();
            listener.Prefixes.Add("http://" + ip + ":" + port + "/");
            listener.Start();

            if (listener.IsListening)
            {
                LuaFunction OnStart = luaTable["OnStart"] as LuaFunction;
                if (OnStart != null)
                    OnStart.Call(luaTable);

                Task.Run(() => ListenerTask().ConfigureAwait(false));
            }
            else
            {
                Console.WriteLine("Web Socket server failed to start");
            }
        }

        public async Task Stop()
        {
            if (listener?.IsListening ?? false && IsActive)
            {
                LuaFunction OnStop = luaTable["OnStop"] as LuaFunction;
                if (OnStop != null)
                    OnStop.Call(luaTable);

                IsActive = false;

                await CloseAllSockets();
                listenerTaskToken.Cancel();

                listener.Stop();
                listener.Close();
            }
        }

        public async Task BroadCast(string data)
        {
            foreach (WebSocketClient client in clients.Values)
            {
                await client.Send(data);
            }
        }

        private async Task ListenerTask()
        {
            CancellationToken cancellationToken = listenerTaskToken.Token;

            try
            {
                while(!cancellationToken.IsCancellationRequested)
                {
                    HttpListenerContext context = await listener.GetContextAsync();

                    if (IsActive)
                    {
                        if (context.Request.IsWebSocketRequest)
                        {
                            Thread.Sleep(50);
                            HttpListenerWebSocketContext wsContext = null;

                            try
                            {
                                wsContext = await context.AcceptWebSocketAsync(null);
                                int socketId = Interlocked.Increment(ref socketCounter);
                                WebSocketClient client = new WebSocketClient(socketId, wsContext.WebSocket);

                                clients.TryAdd(socketId, client);
                                _ = Task.Run(() => SocketTask(client).ConfigureAwait(false));

                                try
                                {
                                    LuaFunction OnConnect = luaTable["OnConnect"] as LuaFunction;
                                    if (OnConnect != null)
                                        OnConnect.Call(luaTable, client);
                                }
                                catch (AccessViolationException e)
                                {
                                    Console.WriteLine(e.Message);
                                }
                            }
                            catch (Exception)
                            {
                                context.Response.StatusCode = 500;
                                context.Response.StatusDescription = "WebSocket upgrade failed";
                                context.Response.Close();
                                return;
                            }
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = 409;
                        context.Response.StatusDescription = "Server is shutting down";
                        context.Response.Close();
                        return;
                    }
                }
            }
            catch (HttpListenerException e) when (IsActive)
            {
                Console.WriteLine(e.Message);
            }
        }

        private async Task SocketTask(WebSocketClient client)
        {
            System.Net.WebSockets.WebSocket socket = client.ws;
            CancellationToken stToken = socketTaskToken.Token;

            try
            {
                ArraySegment<byte> buffer = System.Net.WebSockets.WebSocket.CreateServerBuffer(8192);

                while (socket.State != WebSocketState.Closed && socket.State != WebSocketState.Aborted && !stToken.IsCancellationRequested)
                {
                    WebSocketReceiveResult receiveResult = await client.ws.ReceiveAsync(buffer, stToken);

                    if (!stToken.IsCancellationRequested)
                    {
                        if (client.ws.State == WebSocketState.CloseReceived && receiveResult.MessageType == WebSocketMessageType.Close)
                        {
                            await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Acknowledge close frame", CancellationToken.None);
                        }

                        if (client.ws.State == WebSocketState.Open)
                        {
                            try
                            {
                                LuaFunction OnReceive = luaTable["OnReceive"] as LuaFunction;
                            if (OnReceive != null)
                                OnReceive.Call(luaTable, client, Encoding.UTF8.GetString(buffer));
                            //Thread.Sleep(10);
                            }
                            catch (AccessViolationException e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception e)
            {
                LuaFunction OnError = luaTable["OnError"] as LuaFunction;
                if (OnError != null)
                    OnError.Call(luaTable, e.Message);
            }
            finally
            {
                if (client.ws.State != WebSocketState.Closed)
                {
                    client.ws.Abort();
                }

                if (clients.TryRemove(client.id, out _))
                {
                    socket.Dispose();
                }

                LuaFunction OnDisconnect = luaTable["OnDisconnect"] as LuaFunction;
                if (OnDisconnect != null)
                    OnDisconnect.Call(luaTable, client);
            }
        }

        private async Task CloseAllSockets()
        {
            List<System.Net.WebSockets.WebSocket> disposeList = new List<System.Net.WebSockets.WebSocket>(clients.Count);

            while (clients.Count > 0)
            {
                WebSocketClient client = clients.ElementAt(0).Value;

                if (client.ws.State == WebSocketState.Open)
                {
                    CancellationTokenSource timeout = new CancellationTokenSource(2500);

                    try
                    {
                        await client.ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server is shutting down", timeout.Token);
                        Thread.Sleep(1); // That's needed to be sync with lua
                    }
                    catch (OperationCanceledException)
                    {

                    }
                }

                if (clients.TryRemove(client.id, out _))
                {
                    disposeList.Add(client.ws);
                }
            }

            socketTaskToken.Cancel();

            foreach (System.Net.WebSockets.WebSocket ws in disposeList)
            {
                ws.Dispose();
            }
        }
    }
}
