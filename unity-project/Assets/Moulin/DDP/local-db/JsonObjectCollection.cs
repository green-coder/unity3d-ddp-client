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

﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Moulin.DDP {

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
			OnAdded?.Invoke(docId, fields);
			documents.Add(docId, fields);
		}

		public void Change(string docId, JSONObject fields, JSONObject cleared) {
			OnChanged?.Invoke(docId, fields, cleared);

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
			OnRemoved?.Invoke(docId);
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

}
