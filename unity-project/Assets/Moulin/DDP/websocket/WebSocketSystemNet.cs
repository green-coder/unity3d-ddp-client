/*
	The MIT License (MIT)
    
    Copyright (c) 2017 Andreas Bresser <self@andreasbresser.de>

	Permission is hereby granted, free of charge, to any person obtaining a copy of
	this software and associated documentation files (the "Software"), to deal in
	the Software without restriction, including without limitation the rights to
	use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
	of the Software, and to permit persons to whom the Software is furnished to do
	so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in all
	copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
	SOFTWARE.
*/

using System;
using System.Threading.Tasks;
#if !WINDOWS_UWP
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
#endif

namespace Moulin.DDP
{
    public class WebSocketSystemNet : WebSocketConnection
    {
        public WebSocketSystemNet(DdpConnection ddpConnection, string url)
        {
            this.ddpConnection = ddpConnection;
            uri = new Uri(url);
            Task.Run(() => ConnectAsync());
        }

#if !WINDOWS_UWP
        private ClientWebSocket webSocket;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public override async Task ConnectAsync()
        {
            webSocket = new ClientWebSocket();
            await webSocket.ConnectAsync(uri, cts.Token);
            OnOpen.Invoke();
            await Task.Factory.StartNew(
                async () =>
                {
                    ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                    while (true)
                    {
                        WebSocketReceiveResult result = await webSocket.ReceiveAsync(buffer, cts.Token);
                        if (webSocket.State != WebSocketState.Open)
                        {
                            break;
                        }
                        string json = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                        OnMessage.Invoke(json);
                    }
                }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            // TODO: detect if some error occured and the connection was not closed clean
            // OnClose.Invoke(true);
        }

        public override async Task CloseAsync()
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cts.Token);
        }

        public override void Dispose()
        {
            webSocket.Dispose();
        }

        async public override Task Send(string message)
        {
            Byte[] bytes = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text, true, cts.Token);
        }
#endif
    }
}