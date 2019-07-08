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
        return client;
    }

    public static Client GetClient() {
        return client;
    }
}
