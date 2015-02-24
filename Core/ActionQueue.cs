using System;
using System.Collections.Generic;
using Castle.Core;
using Core.Exceptions;
using Action = Core.Actions.Action;

namespace Core {
    [CastleComponent("Core.ActionQueue", typeof (ActionQueue), Lifestyle = LifestyleType.Singleton)]
    public class ActionQueue {
        internal readonly Queue<Action> Queue;

        public ActionQueue() {
            Queue = new Queue<Action>();
        }

        public int Count {
            get { return Queue.Count; }
        }

        public virtual void Push(Action action) {
            if (action == null) throw new InvalidActionException();
            Queue.Enqueue(action);
        }

        internal Action Pop() {
            try {
                return Queue.Dequeue();
            }
            catch (InvalidOperationException) {
                return null;
            }
        }

        internal void Clear() {
            Queue.Clear();
        }
    }
}