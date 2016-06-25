using UnityEngine;
using System.Collections;

public class TestLocalDB : MonoBehaviour {

	public string serverUrl = "ws://localhost:3000/websocket";
	public bool logMessages;

	private DdpConnection ddpConnection;
	private LocalDB localDB;
	private JsonObjectCollection friendCollection;

	public void Start() {
		ddpConnection = new DdpConnection(serverUrl);
		ddpConnection.logMessages = logMessages;

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
			ddpConnection.Connect();
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
			MethodCall methodCall = ddpConnection.Call("friends.create", JSONObject.CreateStringObject("Coco"));
			methodCall.OnUpdated = (MethodCall obj) => {
				Debug.Log("Updated, methodId=" + obj.id);
			};
			methodCall.OnResult = (MethodCall obj) => {
				Debug.Log("Result = " + obj.result);
			};
		}

		if (Input.GetKeyDown(KeyCode.D)) {
			foreach (var entry in friendCollection.documents) {
				Debug.Log(entry.Key + " " + entry.Value);
			}
		}

	}

}
