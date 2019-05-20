using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Threading.Tasks;

namespace LeanCloud.Play.Test
{
    public class KickTest
    {
        [UnityTest]
        public IEnumerator Kick() {
            Logger.LogDelegate += Utils.Log;

            var flag = false;
            var roomName = "kt0_r";
            var c0 = Utils.NewClient("kt0_0");
            var c1 = Utils.NewClient("kt0_1");
            c0.Connect().OnSuccess(_ => {
                return c0.CreateRoom(roomName);
            }).Unwrap().OnSuccess(_ => {
                c0.OnPlayerRoomJoined += newPlayer => {
                    c0.KickPlayer(newPlayer.ActorId);
                };
                return c1.Connect();
            }).Unwrap().OnSuccess(_ => {
                c1.OnRoomKicked += (code, msg) => {
                    Debug.Log($"{c1.UserId} is kicked");
                    Assert.AreEqual(code, 0);
                    flag = true;
                };
                return c1.JoinRoom(roomName);
            }).Unwrap().OnSuccess(_ => {
                Debug.Log($"{c1.UserId} joined room");
            });

            while (!flag) {
                yield return null;
            }
            c0.Close();
            c1.Close();
        }

        [UnityTest]
        public IEnumerator KickWithMsg() {
            Logger.LogDelegate += Utils.Log;

            var flag = false;
            var roomName = "kt1_r";
            var c0 = Utils.NewClient("kt1_0");
            var c1 = Utils.NewClient("kt1_1");
            c0.Connect().OnSuccess(_ => {
                return c0.CreateRoom(roomName);
            }).Unwrap().OnSuccess(_ => {
                c0.OnPlayerRoomJoined += newPlayer => {
                    c0.KickPlayer(newPlayer.ActorId, 404, "You cheat!");
                };
                return c1.Connect();
            }).Unwrap().OnSuccess(_ => {
                c1.OnRoomKicked += (code, msg) => {
                    Assert.AreEqual(code, 404);
                    Debug.Log($"{c1.UserId} is kicked for {msg}");
                    flag = true;
                };
                return c1.JoinRoom(roomName);
            }).Unwrap().OnSuccess(_ => {
                Debug.Log($"{c1.UserId} joined room");
            });

            while (!flag) {
                yield return null;
            }
            c0.Close();
            c1.Close();
        }
    }
}
