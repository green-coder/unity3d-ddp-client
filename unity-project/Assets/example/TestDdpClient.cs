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

using UnityEngine;
using System.Collections;
using Moulin.DDP;
using System;
using UnityEngine.UI;
using System.Collections.Generic;

public class TestDdpClient : MonoBehaviour {
    public Text DebugText;

    public string serverUrl = "ws://localhost:3000/websocket";
	public bool logMessages;

	private DdpConnection ddpConnection;
    private Queue<string> logQueue;

    public void Start() {
		Application.runInBackground = true; // Let the game run when the editor is not focused.

        this.DebugText.text = "";
        logQueue = new Queue<string>();
        ddpConnection = new DdpConnection(serverUrl);
		ddpConnection.logMessages = logMessages;
        ddpConnection.OnDebugMessage += AddDebugText;

        ddpConnection.OnConnected += (DdpConnection connection) => {
            AddDebugText("Connected.");

			StartCoroutine(MyCoroutine());
		};

		ddpConnection.OnDisconnected += (DdpConnection connection) => {
            AddDebugText("Disconnected.");

			StartCoroutine(CoroutineHelper.GetInstance().RunAfter(() => {
                AddDebugText("Try to reconnect ...");
				connection.ConnectAsync();
			}, 2.0f));
		};

		ddpConnection.OnConnectionClosed += (DdpConnection connection) => {
            AddDebugText("Connection closed.");
		};

		ddpConnection.OnError += (DdpError error) => {
            AddDebugText("Error: " + error.errorCode + " " + error.reason);
		};

		ddpConnection.OnAdded += (collection, id, fields) => {
            AddDebugText("Added docId " + id +
				" in collection " + collection);
		};

		ddpConnection.OnRemoved += (collection, id) => {
            AddDebugText("Removed docId " + id +
				" in collection " + collection);
		};

		ddpConnection.OnChanged += (collection, id, fields, cleared) => {
            AddDebugText("Changed docId " + id +
				" in collection " + collection +
				" fields: " + fields +
				" cleared:" + cleared);
		};

		ddpConnection.OnAddedBefore += (collection, id, fields, before) => {
            AddDebugText("Added docId " + id +
				" before docId " + before +
				" in collection " + collection +
				" fields: " + fields);
		};

		ddpConnection.OnMovedBefore += (collection, id, before) => {
            AddDebugText("Moved docId " + id +
				" before docId " + before +
				" in collection " + collection);
		};

	}

    public void Connect()
    {
        AddDebugText("Connecting ...");
        ddpConnection.ConnectAsync();
    }

    public void Disconnect()
    {
        AddDebugText("Closing connection ...");
        ddpConnection.Close();
    }

    public void Subscribe()
    {
        friendSub = ddpConnection.Subscribe("friends");
        friendSub.OnReady = (Subscription obj) => {
            AddDebugText("Ready subscription: " + obj.id);
        };
    }

    public void Unsubscribe()
    {
        ddpConnection.Unsubscribe(friendSub);
    }

    public void RemoveAll()
    {
        ddpConnection.Call("friends.removeAll");
    }

    public void CreateFriends()
    {
        MethodCall methodCall = ddpConnection.Call("friends.create", JSONObject.CreateStringObject("Coco"));
        methodCall.OnUpdated = (MethodCall obj) => {
            AddDebugText("Updated, methodId=" + obj.id);
        };
        methodCall.OnResult = (MethodCall obj) => {
            AddDebugText("Result = " + obj.result);
        };
    }

    public void CallAdd()
    {
        MethodCall methodCall = ddpConnection.Call("friends.add", JSONObject.Create(19), JSONObject.Create(23));
        methodCall.OnUpdated = (MethodCall obj) => {
            AddDebugText("Updated, methodId=" + obj.id);
        };
        methodCall.OnResult = (MethodCall obj) => {
            AddDebugText("Result = " + obj.result);
        };
    }

    private Subscription friendSub;

    public void AddDebugText(string text)
    {
        logQueue.Enqueue(text);
    }


    public void Update()
    {
        if (logQueue != null)
        {
            while (logQueue.Count > 0)
            {
                string log = logQueue.Dequeue();
                Debug.Log(log);
                string[] original = DebugText.text.Split('\n');
                List<string> logMessages = new List<string>();
                for (int i = 0; i < Mathf.Min(original.Length, 10); i++)
                {
                    logMessages.Add(original[i]);
                }
                DebugText.text = log + "\n" + string.Join("\n", logMessages);
            }
        }

        if (Input.GetKeyDown(KeyCode.C)) {
            Connect();
		}

		if (Input.GetKeyDown(KeyCode.V)) {
            Disconnect();
		}

		if (Input.GetKeyDown(KeyCode.S)) {
            Subscribe();
		}

		if (Input.GetKeyDown(KeyCode.U)) {
            Unsubscribe();
		}

		if (Input.GetKeyDown(KeyCode.R)) {
            RemoveAll();
		}

		if (Input.GetKeyDown(KeyCode.F)) {
            CreateFriends();
		}

		if (Input.GetKeyDown(KeyCode.A)) {
            CallAdd();
		}

	}

    private IEnumerator MyCoroutine() {
		MethodCall methodCall = ddpConnection.Call("friends.add", JSONObject.Create(19), JSONObject.Create(23));
		yield return methodCall.WaitForResult();
        AddDebugText("(19 + 23)'s call has a result: " + methodCall.result.i);
	}

}
