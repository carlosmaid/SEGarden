﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VRage;
using VRage.Collections;

namespace SEGarden.Threading {

    /// <summary>
    /// This comes straight from Autopilot
    /// https://github.com/Rynchodon/Autopilot
    /// </summary>
    public class LockedQueue<T> {
        private readonly MyQueue<T> Queue;
        private readonly FastResourceLock lock_Queue = new FastResourceLock();

        public LockedQueue(IEnumerable<T> collection) { Queue = new MyQueue<T>(collection); }

        public LockedQueue(int capacity = 1) { Queue = new MyQueue<T>(capacity); }

        public int Count {
            get {
                using (lock_Queue.AcquireSharedUsing())
                    return Queue.Count;
            }
        }

        public T this[int index] {
            get {
                using (lock_Queue.AcquireSharedUsing())
                    return Queue[index];
            }
            set {
                using (lock_Queue.AcquireExclusiveUsing())
                    Queue[index] = value;
            }
        }

        public void Clear() {
            using (lock_Queue.AcquireExclusiveUsing())
                Queue.Clear();
        }

        public bool TryDequeue(out T value) {
            using (lock_Queue.AcquireExclusiveUsing())
                if (Queue.Count != 0) {
                    value = Queue.Dequeue();
                    return true;
                }

            value = default(T);
            return false;
        }

        public void Enqueue(T item) {
            using (lock_Queue.AcquireExclusiveUsing())
                Queue.Enqueue(item);
        }

        public bool TryPeek(out T value) {
            using (lock_Queue.AcquireSharedUsing())
                if (Queue.Count != 0) {
                    value = Queue.Peek();
                    return true;
                }

            value = default(T);
            return false;
        }

        public void TrimExcess() {
            using (lock_Queue.AcquireExclusiveUsing())
                Queue.TrimExcess();
        }

        public void DequeueAll(Action<T> invoke) {
            using (lock_Queue.AcquireExclusiveUsing())
                while (Queue.Count > 0)
                    invoke(Queue.Dequeue());
        }

        public void ForEach(Action<T> invoke) {
            using (lock_Queue.AcquireSharedUsing())
                for (int i = 0; i < Queue.Count; i++)
                    invoke(Queue[i]);
        }
    }
}