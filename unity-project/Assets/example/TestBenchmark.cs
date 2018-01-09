/*
	The MIT License (MIT)

	Copyright (c) 2018 Andreas Bresser <self@andreasbresser.de>

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
using Moulin.DDP;
using UnityEngine.UI;
using System.Collections.Generic;

public class TestBenchmark : MonoBehaviour {
    public Text DebugText;

    public string serverUrl = "ws://localhost:3000/websocket";
	public bool logMessages;

    private LocalDB localDB;

    private DdpConnection ddpConnection;
    private Queue<string> logQueue;
    private JsonObjectCollection benchmarkCollection;
    private int Received;

    public void Start() {
		Application.runInBackground = true; // Let the game run when the editor is not focused.

        DebugText.text = "";
        logQueue = new Queue<string>();

        ddpConnection = new DdpConnection(serverUrl)
        {
            logMessages = logMessages
        };
        ddpConnection.OnDebugMessage += AddDebugText;

        ddpConnection.OnConnected += (DdpConnection connection) => {
            AddDebugText("Connected!");
            ddpConnection.Subscribe("benchmark");
        };
        ddpConnection.Connect();

		ddpConnection.OnDisconnected += (DdpConnection connection) => {
            AddDebugText("Disconnected.");
		};

		ddpConnection.OnConnectionClosed += (DdpConnection connection) => {
            AddDebugText("Connection closed.");
		};

		ddpConnection.OnError += (DdpError error) => {
            Debug.LogError("Error: " + error.errorCode + " " + error.reason);
            AddDebugText("Error: " + error.errorCode + " " + error.reason);
		};

        localDB = new LocalDB((db, collectionName) => {
            return new JsonObjectCollection(db, collectionName);
        }, ddpConnection);

        JsonObjectCollection benchmarkCollection = (JsonObjectCollection)localDB.GetCollection("benchmark");
        benchmarkCollection.OnAdded += (string docId, JSONObject fields) => {
            Received++;
            if (Received % 100 == 0)
            {
                AddDebugText("Received " + Received + " items");
            }
        };
    }
    
    public void AddDebugText(string text)
    {
		Debug.Log(text);
        logQueue.Enqueue(text);
    }

    public void Recreate()
    {
        Received = 0;
        ddpConnection.Call("benchmark.recreate");
    }

    public void CallCount()
    {
        CallCount(0);
    }

    public void CallCount(long i)
    {
        MethodCall countCall = ddpConnection.Call("benchmark.count", JSONObject.Create(i));
        countCall.OnResult += (MethodCall call) =>
        {
            if (call.result.i < 20)
            {
                CallCount(call.result.i);
            }
            else
            {
                AddDebugText("CallCount done after " + call.result.i + " calls (last id: " + call.id + ")!");
            }
        };
        
    }
 
    public void Connect()
    {
        ddpConnection.Connect();
    }

    public void Disconnect()
    {
        ddpConnection.Close();
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

        if (Input.GetKeyDown(KeyCode.C))
        {
            Connect();
		}

		if (Input.GetKeyDown(KeyCode.V))
        {
            Disconnect();
		}

        if (Input.GetKeyDown(KeyCode.R)) // R like Recreate
        {
            Recreate();
        }

        if (Input.GetKeyDown(KeyCode.P)) // P like PingPong
        {
            CallCount();
        }
    }
}
