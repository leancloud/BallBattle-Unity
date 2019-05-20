using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Threading.Tasks;

namespace LeanCloud.Play.Test
{
    public class LobbyTest
    {
        [UnityTest, Timeout(100000)]
        public IEnumerator RoomListUpdate()
        {
            Logger.LogDelegate += Utils.Log;

            var f = false;
            var c0 = Utils.NewClient("lt0_0");
            var c1 = Utils.NewClient("lt0_1");
            var c2 = Utils.NewClient("lt0_2");
            var c3 = Utils.NewClient("lt0_3");

            c0.Connect().OnSuccess(_ => {
                c0.OnLobbyRoomListUpdated += roomList => {
                    Debug.Log($"the count of rooms is {roomList.Count}");
                    f = roomList.Count >= 3;
                };
                return c0.JoinLobby();
            }).Unwrap().OnSuccess(_ => {
                return c1.Connect();
            }).Unwrap().OnSuccess(_ => {
                return c1.CreateRoom();
            }).Unwrap().OnSuccess(_ => {
                return c2.Connect();
            }).Unwrap().OnSuccess(_ => {
                return c2.CreateRoom();
            }).Unwrap().OnSuccess(_ => {
                return c3.Connect();
            }).Unwrap().OnSuccess(_ => {
                return c3.CreateRoom();
            }).Unwrap().OnSuccess(_ => {
                Debug.Log("create dones");
            });
                
            while (!f) {
                yield return null;
            }
            c0.Close();
            c1.Close();
            c2.Close();
            c3.Close();
        }
    }
}
