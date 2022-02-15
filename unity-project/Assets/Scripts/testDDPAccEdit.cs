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

public class testDDPAccEdit : MonoBehaviour {

	public string serverUrl = "ws://localhost:3000/websocket";
	private string username; 
	private string password;
	public string token; 
	public bool logMessages;

	private DdpConnection ddpConnection;
	private DdpAccount account;

	public void Start() {
		Application.runInBackground = true; // Let the game run when the editor is not focused.

		ddpConnection = new DdpConnection(serverUrl);
		ddpConnection.logMessages = logMessages;

		account = new DdpAccount(ddpConnection);

		ddpConnection.OnConnected += (DdpConnection connection) => {
			Debug.Log("Connected.");
		};

		ddpConnection.OnDisconnected += (DdpConnection connection) => {
			Debug.Log("Disconnected.");

			StartCoroutine(CoroutineHelper.GetInstance().RunAfter(() => {
				Debug.Log("Try to reconnect ...");
				connection.Connect();
			}, 2.0f));
		};

		ddpConnection.OnError += (DdpError error) => {
			Debug.Log("Error: " + error.errorCode + " " + error.reason);
		};

		ddpConnection.OnAdded += (collection, id, fields) => {
			Debug.Log("Added docId " + id +
				" in collection " + collection);
		};

		ddpConnection.OnRemoved += (collection, id) => {
			Debug.Log("Removed docId " + id +
				" in collection " + collection);
		};

		ddpConnection.OnChanged += (collection, id, fields, cleared) => {
			Debug.Log("Changed docId " + id +
				" in collection " + collection +
				" fields: " + fields +
				" cleared:" + cleared);
		};

		ddpConnection.OnAddedBefore += (collection, id, fields, before) => {
			Debug.Log("Added docId " + id +
				" before docId " + before +
				" in collection " + collection +
				" fields: " + fields);
		};

		ddpConnection.OnMovedBefore += (collection, id, before) => {
			Debug.Log("Moved docId " + id +
				" before docId " + before +
				" in collection " + collection);
		};

	}
/*
	public void Update() {

		if (Input.GetKeyDown(KeyCode.C)) {
			Debug.Log("Connecting ...");
			ddpConnection.Connect();
		}

		if (Input.GetKeyDown(KeyCode.V)) {
			ddpConnection.Close();
		}
		

		if (Input.GetKeyDown(KeyCode.U)) {
			StartCoroutine(account.CreateUserAndLogin(username, password));
		}

		if (Input.GetKeyDown(KeyCode.L)) {
			StartCoroutine(account.Login(username, password));
		}


		if (Input.GetKeyDown(KeyCode.R)) {
			StartCoroutine(account.ResumeSession(token));
		}

		if (Input.GetKeyDown(KeyCode.T)) {
			Debug.Log("Token " + account.token + " expires at " + account.tokenExpiration);
		}

		if (Input.GetKeyDown(KeyCode.O)) {
			StartCoroutine(account.Logout());
		}
	}
	*/

    public void ConnectDdp(){
        Debug.Log("Connecting ...");
				ddpConnection.Connect();
    }

    public void DisconnectDdp(){
        ddpConnection.Close();
				Debug.Log("connection closed");
    }

		public void ReadUserInput(string s){
				username = s;
				Debug.Log("username: " + username);
		}

		public void ReadPasswordInput(string s){
				password = s;
				Debug.Log("password: " + password);
		}

    public void LoginUser(){
        StartCoroutine(account.Login(username, password));
				Debug.Log("Logged in");
		}

		public void LogoutUser(){
        StartCoroutine(account.Logout());
				Debug.Log("Logged out");
		}

		void OnApplicationFocus(bool pauseStatus) {
      if(pauseStatus){
          Debug.Log("your app is NO LONGER in the background");
      }else{
          Debug.Log("your app is now in the background");
      }
 		}
}
