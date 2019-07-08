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
    Dictionary<int, BallInfo> ballInfoDict;

    public void Init() {
        var client = LeanCloudUtils.GetClient();
        playerInfoList = new List<PlayerInfoItem>();
        ballInfoDict = new Dictionary<int, BallInfo>();
        StartCoroutine(Tick());
    }

    public void AddPlayerInfo(BallBeh ball) {
        var ballInfoGO = Instantiate(ballInfoTemplete);
        ballInfoGO.transform.parent = transform;
        var ballInfo = ballInfoGO.GetComponent<BallInfo>();
        ballInfo.ball = ball;
        ballInfoDict.Add(ball.Id, ballInfo);

        NewPlayerInfoItem();
    }

    public void RemovePlayerInfo(BallBeh ball) {
        var playerInfoItem = playerInfoList[0];
        playerInfoList.RemoveAt(0);
        Destroy(playerInfoItem.gameObject);

        var ballInfo = ballInfoDict[ball.Id];
        ballInfoDict.Remove(ball.Id);
        Destroy(ballInfo.gameObject);
    }

    public void UpdateList() {
        var client = LeanCloudUtils.GetClient();
        var playerList = client.Room.PlayerList;
        playerList.Sort((p1, p2) => {
            var w1 = p1.CustomProperties.GetInt("weight");
            var w2 = p2.CustomProperties.GetInt("weight");
            return w2 - w1;
        });
        for (int i = 0; i < playerList.Count; i++) {
            var player = playerList[i];
            var playerInfoItem = playerInfoList[i];
            var weight = player.CustomProperties.GetInt("weight");
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
