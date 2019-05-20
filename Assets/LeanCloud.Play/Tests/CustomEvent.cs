using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Threading.Tasks;

namespace LeanCloud.Play.Test
{
    public class CustomEvent
    {
        [UnityTest]
        public IEnumerator CustomEventWithReceiverGroup() {
            Logger.LogDelegate += Utils.Log;

            var f = false;
            var roomName = "ce0_r";
            var c0 = Utils.NewClient("ce0_0");
            var c1 = Utils.NewClient("ce0_1");

            c0.Connect().OnSuccess(_ => {
                return c0.CreateRoom(roomName);
            }).Unwrap().OnSuccess(_ => {
                c0.OnCustomEvent += (eventId, eventData, senderId) => {
                    Assert.AreEqual(eventId, 1);
                    Assert.AreEqual(eventData["name"] as string, "aaa");
                    Assert.AreEqual(int.Parse(eventData["count"].ToString()), 100);
                    f = true;
                };
                return c1.Connect();
            }).Unwrap().OnSuccess(_ => {
                return c1.JoinRoom(roomName);
            }).Unwrap().OnSuccess(_ => {
                var eventData = new Dictionary<string, object> {
                    { "name", "aaa" },
                    { "count", 100 },
                };
                var options = new SendEventOptions { 
                    ReceiverGroup = ReceiverGroup.MasterClient
                };
                return c1.SendEvent(1, eventData, options);
            }).Unwrap().OnSuccess(_ => {
                Debug.Log("send event done");
            });

            while (!f) {
                yield return null;
            }
            c0.Close();
            c1.Close();
        }

        [UnityTest]
        public IEnumerator CustomEventWithTargetIds() {
            Logger.LogDelegate += Utils.Log;

            var f = false;
            var roomName = "ce1_r";
            var c0 = Utils.NewClient("ce1_0");
            var c1 = Utils.NewClient("ce1_1");

            c0.Connect().OnSuccess(_ => {
                return c0.CreateRoom(roomName);
            }).Unwrap().OnSuccess(_ => {
                c0.OnCustomEvent += (eventId, eventData, senderId) => {
                    Assert.AreEqual(eventId, 2);
                    Assert.AreEqual(eventData["name"] as string, "aaa");
                    Assert.AreEqual(int.Parse(eventData["count"].ToString()), 100);
                    f = true;
                };
                return c1.Connect();
            }).Unwrap().OnSuccess(_ => {
                return c1.JoinRoom(roomName);
            }).Unwrap().OnSuccess(_ => {
                var eventData = new Dictionary<string, object> {
                    { "name", "aaa" },
                    { "count", 100 },
                };
                var options = new SendEventOptions {
                    targetActorIds = new List<int> { 1, 2 }
                };
                return c1.SendEvent(2, eventData, options);
            }).Unwrap().OnSuccess(_ => {
                Debug.Log("send event done");
            });

            while (!f) {
                yield return null;
            }
            c0.Close();
            c1.Close();
        }


        [UnityTest]
        public IEnumerator SimpleEvent() {
            Logger.LogDelegate += Utils.Log;

            var f0 = false;
            var f1 = false;
            var roomName = "ce2_r";
            var c0 = Utils.NewClient("ce2_0");
            var c1 = Utils.NewClient("ce2_1");

            c0.Connect().OnSuccess(_ => {
                return c0.CreateRoom(roomName);
            }).Unwrap().OnSuccess(_ => {
                c0.OnCustomEvent += (eventId, eventData, senderId) => {
                    Assert.AreEqual(eventId, 3);
                    f0 = true;
                };
                return c1.Connect();
            }).Unwrap().OnSuccess(_ => {
                return c1.JoinRoom(roomName);
            }).Unwrap().OnSuccess(_ => {
                c1.OnCustomEvent += (eventId, eventData, senderId) => {
                    Assert.AreEqual(eventId, 3);
                    f1 = true;
                };
                return c1.SendEvent(3);
            }).Unwrap().OnSuccess(_ => {
                Debug.Log("send event done");
            });

            while (!f0 || !f1) {
                yield return null;
            }
            c0.Close();
            c1.Close();
        }
    }
}
