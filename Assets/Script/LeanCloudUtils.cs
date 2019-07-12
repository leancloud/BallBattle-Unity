using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LeanCloud.Play;

public static class LeanCloudUtils
{
    static Client client;

    public static Client InitClient(string userId) {
        // 注册自定义类型的序列化
        CodecUtils.RegisterType(typeof(Food), 2, Food.Serialize, Food.Deserialize);
        CodecUtils.RegisterType(typeof(Move), 3, Move.Serialize, Move.Deserialize);
        CodecUtils.RegisterType(typeof(Vec2), 4, Vec2.Serialize, Vec2.Deserialize);
        client = new Client("vAGmhiMWKL36JMXdepqx3sgV-gzGzoHsz", "Gt9CnVkM20XGFkAFkEkCKULE", userId);
        //client = new Client("FQr8l8LLvdxIwhMHN77sNluX-9Nh9j0Va", "MJSm46Uu6LjF5eNmqfbuUmt6", userId);
        client.OnPlayerCustomPropertiesChanged += (player, changedProps) => {
            Debug.Log(changedProps);
        };
        return client;
    }

    public static Client GetClient() {
        return client;
    }
}
