using UnityEngine;
using System;
using System.Collections;

public class Subscription {
	public string id;
	public string name;
	public JSONObject[] items;
	public bool isReady;
	public Action<Subscription> OnReady;
}
