using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Threading.Tasks;

namespace LeanCloud.Play.Test
{
    public class MasterTest {
        [UnityTest]
        public IEnumerator SetNewMaster() {
            var f0 = false;
            var f1 = false;
            var roomName = "mt0_r";
            var c0 = Utils.NewClient("mt0_0");
            var c1 = Utils.NewClient("mt0_1");

            c0.Connect().OnSuccess(_ => {
                return c0.CreateRoom(roomName);
            }).Unwrap().OnSuccess(t => {
                var room = t.Result;
                c0.OnMasterSwitched += newMaster => {
                    Assert.AreEqual(newMaster.ActorId, c1.Player.ActorId);
                    Assert.AreEqual(newMaster.ActorId, c0.Room.MasterActorId);
                    f0 = true;
                };
                return c1.Connect();
            }).Unwrap().OnSuccess(_ => {
                return c1.JoinRoom(roomName);
            }).Unwrap().OnSuccess(_ => {
                c1.OnMasterSwitched += newMaster => {
                    Assert.AreEqual(newMaster.ActorId, c1.Player.ActorId);
                    Assert.AreEqual(newMaster.ActorId, c1.Room.MasterActorId);
                    f1 = true;
                };
                return c0.SetMaster(c1.Player.ActorId);
            }).Unwrap().OnSuccess(_ => {
                Debug.Log("set master done");
            });

            while (!f0 || !f1) {
                yield return null;
            }
            c0.Close();
            c1.Close();
        }

        [UnityTest]
        public IEnumerator MasterLeave() {
            Logger.LogDelegate += Utils.Log;

            var f0 = false;
            var f1 = false;
            var roomName = "mt1_r";
            var c0 = Utils.NewClient("mt1_0");
            var c1 = Utils.NewClient("mt1_1");

            c0.Connect().OnSuccess(_ => {
                return c0.CreateRoom(roomName);
            }).Unwrap().OnSuccess(_ => {
                return c1.Connect();
            }).Unwrap().OnSuccess(_ => {
                return c1.JoinRoom(roomName);
            }).Unwrap().OnSuccess(_ => {
                Assert.AreEqual(c1.Room.MasterActorId, c0.Player.ActorId);
                c1.OnMasterSwitched += newMaster => {
                    Assert.AreEqual(newMaster.ActorId, c1.Player.ActorId);
                    f1 = true;
                };
                return c0.LeaveRoom();
            }).Unwrap().OnSuccess(_ => {
                Debug.Log("leave room done");
                f0 = true;
            });

            while (!f0 || !f1) {
                yield return null;
            }
            c0.Close();
            c1.Close();
        }

        [UnityTest]
        public IEnumerator FixMaster() {
            Logger.LogDelegate += Utils.Log;

            var f0 = false;
            var f1 = false;
            var roomName = "mt2_r";
            var c0 = Utils.NewClient("mt2_0");
            var c1 = Utils.NewClient("mt2_1");

            c0.Connect().OnSuccess(_ => {
                var options = new RoomOptions {
                    Flag = CreateRoomFlag.FixedMaster
                };
                return c0.CreateRoom(roomName, options);
            }).Unwrap().OnSuccess(_ => {
                return c1.Connect();
            }).Unwrap().OnSuccess(_ => {
                return c1.JoinRoom(roomName);
            }).Unwrap().OnSuccess(_ => {
                c1.OnPlayerRoomLeft += leftPlayer => {
                    Assert.AreEqual(leftPlayer.ActorId, c0.Player.ActorId);
                };
                c1.OnMasterSwitched += newMaster => {
                    Assert.AreEqual(c1.Room.MasterActorId, -1);
                    Assert.AreEqual(newMaster, null);
                    f1 = true;
                };
                return c0.LeaveRoom();
            }).Unwrap().OnSuccess(_ => {
                Debug.Log("leave room done");
                f0 = true;
            });

            while (!f0 || !f1) {
                yield return null;
            }
            c0.Close();
            c1.Close();
        }
    }
}
