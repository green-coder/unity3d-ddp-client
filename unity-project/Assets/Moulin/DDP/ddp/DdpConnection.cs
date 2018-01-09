/*
	The MIT License (MIT)

	Copyright (c) 2016 Vincent Cantin (user "green-coder" on Github.com)

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

﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Moulin.DDP
{

	/*
	 * DDP protocol:
	 *   https://github.com/meteor/meteor/blob/master/packages/ddp/DDP.md
	 */
	public class DdpConnection : IDisposable {

		// The possible values for the "msg" field.
		public class MessageType {
			// Client -> server.
			public const string CONNECT = "connect";
			public const string PONG    = "pong";
			public const string SUB     = "sub";
			public const string UNSUB   = "unsub";
			public const string METHOD  = "method";

			// Server -> client.
			public const string CONNECTED    = "connected";
			public const string FAILED       = "failed";
			public const string PING         = "ping";
			public const string NOSUB        = "nosub";
			public const string ADDED        = "added";
			public const string CHANGED      = "changed";
			public const string REMOVED      = "removed";
			public const string READY        = "ready";
			public const string ADDED_BEFORE = "addedBefore";
			public const string MOVED_BEFORE = "movedBefore";
			public const string RESULT       = "result";
			public const string UPDATED      = "updated";
			public const string ERROR        = "error";
		}

		// Field names supported in the DDP protocol.
		public class Field {
			public const string SERVER_ID   = "server_id";
			public const string MSG         = "msg";
			public const string SESSION     = "session";
			public const string VERSION     = "version";
			public const string SUPPORT     = "support";

			public const string NAME        = "name";
			public const string PARAMS      = "params";
			public const string SUBS        = "subs";
			public const string COLLECTION  = "collection";
			public const string FIELDS      = "fields";
			public const string CLEARED     = "cleared";
			public const string BEFORE      = "before";

			public const string ID          = "id";
			public const string METHOD      = "method";
			public const string METHODS     = "methods";
			public const string RANDOM_SEED = "randomSeed"; // unused
			public const string RESULT      = "result";

			public const string ERROR       = "error";
			public const string REASON      = "reason";
			public const string DETAILS     = "details";
			public const string MESSAGE     = "message";   // undocumented
			public const string ERROR_TYPE  = "errorType"; // undocumented
			public const string OFFENDING_MESSAGE = "offendingMessage";
		}

        public enum ConnectionState {
			NOT_CONNECTED,
			CONNECTING,
			CONNECTED,
			DISCONNECTED,
			CLOSING,
			CLOSED
		}

		// The DDP protocol version implemented by this library.
		public const string DDP_PROTOCOL_VERSION = "1";
        
        private WebSocketConnection ws;
		private ConnectionState ddpConnectionState;
		private string sessionId;
        
		private Dictionary<string, Subscription> subscriptions = new Dictionary<string, Subscription>();
		private Dictionary<string, MethodCall> methodCalls = new Dictionary<string, MethodCall>();

		private int subscriptionId;
		private int methodCallId;

        public delegate void OnDebugMessageDelegate(string message);
        public delegate void OnConnectedDelegate(DdpConnection connection);
		public delegate void OnDisconnectedDelegate(DdpConnection connection);
		public delegate void OnConnectionClosedDelegate(DdpConnection connection);
		public delegate void OnAddedDelegate(string collection, string docId, JSONObject fields);
		public delegate void OnChangedDelegate(string collection, string docId, JSONObject fields, JSONObject cleared);
		public delegate void OnRemovedDelegate(string collection, string docId);
		public delegate void OnAddedBeforeDelegate(string collection, string docId, JSONObject fields, string before);
		public delegate void OnMovedBeforeDelegate(string collection, string docId, string before);
		public delegate void OnErrorDelegate(DdpError error);

        public event OnDebugMessageDelegate OnDebugMessage;
		public event OnConnectedDelegate OnConnected;
		public event OnDisconnectedDelegate OnDisconnected;
		public event OnConnectionClosedDelegate OnConnectionClosed;
		public event OnAddedDelegate OnAdded;
		public event OnChangedDelegate OnChanged;
		public event OnRemovedDelegate OnRemoved;
		public event OnAddedBeforeDelegate OnAddedBefore;
		public event OnMovedBeforeDelegate OnMovedBefore;
		public event OnErrorDelegate OnError;

		public bool logMessages;
        public string url;

		public DdpConnection(string url) {
            this.url = url;
#if WINDOWS_UWP
            ws = new WebSocketUWP(this, url);
#else
            ws = new WebSocketSystemNet(this, url);
#endif
			ws.OnOpen += OnWebSocketOpen;
			ws.OnError += OnWebSocketError;
			ws.OnClose += OnWebSocketClose;
			ws.OnMessage += OnWebSocketMessage;
        }

        public void OnWebSocketOpen()
        {
            OnDebugMessage?.Invoke("Websocket open");
            Send(GetConnectMessage());
			foreach (Subscription subscription in subscriptions.Values)
			{
				Send(GetSubscriptionMessage(subscription));
			}
			foreach (MethodCall methodCall in methodCalls.Values)
			{
				Send(GetMethodCallMessage(methodCall));
			}
        }

		private void OnWebSocketError(string reason) {
            OnError?.Invoke(new DdpError()
            {
                errorCode = "WebSocket error",
                reason = reason
            });
		}

		private void OnWebSocketClose(bool wasClean) {
			if (wasClean) {
				ddpConnectionState = ConnectionState.CLOSED;
				sessionId = null;
				subscriptions.Clear();
				methodCalls.Clear();
                OnDisconnected?.Invoke(this);
			} else {
				ddpConnectionState = ConnectionState.DISCONNECTED;
				OnDisconnected?.Invoke(this);
			}
		}

        // TODO: return Task instead of void
		private void OnWebSocketMessage(string data) {
			if (logMessages) OnDebugMessage?.Invoke("OnMessage: " + data);
			JSONObject message = new JSONObject(data);
            HandleMessage(message);
		}

		private void HandleMessage(JSONObject message) {
			if (!message.HasField(Field.MSG)) {
				// Silently ignore those messages.
				return;
			}

			switch (message[Field.MSG].str) {
			    case MessageType.CONNECTED: {
					sessionId = message[Field.SESSION].str;
					ddpConnectionState = ConnectionState.CONNECTED;
                    OnConnected?.Invoke(this);
                    break;
				}

			    case MessageType.FAILED: {
                    OnError?.Invoke(new DdpError()
                    {
                        errorCode = "Connection refused",
                        reason = "The server is using an unsupported DDP protocol version: " +
                        message[Field.VERSION]
                    });
                    Close();
					break;
				}

			    case MessageType.PING: {
					if (message.HasField(Field.ID)) {
						Send(GetPongMessage(message[Field.ID].str));
					}
					else {
                        Send(GetPongMessage());
					}
					break;
				}

			    case MessageType.NOSUB: {
				    string subscriptionId = message[Field.ID].str;
					subscriptions.Remove(subscriptionId);

				    if (message.HasField(Field.ERROR)) {
                        OnError?.Invoke(GetError(message[Field.ERROR]));
                    }
				    break;
			    }

			    case MessageType.ADDED: {
                    OnAdded?.Invoke(
                        message[Field.COLLECTION].str,
                        message[Field.ID].str,
                        message[Field.FIELDS]);
                    break;
			    }

			    case MessageType.CHANGED: {
                    OnChanged?.Invoke(
                        message[Field.COLLECTION].str,
                        message[Field.ID].str,
                        message[Field.FIELDS],
                        message[Field.CLEARED]);
                    break;
			    }

			    case MessageType.REMOVED: {
                        OnRemoved?.Invoke(
                            message[Field.COLLECTION].str,
                            message[Field.ID].str);
                        break;
			    }

			    case MessageType.READY: {
				    string[] subscriptionIds = ToStringArray(message[Field.SUBS]);

				    foreach (string subscriptionId in subscriptionIds) {
					    Subscription subscription;
						subscription = subscriptions[subscriptionId];
					    if (subscription != null) {
						    subscription.isReady = true;
                            subscription.OnReady?.Invoke(subscription);
                        }
				    }
				    break;
			    }

			    case MessageType.ADDED_BEFORE: {
                     OnAddedBefore?.Invoke(
                            message[Field.COLLECTION].str,
                            message[Field.ID].str,
                            message[Field.FIELDS],
                            message[Field.BEFORE].str);
                            break;
			    }

			    case MessageType.MOVED_BEFORE: {
                OnMovedBefore?.Invoke(
                    message[Field.COLLECTION].str,
                    message[Field.ID].str,
                    message[Field.BEFORE].str);
                    break;
				}

			    case MessageType.RESULT: {
					string methodCallId = message[Field.ID].str;
					MethodCall methodCall = methodCalls[methodCallId];
					if (methodCall != null) {
						if (message.HasField(Field.ERROR)) {
							methodCall.error = GetError(message[Field.ERROR]);
						}
						methodCall.result = message[Field.RESULT];
						if (methodCall.hasUpdated) {
							methodCalls.Remove(methodCallId);
						}
						methodCall.hasResult = true;
                        methodCall.OnResult?.Invoke(methodCall);
                    }
					break;
				}

			    case MessageType.UPDATED: {
					string[] methodCallIds = ToStringArray(message[Field.METHODS]);
					foreach (string methodCallId in methodCallIds) {
						MethodCall methodCall = methodCalls[methodCallId];
						if (methodCall != null) {
							if (methodCall.hasResult) {
								methodCalls.Remove(methodCallId);
							}
							methodCall.hasUpdated = true;
                            methodCall.OnUpdated?.Invoke(methodCall);
                        }
					}
					break;
				}

			    case MessageType.ERROR: {
                    OnError?.Invoke(GetError(message));
                    break;
				}
			}
		}

		private string GetConnectMessage() {
			JSONObject message = new JSONObject(JSONObject.Type.OBJECT);
			message.AddField(Field.MSG, MessageType.CONNECT);
			if (sessionId != null) {
				message.AddField(Field.SESSION, sessionId);
			}
			message.AddField(Field.VERSION, DDP_PROTOCOL_VERSION);

			JSONObject supportedVersions = new JSONObject(JSONObject.Type.ARRAY);
			supportedVersions.Add(DDP_PROTOCOL_VERSION);
			message.AddField(Field.SUPPORT, supportedVersions);

			return message.Print();
		}

		private string GetPongMessage() {
			JSONObject message = new JSONObject(JSONObject.Type.OBJECT);
			message.AddField(Field.MSG, MessageType.PONG);

			return message.Print();
		}

		private string GetPongMessage(string id) {
			JSONObject message = new JSONObject(JSONObject.Type.OBJECT);
			message.AddField(Field.MSG, MessageType.PONG);
			message.AddField(Field.ID, id);

			return message.Print();
		}

		private string GetSubscriptionMessage(Subscription subscription) {
			JSONObject message = new JSONObject(JSONObject.Type.OBJECT);
			message.AddField(Field.MSG, MessageType.SUB);
			message.AddField(Field.ID, subscription.id);
			message.AddField(Field.NAME, subscription.name);
			if (subscription.items.Length > 0) {
				message.AddField(Field.PARAMS, new JSONObject(subscription.items));
			}

			return message.Print();
		}

		private string GetUnsubscriptionMessage(Subscription subscription) {
			JSONObject message = new JSONObject(JSONObject.Type.OBJECT);
			message.AddField(Field.MSG, MessageType.UNSUB);
			message.AddField(Field.ID, subscription.id);

			return message.Print();
		}

		private string GetMethodCallMessage(MethodCall methodCall) {
			JSONObject message = new JSONObject(JSONObject.Type.OBJECT);
			message.AddField(Field.MSG, MessageType.METHOD);
			message.AddField(Field.METHOD, methodCall.methodName);
			if (methodCall.items.Length > 0) {
				message.AddField(Field.PARAMS, new JSONObject(methodCall.items));
			}
			message.AddField(Field.ID, methodCall.id);
			//message.AddField(Field.RANDOM_SEED, xxx);

			return message.Print();
		}

		private DdpError GetError(JSONObject obj) {
			string errorCode = null;
			if (obj.HasField(Field.ERROR)) {
				JSONObject errorCodeObj = obj[Field.ERROR];
				errorCode = errorCodeObj.IsNumber ? "" + errorCodeObj.i : errorCodeObj.str;
			}

			return new DdpError() {
				errorCode = errorCode,
				reason = obj[Field.REASON].str,
				message = obj.HasField(Field.MESSAGE) ? obj[Field.MESSAGE].str : null,
				errorType = obj.HasField(Field.ERROR_TYPE) ? obj[Field.ERROR_TYPE].str : null,
				offendingMessage = obj.HasField(Field.OFFENDING_MESSAGE) ? obj[Field.OFFENDING_MESSAGE].str : null
			};
		}

		private string[] ToStringArray(JSONObject jo) {
			string[] result = new string[jo.Count];
			for (int i = 0; i < result.Length; i++) {
				result[i] = jo[i].str;
			}
			return result;
		}

        private void Send(string message) {
            if (logMessages) OnDebugMessage?.Invoke("Send: " + message);
            ws.Send(message);
		}

		public ConnectionState GetConnectionState() {
			return ddpConnectionState;
		}

        public async void Connect()
        {
            await ConnectAsync();
        }

		public async Task ConnectAsync() {
			if ((ddpConnectionState == ConnectionState.NOT_CONNECTED) ||
  			  (ddpConnectionState == ConnectionState.DISCONNECTED) ||
  			  (ddpConnectionState == ConnectionState.CLOSED)) {
  			    ddpConnectionState = ConnectionState.CONNECTING;
                if (logMessages) OnDebugMessage?.Invoke("Connecting to " + url + " ...");
                await ws.ConnectAsync();
            } else if (logMessages) OnDebugMessage?.Invoke("Connect request ignored: Already " + ddpConnectionState);
		}

        public async void Close()
        {
            await CloseAsync();
        }

		public async Task CloseAsync() {
			if (ddpConnectionState == ConnectionState.CONNECTED) {
				ddpConnectionState = ConnectionState.CLOSING;
                await ws.CloseAsync();
                ddpConnectionState = ConnectionState.CLOSED;
            }
		}

		void Dispose() {
			Close();
            ws.Dispose();
		}

        public Subscription Subscribe(string name, params JSONObject[] items)
        {
            Subscription subscription = new Subscription()
            {
                id = "" + subscriptionId++,
                name = name,
                items = items
            };
            subscriptions[subscription.id] = subscription;
            Send(GetSubscriptionMessage(subscription));
            return subscription;
        }

        public void Unsubscribe(Subscription subscription) {
			Send(GetUnsubscriptionMessage(subscription));
		}

        public MethodCall Call(string methodName, params JSONObject[] items) {
			MethodCall methodCall = new MethodCall() {
				id = "" + methodCallId++,
				methodName = methodName,
				items = items
			};
			methodCalls[methodCall.id] = methodCall;
            Send(GetMethodCallMessage(methodCall));
            
            return methodCall;
		}

        void IDisposable.Dispose()
        {
           Dispose();
        }
    }

}
