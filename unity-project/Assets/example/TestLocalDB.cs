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
using Moulin.DDP;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Threading.Tasks;
using System;

public class TestLocalDB : MonoBehaviour {
    public Text DebugText;

    public string serverUrl = "ws://localhost:3000/websocket";
	public bool logMessages;

	private DdpConnection ddpConnection;
	private LocalDB localDB;
	private JsonObjectCollection friendCollection;

    private Queue<string> logQueue;

    private Subscription friendSub;

    public async Task Start() {
		Application.runInBackground = true; // Let the game run when the editor is not focused.

        // clear debug log
        DebugText.text = "";
        logQueue = new Queue<string>();

        ddpConnection = new DdpConnection(serverUrl)
        {
            logMessages = logMessages
        };
        ddpConnection.OnDebugMessage += AddDebugText;

        ddpConnection.OnConnected += (DdpConnection connection) => {
            AddDebugText("Connected.");
		};

		ddpConnection.OnDisconnected += (DdpConnection connection) => {
            AddDebugText("Disconnected.");
            Reconnect();
        };

		ddpConnection.OnError += (DdpError error) => {
            AddDebugText("Error: " + error.errorCode + ": " + error.reason);
		};

		localDB = new LocalDB((db, collectionName) => {
			return new JsonObjectCollection(db, collectionName);
		}, ddpConnection);

		friendCollection = (JsonObjectCollection) localDB.GetCollection("friends");
		friendCollection.OnAdded += (id, fields) => {
            AddDebugText("Added docId " + id);
		};

		friendCollection.OnRemoved += (id) => {
            AddDebugText("Removed docId " + id);
		};

		friendCollection.OnChanged += (id, fields, cleared) => {
            AddDebugText("Changed docId " + id +
				" fields: " + fields +
				" cleared:" + cleared);
		};

        await ddpConnection.ConnectAsync();
    }

    private async void Reconnect()
    {
        await ReconnectAsync();
    }

    private async Task ReconnectAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(2));
        AddDebugText("Try to reconnect ...");
        await ddpConnection.ConnectAsync();
    }

    public void Connect()
    {
        ddpConnection.Connect();
    }

    public void Disconnect()
    {
        ddpConnection.Close();
    }

    public void Subscription()
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

    public void Create()
    {
        ddpConnection.Call("friends.create", JSONObject.CreateStringObject("Coco"));
    }

    public void DebugAttributes()
    {
        if (friendCollection.documents.Count == 0)
        {
            AddDebugText("No friends found. Subscribe first and make sure you created some friends.");
        }
        else
        {
            foreach (var entry in friendCollection.documents)
            {
                AddDebugText(entry.Key + " " + entry.Value);
            }
        }
    }

    public void AddAttributes()
    {
        JSONObject parents = new JSONObject();
        parents.AddField("mother", "wonder woman");
        parents.AddField("father", "batman");
        JSONObject attr = new JSONObject();
        attr.AddField("age", 24);
        attr.AddField("height", 180);
        attr.AddField("parents", parents);
        ddpConnection.Call("friends.addAttributes", JSONObject.StringObject("Coco"), attr);
    }

    public void RemoveAttributes()
    {
        JSONObject attr = new JSONObject();
        attr.AddField("age", 1);
        attr.AddField("height", 1);
        attr.AddField("parents.mother", 1);
        ddpConnection.Call("friends.removeAttributes", JSONObject.StringObject("Coco"), attr);
    }

    public void AddDebugText(string text)
    {
        Debug.Log(text);
        logQueue.Enqueue(text);
    }

    public void Update()
    {
        if (logQueue != null)
        {
            while (logQueue.Count > 0)
            {
                string log = logQueue.Dequeue();
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
            Subscription();
		}

		if (Input.GetKeyDown(KeyCode.U)) {
            Unsubscribe();
		}

		if (Input.GetKeyDown(KeyCode.R)) {
            RemoveAll();
		}

		if (Input.GetKeyDown(KeyCode.F)) {
            Create();
		}

		if (Input.GetKeyDown(KeyCode.D)) {
            DebugAttributes();
		}

		if (Input.GetKeyDown(KeyCode.O)) {
            AddAttributes();
		}

		if (Input.GetKeyDown(KeyCode.P)) {
            RemoveAttributes();
		}

	}

}
