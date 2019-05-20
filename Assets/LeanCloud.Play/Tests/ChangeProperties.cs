using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Threading.Tasks;
using System;

namespace LeanCloud.Play.Test
{
    public class ChangeProperties
    {
        [UnityTest]
        public IEnumerator ChangeRoomProperties() {
            Logger.LogDelegate += Utils.Log;

            var roomName = "cp0_r";
            var f0 = false;
            var f1 = false;
            var c0 = Utils.NewClient("cp0_0");
            var c1 = Utils.NewClient("cp0_1");

            c0.Connect().OnSuccess(_ => {
                return c0.CreateRoom(roomName);
            }).Unwrap().OnSuccess(_ => {
                c0.OnRoomCustomPropertiesChanged += changedProps => {
                    var props = c0.Room.CustomProperties;
                    Assert.AreEqual(props["name"] as string, "leancloud");
                    Assert.AreEqual(int.Parse(props["gold"].ToString()), 1000);
                    f0 = true;
                };
                return c1.Connect();
            }).Unwrap().OnSuccess(_ => {
                return c1.JoinRoom(roomName);
            }).Unwrap().OnSuccess(_ => {
                c1.OnRoomCustomPropertiesChanged += changedProps => {
                    var props = c1.Room.CustomProperties;
                    Assert.AreEqual(props["name"] as string, "leancloud");
                    Assert.AreEqual(int.Parse(props["gold"].ToString()), 1000);
                    f1 = true;
                };
                var newProps = new Dictionary<string, object> {
                    { "name", "leancloud" },
                    { "gold", 1000 },
                };
                return c0.SetRoomCustomProperties(newProps);
            });

            while (!f0 || !f1) {
                yield return null;
            }
            c0.Close();
            c1.Close();
        }

        [Test]
        public async void ChangeRoomPropertiesWithCAS() {
            Logger.LogDelegate += Utils.Log;

            Debug.Log("hello");
            var roomName = "cp1_r";
            var c = Utils.NewClient("cp1");
            await c.Connect();
            var options = new RoomOptions { 
                CustomRoomProperties = new Dictionary<string, object> {
                    { "id", 1 },
                    { "gold", 100 }
                }
            };
            var room = await c.CreateRoom(roomName);

            var newProps = new Dictionary<string, object> {
                    { "gold", 200 },
                };
            var expectedValues = new Dictionary<string, object> {
                    { "id", 2 }
                };

            await c.SetRoomCustomProperties(newProps, expectedValues);

            c.Close();
        }

        [UnityTest]
        public IEnumerator ChangePlayerProperties() {
            Logger.LogDelegate += Utils.Log;

            var f0 = false;
            var f1 = false;
            var roomName = "cp2_r";
            var c0 = Utils.NewClient("cp2_0");
            var c1 = Utils.NewClient("cp2_1");
            c0.Connect().OnSuccess(_ => {
                return c0.CreateRoom(roomName);
            }).Unwrap().OnSuccess(_ => {
                c0.OnPlayerCustomPropertiesChanged += (player, changedProps) => {
                    var props = player.CustomProperties;
                    Assert.AreEqual(props["nickname"] as string, "LeanCloud");
                    Assert.AreEqual(int.Parse(props["gold"].ToString()), 100);
                    var attr = props["attr"] as Dictionary<string, object>;
                    Assert.AreEqual(int.Parse(attr["hp"].ToString()), 10);
                    Assert.AreEqual(int.Parse(attr["mp"].ToString()), 20);
                    Debug.Log("c0 check done");
                    f0 = true;
                };
                return c1.Connect();
            }).Unwrap().OnSuccess(_ => {
                return c1.JoinRoom(roomName);
            }).Unwrap().OnSuccess(_ => {
                c1.OnPlayerCustomPropertiesChanged += (player, changedProps) => {
                    var p = player.CustomProperties;
                    Assert.AreEqual(p["nickname"] as string, "LeanCloud");
                    Assert.AreEqual(int.Parse(p["gold"].ToString()), 100);
                    var attr = p["attr"] as Dictionary<string, object>;
                    Assert.AreEqual(int.Parse(attr["hp"].ToString()), 10);
                    Assert.AreEqual(int.Parse(attr["mp"].ToString()), 20);
                    Debug.Log("c1 check done");
                    f1 = true;
                };
                var props = new Dictionary<string, object> {
                    { "nickname", "LeanCloud" },
                    { "gold", 100 },
                    { "attr", new Dictionary<string, object> {
                            { "hp", 10 },
                            { "mp", 20 }
                        } 
                    }
                };
                return c0.SetPlayerCustomProperties(c1.Player.ActorId, props);
            });

            while (!f0 || !f1) {
                yield return null;
            }
            c0.Close();
            c1.Close();
        }

        [Test]
        public async void ChangePlayerPropertiesWithCAS() {
            Logger.LogDelegate += Utils.Log;

            var roomName = "cp3_r";
            var c = Utils.NewClient("cp3");

            await c.Connect();
            await c.CreateRoom(roomName);
            var props = new Dictionary<string, object> {
                { "id", 1 },
                { "nickname", "lean" }
            };
            await c.Player.SetCustomProperties(props);

            var newProps = new Dictionary<string, object> {
                { "nickname", "cloud" }
            };
            var expectedValues = new Dictionary<string, object> {
                { "id", 2 }
            };
            await c.Player.SetCustomProperties(newProps, expectedValues);
            c.Close();
        }

        [Test]
        public async void GetPlayerPropertiesWhenJoinRoom() {
            var roomName = "cp4_r";
            var c0 = Utils.NewClient("cp4_0");
            var c1 = Utils.NewClient("cp4_1");

            await c0.Connect();
            await c0.CreateRoom(roomName);
            var props = new Dictionary<string, object> {
                { "ready", true }
            };
            await c0.Player.SetCustomProperties(props);

            await c1.Connect();
            await c1.JoinRoom(roomName);
            var master = c1.Room.Master;

            Assert.AreEqual(bool.Parse(master.CustomProperties["ready"].ToString()), true);

            c0.Close();
            c1.Close();
        }

        [Test]
        public async void ChangePropertiesWithSameValue() {
            Logger.LogDelegate += Utils.Log;

            var roomName = "cp5_r";
            var c = Utils.NewClient("cp5");

            await c.Connect();
            await c.CreateRoom(roomName);
            var props = new Dictionary<string, object> {
                { "ready", true }
            };
            await c.Room.SetCustomProperties(props);
            await c.Room.SetCustomProperties(props);
            await c.Player.SetCustomProperties(props);
            await c.Player.SetCustomProperties(props);
            c.Close();
        }
    }
}