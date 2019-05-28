using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LeanCloud.Play;

public class UI : MonoBehaviour
{
    public GameObject playerInfoTemplete;
    public GameObject ballInfoTemplete;

    public Transform rankContentTrans;

    public Text timeText;

    int duration;
    List<PlayerInfoItem> playerInfoList;

    public void Init() {
        var client = LeanCloudUtils.GetClient();
        playerInfoList = new List<PlayerInfoItem>();
        StartCoroutine(Tick());
    }

    public void AddPlayerInfo(Ball ball) {
        var ballInfoGO = Instantiate(ballInfoTemplete);
        ballInfoGO.transform.parent = transform;
        var ballInfo = ballInfoGO.GetComponent<BallInfo>();
        ballInfo.ball = ball;

        NewPlayerInfoItem();
        UpdateList();
    }

    public void RemovePlayerInfo() {
        var playerInfoItem = playerInfoList[0];
        playerInfoList.RemoveAt(0);
        Destroy(playerInfoItem.gameObject);
        UpdateList();
    }

    public void UpdateList() {
        var client = LeanCloudUtils.GetClient();
        var playerList = client.Room.PlayerList;
        playerList.Sort((p1, p2) => {
            var w1 = int.Parse(p1.CustomProperties["weight"].ToString());
            var w2 = int.Parse(p2.CustomProperties["weight"].ToString());
            return w2 - w1;
        });
        for (int i = 0; i < playerList.Count; i++) {
            var player = playerList[i];
            var playerInfoItem = playerInfoList[i];
            var weight = int.Parse(player.CustomProperties["weight"].ToString());
            playerInfoItem.SetInfo(i + 1, player.UserId, weight, player.IsLocal);
        }
    }

    PlayerInfoItem NewPlayerInfoItem() {
        var playerInfoItemGo = Instantiate(playerInfoTemplete);
        playerInfoItemGo.transform.parent = rankContentTrans;
        var playerInfoItem = playerInfoItemGo.GetComponent<PlayerInfoItem>();
        playerInfoList.Add(playerInfoItem);
        return playerInfoItem;
    }

    IEnumerator Tick() { 
        while (true) {
            timeText.text = $"{duration}s";
            duration++;
            yield return new WaitForSeconds(1);
        }
    }

    void OnDestroy() {
        StopAllCoroutines();
    }
}
