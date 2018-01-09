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
using UnityEngine;
#if !WINDOWS_UWP
using System.IO;
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
        }

#if !WINDOWS_UWP
        private object sendlock = new object();
        private ClientWebSocket webSocket = null;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public override async Task ConnectAsync()
        {
            if (webSocket == null || webSocket.State == WebSocketState.Closed)
            {
                webSocket = new ClientWebSocket();
                webSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(5);
            }
            try
            {
                await webSocket.ConnectAsync(uri, cts.Token);
            }
            catch (Exception)
            {
                // e.message is just "Generic WebSocket exception" so we create our own.
                OnError?.Invoke("Can not connect to server.");
                OnClose?.Invoke(false);
                await Task.CompletedTask;
                return;
            }
            // Start a new Thread for incomming messages. 
            // If we would try to call the ReceiveAsync-function using just async Unity will freeze.
            await Task.Factory.StartNew(() => {
                if (webSocket.State == WebSocketState.Open)
                {
                    OnOpen?.Invoke();
                }
                try
                {
                    int maxSize = 1024*8;
                    ArraySegment<Byte> buffer = new ArraySegment<byte>(new Byte[maxSize]);
                    WebSocketReceiveResult result = null;
                    while (true)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            do
                            {
                                Task<WebSocketReceiveResult> t = webSocket.ReceiveAsync(buffer, cts.Token);
                                // Note: If we use the async await instead of synchronous Wait() we might get disconnected.
                                t.Wait();
                                result = t.Result;
                                ms.Write(buffer.Array, buffer.Offset, result.Count);
                                if (webSocket.State != WebSocketState.Open)
                                {
                                    ms.Seek(0, SeekOrigin.Begin);
                                    using (StreamReader reader = new StreamReader(ms, Encoding.UTF8))
                                    {
                                        OnError?.Invoke("Error while receiving data: " + reader.ReadToEnd());
                                    }
                                    OnError?.Invoke("State is " + webSocket.State + " instead of open.");
                                    break;
                                }
                                    

                            }
                            while (!result.EndOfMessage);
                            if (webSocket.State != WebSocketState.Open)
                            {
                                break;
                            }

                            ms.Seek(0, SeekOrigin.Begin);

                            if (result.MessageType == WebSocketMessageType.Text)
                            {
                                using (StreamReader reader = new StreamReader(ms, Encoding.UTF8))
                                {
                                    string json = reader.ReadToEnd();
                                    OnMessage?.Invoke(json);
                                }
                            }
                        }
                    }
                    OnClose?.Invoke(true);
                }
                catch (WebSocketException e)
                {
                    // we assume that we lost the connection when an error occurs, so we set the state back to DISCONNECTED
                    OnError?.Invoke(e.Message);
                    Dispose();
                    OnClose?.Invoke(false);
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
            webSocket = null;
        }

        public override void Send(string message)
        {
            if (webSocket == null)
            {
                OnError?.Invoke("WebSocket not set");
                return;
            }
            if (webSocket.State == WebSocketState.Closed)
            {
                OnError?.Invoke("Can not send message, WebSocket closed");
                return;
            }
            
            Byte[] bytes = Encoding.UTF8.GetBytes(message);
            Task t;
            lock (sendlock)
            {
                t = webSocket.SendAsync(new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text, true, cts.Token);

                t.Wait();
            }

        }
#endif
    }
}