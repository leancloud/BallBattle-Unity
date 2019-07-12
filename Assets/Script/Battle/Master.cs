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
        battle.IdToFoods = new Dictionary<int, FoodBeh>();
        battle.IdToBalls = new Dictionary<int, BallBeh>();
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
        // 开始定时生成食物
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

    void OnBallAndFoodCollision(FoodCollisionArgs args) {
        var ball = args.Ball;
        var food = args.FoodBeh;
        food.gameObject.SetActive(false);
        // 增加体重
        var player = ball.Player;
        var weight = player.CustomProperties.GetFloat("weight") + Constants.FOOD_WEIGHT;
        var props = new PlayObject {
            { "weight", weight }
        };
        player.SetCustomProperties(props);
        // 通知事件
        var eventData = new PlayObject {
            { "pId", player.ActorId },
            { "fId", food.Data.Id }
        };
        var client = LeanCloudUtils.GetClient();
        client.SendEvent(Constants.EAT_EVENT, eventData);
    }

    void OnBallAndBallCollision(BallCollisionArgs args) {
        var ball1 = args.Ball1;
        var ball2 = args.Ball2;
        // 判断胜负
        var weight1 = ball1.Player.CustomProperties.GetFloat("weight");
        var weight2 = ball2.Player.CustomProperties.GetFloat("weight");
        Player winner, loser;
        if (weight1 > weight2) {
            winner = ball1.Player;
            loser = ball2.Player;
        } else {
            winner = ball2.Player;
            loser = ball1.Player;
        }
        var winnerWeight = weight1 + weight2;
        var props = new PlayObject {
            { "weight", winnerWeight }
        };
        // 设置胜利方
        winner.SetCustomProperties(props);
        // 重置失败方
        var loserWeight = Mathf.Pow(Constants.BORN_SIZE, 2);
        var pos = BattleHelper.RandomPos();
        props = new PlayObject {
            { "pos", pos },
            { "weight", loserWeight },
            { "move", null }
        };
        loser.SetCustomProperties(props);
        // 通知胜负情况
        var client = LeanCloudUtils.GetClient();
        var eventData = new PlayObject {
            { "winnerId", winner.ActorId },
            { "loserId", loser.ActorId }
        };
        client.SendEvent(Constants.KILL_EVENT, eventData);
        // 通知重生
        eventData = new PlayObject {
            { "playerId", loser.ActorId }
        };
        client.SendEvent(Constants.REBORN_EVENT, eventData);
    }

    IEnumerator StartSpawnFoods() { 
        while (true) {
            var spawnFoodCount = Constants.INIT_FOOD_COUNT - battle.IdToFoods.Count;
            // 获取最大食物 id
            PlayArray spawnFoods = new PlayArray();
            var nextFoodId = battle.NextFoodId;
            for (int i = 0; i < spawnFoodCount; i++) {
                var foodId = nextFoodId + i;
                var foodPos = BattleHelper.RandomPos();
                var food = new Food { 
                    Id = foodId,
                    Type = i % 3,
                    X = foodPos.X,
                    Y = foodPos.Y
                };
                spawnFoods.Add(food);
            }
            var eventData = new PlayObject {
                { "foods", spawnFoods },
                { "nextFoodId", nextFoodId + spawnFoodCount }
            };
            var client = LeanCloudUtils.GetClient();
            client.SendEvent(Constants.SPAWN_FOOD_EVENT, eventData);
            yield return new WaitForSeconds(Constants.SPWAN_FOOD_DURATION);
        }
    }

    async void NewPlayer(Player player) {
        var weight = Mathf.Pow(Constants.BORN_SIZE, 2);
        var pos = BattleHelper.RandomPos();
        var props = new PlayObject {
            { "weight", weight },
            { "pos", pos }
        };
        await player.SetCustomProperties(props);
        var client = LeanCloudUtils.GetClient();
        // 打包内存中的食物数据
        var foods = battle.GetFoods();
        var eventData = new PlayObject {
            { "foods", foods }
        };
        await client.SendEvent(Constants.BORN_EVENT, eventData, new SendEventOptions { 
            TargetActorIds = new List<int> { player.ActorId }
        });
        // 告知「其他玩家」有新玩家加入
        var otherIds = new List<int>();
        foreach (Player p in client.Room.PlayerList) { 
            if (p == player) {
                continue;
            }
            otherIds.Add(p.ActorId);
        }
        eventData = new PlayObject {
            { "pId", player.ActorId }
        };
        await client.SendEvent(Constants.PLAYER_JOINED_EVENT, eventData, new SendEventOptions {
            TargetActorIds = otherIds
        });
    }
}
