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

	public class TypedCollection<DocType> : DocumentCollection {

		//private LocalDb db;
		private Func<JSONObject, DocType> JSONObjectToDocument;
		private Func<DocType, JSONObject> DocumentToJSONObject;

		public string collectionName;
		public Dictionary<string, DocType> documents = new Dictionary<string, DocType>();

		public delegate void OnAddedDelegate(DocType document);
		public delegate void OnChangedDelegate(DocType oldDocument, DocType newDocument);
		public delegate void OnRemovedDelegate(DocType document);

		public event OnAddedDelegate OnAdded;
		public event OnChangedDelegate OnChanged;
		public event OnRemovedDelegate OnRemoved;

		public TypedCollection(LocalDB db, string collectionName,
							Func<JSONObject, DocType> JSONObjectToDocument,
							Func<DocType, JSONObject> DocumentToJSONObject) {
			//this.db = db;
			this.collectionName = collectionName;
			this.JSONObjectToDocument = JSONObjectToDocument;
			this.DocumentToJSONObject = DocumentToJSONObject;
		}

		public void Add(string docId, JSONObject fields) {
			DocType document = JSONObjectToDocument(fields);
			if (OnAdded != null) {
				OnAdded(document);
			}
			documents.Add(docId, document);
		}

		public void Change(string docId, JSONObject fields, JSONObject cleared) {
			JSONObject jsonDocument = DocumentToJSONObject(documents[docId]);

			DocType oldDocument = default(DocType);
			if (OnChanged != null) {
				oldDocument = JSONObjectToDocument(jsonDocument.Copy());
			}

			if (fields != null) {
				foreach (string field in fields.keys) {
					jsonDocument.SetField(field, fields[field]);
				}
			}

			if (cleared != null) {
				foreach (JSONObject field in cleared.list) {
					jsonDocument.RemoveField(field.str);
				}
			}

			DocType newDocument = JSONObjectToDocument(jsonDocument);

			if (OnChanged != null) {
				OnChanged(oldDocument, newDocument);
			}

			documents[docId] = newDocument;
		}

		public void Remove(string docId) {
			DocType document = documents[docId];

			if (OnRemoved != null) {
				OnRemoved(document);
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

}
