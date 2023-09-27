using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IoT_Server.Modules.WebSocket
{
    class WebSocketClient
    {
        public int id { get; private set; }
        public System.Net.WebSockets.WebSocket ws { get; private set; }

        public WebSocketClient(int id, System.Net.WebSockets.WebSocket ws)
        {
            this.id = id;
            this.ws = ws;
        }

        public async Task Send(string data)
        {
            await ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(data)), System.Net.WebSockets.WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
        }

        public async Task Send(int data)
        {
            await ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(data.ToString())), System.Net.WebSockets.WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
        }

        public async Task Close()
        {
            await ws.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Closed by server", new System.Threading.CancellationTokenSource(2500).Token);
        }
    }
}
