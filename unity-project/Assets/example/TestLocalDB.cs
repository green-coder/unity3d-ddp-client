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
using Moulin.DDP;

public class TestLocalDB : MonoBehaviour {

	public string serverUrl = "ws://localhost:3000/websocket";
	public bool logMessages;

	private DdpConnection ddpConnection;
	private LocalDB localDB;
	private JsonObjectCollection friendCollection;

	public void Start() {
		Application.runInBackground = true; // Let the game run when the editor is not focused.

		ddpConnection = new DdpConnection(serverUrl);
		ddpConnection.logMessages = logMessages;

		ddpConnection.OnConnected += (DdpConnection connection) => {
			Debug.Log("Connected.");
		};

		ddpConnection.OnDisconnected += (DdpConnection connection) => {
			Debug.Log("Disconnected.");

			StartCoroutine(CoroutineHelper.GetInstance().RunAfter(() => {
				Debug.Log("Try to reconnect ...");
				connection.ConnectAsync();
			}, 2.0f));
		};

		ddpConnection.OnError += (DdpError error) => {
			Debug.Log("Error: " + error.errorCode + " " + error.reason);
		};

		localDB = new LocalDB((db, collectionName) => {
			return new JsonObjectCollection(db, collectionName);
		}, ddpConnection);

		friendCollection = (JsonObjectCollection) localDB.GetCollection("friends");
		friendCollection.OnAdded += (id, fields) => {
			Debug.Log("Added docId " + id);
		};

		friendCollection.OnRemoved += (id) => {
			Debug.Log("Removed docId " + id);
		};

		friendCollection.OnChanged += (id, fields, cleared) => {
			Debug.Log("Changed docId " + id +
				" fields: " + fields +
				" cleared:" + cleared);
		};
	}

	private Subscription friendSub;

	public void Update() {
		if (Input.GetKeyDown(KeyCode.C)) {
			Debug.Log("Connecting ...");
			ddpConnection.ConnectAsync();
		}

		if (Input.GetKeyDown(KeyCode.V)) {
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
			ddpConnection.Call("friends.create", JSONObject.CreateStringObject("Coco"));
		}

		if (Input.GetKeyDown(KeyCode.D)) {
			foreach (var entry in friendCollection.documents) {
				Debug.Log(entry.Key + " " + entry.Value);
			}
		}

		if (Input.GetKeyDown(KeyCode.O)) {
			JSONObject parents = new JSONObject();
			parents.AddField("mother", "wonder woman");
			parents.AddField("father", "batman");
			JSONObject attr = new JSONObject();
			attr.AddField("age", 24);
			attr.AddField("height", 180);
			attr.AddField("parents", parents);
			ddpConnection.Call("friends.addAttributes", JSONObject.StringObject("Coco"), attr);
		}

		if (Input.GetKeyDown(KeyCode.P)) {
			JSONObject attr = new JSONObject();
			attr.AddField("age", 1);
			attr.AddField("height", 1);
			attr.AddField("parents.mother", 1);
			ddpConnection.Call("friends.removeAttributes", JSONObject.StringObject("Coco"), attr);
		}

	}

}
