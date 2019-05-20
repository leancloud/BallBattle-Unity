using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCloud.Play {
    internal class PlayContext {
        readonly Queue<Action> actions;
        readonly AutoResetEvent are;

        internal bool IsRunning {
            get; set;
        }

        internal PlayContext() {
            actions = new Queue<Action>();
            are = new AutoResetEvent(false);
            Task.Run(() => {
                IsRunning = true;
                while (IsRunning) {
                    if (actions.Count > 0) {
                        Action action = null;
                        lock (actions) {
                            action = actions.Dequeue();
                        }
                        action?.Invoke();
                    } else {
                        are.WaitOne();
                    }
                }
            });
        }

        internal void Post(Action action) { 
            if (action == null) {
                return;
            }
            lock (actions) {
                actions.Enqueue(action);
                are.Set();
            }
        }
    }
}
