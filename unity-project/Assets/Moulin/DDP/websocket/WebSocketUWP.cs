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
#if WINDOWS_UWP
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif

namespace Moulin.DDP
{
    public class WebSocketUWP : WebSocketConnection
    {
        public WebSocketUWP(DdpConnection ddpConnection, string url)
        {
            this.ddpConnection = ddpConnection;
            uri = new Uri(url);
        }

#if WINDOWS_UWP
        private MessageWebSocket messageWebSocket;
        private DataWriter messageWriter;

        public override async Task ConnectAsync()
        {
            try
            {
                if (messageWebSocket == null)
                {
                    messageWebSocket = new MessageWebSocket();
                    messageWebSocket.Control.MessageType = SocketMessageType.Utf8;
                    messageWebSocket.MessageReceived += MessageReceived;
                    messageWebSocket.Closed += Closed;
                }
                await messageWebSocket.ConnectAsync(uri);
                messageWriter = new DataWriter(messageWebSocket.OutputStream);
                OnOpen?.Invoke();
            } catch (Exception e)
            {
                OnError?.Invoke(e.Message);
                OnClose?.Invoke(false);
                await Task.CompletedTask;
            }
        }

        private void MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args) 
        {
            try
            {
                using (DataReader reader = args.GetDataReader()) {
                    reader.UnicodeEncoding = UnicodeEncoding.Utf8;
                    string read = reader.ReadString(reader.UnconsumedBufferLength);
                    OnMessage?.Invoke(read);
                }
            }
            catch (Exception e)
            {
                OnError?.Invoke(e.Message);
                OnClose?.Invoke(false);
            }
        }

        public override async Task CloseAsync()
        {
            try
            {
                messageWebSocket.Close(1, "closed by user");
                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                OnError?.Invoke("Close:" + e.Message);
            }
        }

        public void Closed(IWebSocket webSocket,  WebSocketClosedEventArgs args) {
            // OnClose.Invoke(args.Reason);
            OnClose?.Invoke(true); // TODO: detect if disconnect was clean
        }

        public override void Dispose()
        {
            try { 
                messageWebSocket.Dispose();
            }
            catch (Exception e)
            {
                OnError?.Invoke("Dispose:" + e.Message);
            }
        }

        public override void Send(string message) 
        {
            try
            {
                messageWriter.WriteString(message);
                Task t = Task.Run(() => messageWriter.StoreAsync());
                t.Wait(); // prevent the messagewriter from joining strings and instead send them directly
            } 
            catch (Exception e)
            {
                OnError?.Invoke("Send:" + e.Message);
            }
        }
#endif
    }
}