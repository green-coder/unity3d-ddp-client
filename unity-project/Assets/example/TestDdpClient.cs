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
using System.Collections;
using DDP;

public class TestDdpClient : MonoBehaviour {

	public string serverUrl = "ws://localhost:3000/websocket";
	public bool logMessages;

	private DdpConnection ddpConnection;

	public void Start() {
		ddpConnection = new DdpConnection(serverUrl);
		ddpConnection.logMessages = logMessages;

		ddpConnection.OnConnected += (DdpConnection connection) => {
			Debug.Log("Connected.");

			StartCoroutine(MyCoroutine());
		};

		ddpConnection.OnDisconnected += (DdpConnection connection) => {
			Debug.Log("Disconnected.");

			StartCoroutine(CoroutineHelper.GetInstance().RunAfter(() => {
				Debug.Log("Try to reconnect ...");
				connection.Connect();
			}, 2.0f));
		};

		ddpConnection.OnConnectionClosed += (DdpConnection connection) => {
			Debug.Log("Connection closed.");
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

	private Subscription friendSub;

	public void Update() {
		if (Input.GetKeyDown(KeyCode.C)) {
			Debug.Log("Connecting ...");
			ddpConnection.Connect();
		}

		if (Input.GetKeyDown(KeyCode.V)) {
			Debug.Log("Closing connection ...");
			ddpConnection.Close();
		}

		if (Input.GetKeyDown(KeyCode.S)) {
			friendSub = ddpConnection.Subscribe("friends");
			friendSub.OnReady = (Subscription obj) => {
				Debug.Log("Ready subscription: " + obj.id);
			};
		}

		if (Input.GetKeyDown(KeyCode.U)) {
			ddpConnection.Unsubscribe(friendSub);
		}

		if (Input.GetKeyDown(KeyCode.R)) {
			ddpConnection.Call("friends.removeAll");
		}

		if (Input.GetKeyDown(KeyCode.F)) {
			MethodCall methodCall = ddpConnection.Call("friends.create", JSONObject.CreateStringObject("Coco"));
			methodCall.OnUpdated = (MethodCall obj) => {
				Debug.Log("Updated, methodId=" + obj.id);
			};
			methodCall.OnResult = (MethodCall obj) => {
				Debug.Log("Result = " + obj.result);
			};
		}

		if (Input.GetKeyDown(KeyCode.A)) {
			MethodCall methodCall = ddpConnection.Call("friends.add", JSONObject.Create(7), JSONObject.Create(5));
			methodCall.OnUpdated = (MethodCall obj) => {
				Debug.Log("Updated, methodId=" + obj.id);
			};
			methodCall.OnResult = (MethodCall obj) => {
				Debug.Log("Result = " + obj.result);
			};
		}

	}

	private IEnumerator MyCoroutine() {
		MethodCall methodCall = ddpConnection.Call("friends.add", JSONObject.Create(19), JSONObject.Create(23));
		yield return methodCall.WaitForResult();
		Debug.Log("(19 + 23)'s call has a result: " + methodCall.result.i);
	}

}
