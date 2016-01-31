using UnityEngine;
using System.Collections.Generic;

public class ConcurrentQueue<T> {

	private object locker = new object();
	private readonly Queue<T> q = new Queue<T> ();

	public void Enqueue(T item) {
		lock (locker) {
			q.Enqueue(item);
		}
	}

	public T Dequeue() {
		lock (locker) {
			return q.Dequeue();
		}
	}

	public int Count {
		get {
			lock (locker) {
				return q.Count;
			}
		}
	}

}
