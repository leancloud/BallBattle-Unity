using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LeanCloud.Play;

public class Master : MonoBehaviour {
    Battle battle;

    /// <summary>
    /// 房主进入房间时调用
    /// </summary>
    public void InitGame() {
        battle = GetComponent<Battle>();
        battle.IdToFoods = new Dictionary<int, Food>();
        battle.IdToBalls = new Dictionary<int, Ball>();
        // 初始化玩家
        var client = LeanCloudUtils.GetClient();
        NewPlayer(client.Player);
        // 开始定时生成小球
        StartCoroutine(StartSpawnFoods());
    }

    /// <summary>
    /// 在房主切换时调用
    /// </summary>
    public void SwitchGame() {
        battle = GetComponent<Battle>();
        // 开始定时生成小球
        StartCoroutine(StartSpawnFoods());
    }

    void Start() {
        var client = LeanCloudUtils.GetClient();
        client.OnPlayerRoomJoined += OnPlayerRoomJoined;
    }

    void OnDestroy() {
        var client = LeanCloudUtils.GetClient();
        client.OnPlayerRoomJoined -= OnPlayerRoomJoined;
        StopAllCoroutines();
    }

    void OnPlayerRoomJoined(Player player) {
        NewPlayer(player);
    }

    void OnBallAndFoodCollision(Dictionary<string, object> args) {
        var ball = args["ball"] as Ball;
        var food = args["food"] as Food;
        food.gameObject.SetActive(false);
        // 增加体重
        var player = ball.Player;
        var weight = int.Parse(player.CustomProperties["weight"].ToString()) + Constants.FOOD_WEIGHT;
        var props = new Dictionary<string, object> {
            { "weight", weight }
        };
        player.SetCustomProperties(props);
        // 通知事件
        var eventData = new Dictionary<string, object> {
            { "pId", player.ActorId },
            { "fId", food.Id }
        };
        var client = LeanCloudUtils.GetClient();
        client.SendEvent(Constants.EAT_EVENT, eventData);
    }

    void OnBallAndBallCollision(Dictionary<string, Ball> args) {
        var ball1 = args["b1"];
        var ball2 = args["b2"];
        // 判断胜负
        var weight1 = int.Parse(ball1.Player.CustomProperties["weight"].ToString());
        var weight2 = int.Parse(ball2.Player.CustomProperties["weight"].ToString());
        Player winner, loser;
        if (weight1 > weight2) {
            winner = ball1.Player;
            loser = ball2.Player;
        } else {
            winner = ball2.Player;
            loser = ball1.Player;
        }
        var winnerWeight = weight1 + weight2;
        var props = new Dictionary<string, object> {
            { "weight", winnerWeight }
        };
        winner.SetCustomProperties(props);
        var client = LeanCloudUtils.GetClient();
        var eventData = new Dictionary<string, object> {
            { "winnerId", winner.ActorId },
            { "loserId", loser.ActorId }
        };
        client.SendEvent(Constants.KILL_EVENT, eventData);
        // 重置失败方
        var loserWeight = Mathf.Pow(Constants.BORN_SIZE, 2);
        var pos = BattleHelper.RandomPos();
        props = new Dictionary<string, object> {
            { "pos", pos },
            { "weight", loserWeight },
            { "move", null }
        };
        loser.SetCustomProperties(props);
        eventData = new Dictionary<string, object> {
            { "playerId", loser.ActorId }
        };
        client.SendEvent(Constants.REBORN_EVENT, eventData);
    }

    IEnumerator StartSpawnFoods() { 
        while (true) {
            var spawnFoodCount = Constants.INIT_FOOD_COUNT - battle.IdToFoods.Count;
            // 获取最大食物 id
            var spawnFoods = new List<object>();
            var nextFoodId = battle.NextFoodId;
            for (int i = 0; i < spawnFoodCount; i++) {
                var foodId = nextFoodId + i;
                var foodPos = BattleHelper.RandomPos();
                var food = new Dictionary<string, object> {
                    { "id", foodId },
                    { "type", i % 3 },
                    { "x", float.Parse(foodPos["x"].ToString()) },
                    { "y", float.Parse(foodPos["y"].ToString()) }
                };
                spawnFoods.Add(food);
            }
            var eventData = new Dictionary<string, object> {
                { "foods", spawnFoods },
                { "nextFoodId", nextFoodId + spawnFoodCount }
            };
            var client = LeanCloudUtils.GetClient();
            client.SendEvent(Constants.SPAWN_FOOD_EVENT, eventData);
            yield return new WaitForSeconds(Constants.SPWAN_FOOD_DURATION);
        }
    }

    void NewPlayer(Player player) {
        var weight = Mathf.Pow(Constants.BORN_SIZE, 2);
        var pos = BattleHelper.RandomPos();
        var props = new Dictionary<string, object> {
            { "weight", weight },
            { "pos", pos }
        };
        player.SetCustomProperties(props);
        var client = LeanCloudUtils.GetClient();
        // 打包内存中的食物数据
        var foods = battle.GetFoods();
        var eventData = new Dictionary<string, object> {
            { "foods", foods }
        };
        client.SendEvent(Constants.BORN_EVENT, eventData, new SendEventOptions { 
            targetActorIds = new List<int> { player.ActorId }
        });
        // 告知其他玩家有新玩家加入
        var otherIds = new List<int>();
        foreach (Player p in client.Room.PlayerList) { 
            if (p == player) {
                continue;
            }
            otherIds.Add(p.ActorId);
        }
        eventData = new Dictionary<string, object> {
            { "pId", player.ActorId }
        };
        client.SendEvent(Constants.PLAYER_JOINED_EVENT, eventData, new SendEventOptions {
            targetActorIds = otherIds
        });
    }
}
