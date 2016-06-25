using System;
using System.Collections;
using System.Collections.Generic;

public class JsonObjectCollection : DocumentCollection {

	protected LocalDB db;
	public string collectionName;
	public Dictionary<string, JSONObject> documents = new Dictionary<string, JSONObject>();

	public delegate void OnAddedDelegate(string docId, JSONObject fields);
	public delegate void OnChangedDelegate(string docId, JSONObject fields, JSONObject cleared);
	public delegate void OnRemovedDelegate(string docId);

	public event OnAddedDelegate OnAdded;
	public event OnChangedDelegate OnChanged;
	public event OnRemovedDelegate OnRemoved;

	public JsonObjectCollection(LocalDB db, string collectionName) {
		this.db = db;
		this.collectionName = collectionName;
	}

	public void Add(string docId, JSONObject fields) {
		if (OnAdded != null) {
			OnAdded(docId, fields);
		}

		documents.Add(docId, fields);
	}

	public void Change(string docId, JSONObject fields, JSONObject cleared) {
		if (OnChanged != null) {
			OnChanged(docId, fields, cleared);
		}

		JSONObject document = documents[docId];

		if (fields != null) {
			foreach (string field in fields.keys) {
				document.SetField(field, fields[field]);
			}
		}

		if (cleared != null) {
			foreach (JSONObject field in cleared.list) {
				document.RemoveField(field.str);
			}
		}
	}

	public void Remove(string docId) {
		if (OnRemoved != null) {
			OnRemoved(docId);
		}

		documents.Remove(docId);
	}

	public void AddBefore(string docId, JSONObject fields, string before) {
		// For now, collection ordering is not supported.
		Add(docId, fields);
	}

	public void MoveBefore(string docId, string before) {
		// For now, collection ordering is not supported.
	}

}
