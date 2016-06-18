using UnityEngine;
using System.Threading.Collections;
using System;
using System.Collections;

public class CoroutineHelper : MonoBehaviour {

	private static CoroutineHelper coroutineHelper;

	public static CoroutineHelper GetInstance() {
		if (coroutineHelper == null) {
			GameObject gameObject = new GameObject();
			gameObject.name = "CoroutineHelper";
			coroutineHelper = gameObject.AddComponent<CoroutineHelper>();
		}
		return coroutineHelper;
	}

	private ConcurrentQueue<Action> actionQueue = new ConcurrentQueue<Action>();

	public void RunInMainThread(Action action) {
		actionQueue.Enqueue(action);
	}

	public void LaunchFromMainThread(IEnumerator coroutine) {
		actionQueue.Enqueue(() => StartCoroutine(coroutine));
	}

	public void RunInMainThreadAfter(Action action, float delay) {
		LaunchFromMainThread(RunAfter(action, delay));
	}

	public IEnumerator RunAfter(Action action, float delay) {
		yield return new WaitForSeconds(delay);
		action();
	}

	public void Update() {
		Action action = null;
		while (actionQueue.TryDequeue(out action)) {
			action();
		}
	}

}
