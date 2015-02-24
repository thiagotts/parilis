using System;
using System.Collections.Generic;
using Castle.Core;
using Core.Exceptions;
using Action = Core.Actions.Action;

namespace Core {
    [CastleComponent("Core.ActionQueue", typeof (ActionQueue), Lifestyle = LifestyleType.Singleton)]
    public class ActionQueue {
        private readonly Queue<Action> queue;

        public ActionQueue() {
            queue = new Queue<Action>();
        }

        public int Count { get { return queue.Count; } }

        internal void Push(Action action) {
            if (action == null) throw new InvalidActionException();
            queue.Enqueue(action);
        }

        internal Action Pop() {
            try {
                return queue.Dequeue();
            }
            catch (InvalidOperationException) {
                return null;
            }
        }

        internal void Clear() {
            queue.Clear();
        }
    }
}