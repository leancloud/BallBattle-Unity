using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using LeanCloud.Play;

namespace LeanCloud.Play.Test
{
    public class RouterTest
    {
        [Test]
        public async void PlayServer() {
            Logger.LogDelegate += Utils.Log;

            var appId = "FQr8l8LLvdxIwhMHN77sNluX-9Nh9j0Va";
            var appKey = "MJSm46Uu6LjF5eNmqfbuUmt6";
            var userId = "rt0";
            var playServer = "https://game-router-cn-e1.leancloud.cn/v1";
            var c = new Client(appId, appKey, userId, playServer: playServer);
            await c.Connect();
            await c.CreateRoom();
            c.Close();
        }
    }
}
