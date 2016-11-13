using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DDP {

	public class LocalDB {

		private Func<LocalDB, string, DocumentCollection> CreateCollection;
		private DdpConnection connection;
		private Dictionary<string, DocumentCollection> collections = new Dictionary<string, DocumentCollection>();

		public LocalDB(Func<LocalDB, string, DocumentCollection> CreateCollection) {
			this.CreateCollection = CreateCollection;
		}

		public LocalDB(Func<LocalDB, string, DocumentCollection> CreateCollection, DdpConnection connection) {
			this.CreateCollection = CreateCollection;
			SetConnection(connection);
		}

		public void SetConnection(DdpConnection connection) {
			if (this.connection != null) {
				this.connection.OnAdded -= Add;
				this.connection.OnRemoved -= Remove;
				this.connection.OnChanged -= Change;
				this.connection.OnAddedBefore -= AddBefore;
				this.connection.OnMovedBefore -= MoveBefore;
				this.connection = null;
			}

			if (connection != null) {
				this.connection = connection;
				this.connection.OnAdded += Add;
				this.connection.OnRemoved += Remove;
				this.connection.OnChanged += Change;
				this.connection.OnAddedBefore += AddBefore;
				this.connection.OnMovedBefore += MoveBefore;
			}
		}

		public DocumentCollection GetCollection(string collectionName) {
			if (!collections.ContainsKey(collectionName)) {
				collections[collectionName] = CreateCollection(this, collectionName);
			}

			return collections[collectionName];
		}

		public void Add(string collectionName, string docId, JSONObject fields) {
			GetCollection(collectionName).Add(docId, fields);
		}

		public void Change(string collectionName, string docId, JSONObject fields, JSONObject cleared) {
			GetCollection(collectionName).Change(docId, fields, cleared);
		}

		public void Remove(string collectionName, string docId) {
			GetCollection(collectionName).Remove(docId);
		}

		public void AddBefore(string collectionName, string docId, JSONObject fields, string before) {
			GetCollection(collectionName).AddBefore(docId, fields, before);
		}

		public void MoveBefore(string collectionName, string docId, string before) {
			GetCollection(collectionName).MoveBefore(docId, before);
		}

		public void ClearCollections() {
			collections.Clear();
		}

	}

}
