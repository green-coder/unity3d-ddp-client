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
using System.Threading.Collections;
using System;
using System.Collections;

namespace DDP {

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

}
