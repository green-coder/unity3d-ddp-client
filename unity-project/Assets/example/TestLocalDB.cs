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
