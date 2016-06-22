using UnityEngine;
using System.Collections;

public class TestDdpAccount : MonoBehaviour {

	public string serverUrl = "ws://localhost:3000/websocket";
	public string username;
	public string password;
	public string token;
	public bool logMessages;

	private DdpConnection ddpConnection;
	private DdpAccount account;

	public void Start() {
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
}
