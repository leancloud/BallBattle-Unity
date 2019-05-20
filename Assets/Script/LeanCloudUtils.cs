using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LeanCloud.Play;

public static class LeanCloudUtils
{
    static Client client;

    public static Client InitClient(string userId) {
        client = new Client("vAGmhiMWKL36JMXdepqx3sgV-gzGzoHsz", "Gt9CnVkM20XGFkAFkEkCKULE", userId);
        return client;
    }

    public static Client GetClient() {
        return client;
    }
}
