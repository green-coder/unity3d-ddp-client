using System;
using System.Collections;
using System.Collections.Generic;

namespace DDP {

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
