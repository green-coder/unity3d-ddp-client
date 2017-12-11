/*
	The MIT License (MIT)

	Copyright (c) 2016 Vincent Cantin (user "green-coder" on Github.com)
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

using UnityEngine;
using Moulin.DDP;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections.Generic;

public class TestDdpAccount : MonoBehaviour {
    public Text DebugText;

	public string serverUrl = "ws://localhost:3000/websocket";
	public string username;
	public string password;
	public string token;
	public bool logMessages;

    private Queue<string> logQueue;

	private DdpConnection ddpConnection;
	private DdpAccount account;

	public void Start() {
        Application.runInBackground = true; // Let the game run when the editor is not focused.

        // clear debug log
        this.DebugText.text = ""; 
        logQueue = new Queue<string>();

        ddpConnection = new DdpConnection(serverUrl)
        {
            logMessages = logMessages
        };
        ddpConnection.OnDebugMessage += AddDebugText;

		account = new DdpAccount(ddpConnection);

		ddpConnection.OnConnected += (DdpConnection connection) => {
            AddDebugText("Connected.");
		};

		ddpConnection.OnDisconnected += (DdpConnection connection) => {
            AddDebugText("Disconnected.");

			StartCoroutine(CoroutineHelper.GetInstance().RunAfter(async () => {
                AddDebugText("Try to reconnect ...");
				await connection.ConnectAsync();
			}, 2.0f));
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
        Task.Run(() => ddpConnection.ConnectAsync());
    }

    public void Disconnect()
    {
        ddpConnection.Close();
    }

    public void CreateUserAndLogin()
    {
        StartCoroutine(account.CreateUserAndLogin(username, password));
    }

    public void Login()
    {
        StartCoroutine(account.Login(username, password));
    }

    public void ResumeSession()
    {
        StartCoroutine(account.ResumeSession(token));
    }

    public void TokenInformation()
    {
        AddDebugText("Token " + account.token + " expires at " + account.tokenExpiration);
    }

    public void Logout()
    {
        StartCoroutine(account.Logout());
    }

    public void AddDebugText(string text)
    {
        logQueue.Enqueue(text);
    }


    public void Update() {
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

		if (Input.GetKeyDown(KeyCode.U)) {
            CreateUserAndLogin();
        }

		if (Input.GetKeyDown(KeyCode.L)) {
            Login();
		}

		if (Input.GetKeyDown(KeyCode.R)) {
            ResumeSession();
		}

		if (Input.GetKeyDown(KeyCode.T)) {
            TokenInformation();
		}

		if (Input.GetKeyDown(KeyCode.O)) {
            Logout();
		}
	}
}
