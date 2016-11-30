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
using System;
using System.Collections;
using System.Collections.Generic;

namespace Moulin.DDP {

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
