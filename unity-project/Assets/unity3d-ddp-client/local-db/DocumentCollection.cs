
public interface DocumentCollection {
	void Add(string docId, JSONObject fields);
	void Change(string docId, JSONObject fields, JSONObject cleared);
	void Remove(string docId);
	void AddBefore(string docId, JSONObject fields, string before);
	void MoveBefore(string docId, string before);
}
