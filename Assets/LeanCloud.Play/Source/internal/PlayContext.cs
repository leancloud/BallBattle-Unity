using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LeanCloud.Play {
    internal class PlayContext : MonoBehaviour {
        Queue<Action> runningActions;
        Queue<Action> waitingActions;

        bool running;

        void Awake() {
            runningActions = new Queue<Action>();
            waitingActions = new Queue<Action>();
            running = true;
        }

        void Update() {
            if (!running) {
                return;
            }
            if (waitingActions.Count > 0) { 
                lock (waitingActions) {
                    var temp = runningActions;
                    runningActions = waitingActions;
                    waitingActions = temp;
                }
                while (runningActions.Count > 0) {
                    var action = runningActions.Dequeue();
                    action.Invoke();
                    if (!running) { 
                        lock (waitingActions) {
                            var temp = waitingActions;
                            waitingActions = runningActions;
                            foreach (var act in temp) {
                                waitingActions.Enqueue(act);
                            }
                        }
                    }
                }
            }
        }

        internal void Post(Action action) { 
            if (action == null) {
                return;
            }
            lock (waitingActions) {
                waitingActions.Enqueue(action);
            }
        }

        internal void Pause() {
            running = false;
        }

        internal void Resume() {
            running = true;
        }
    }
}
