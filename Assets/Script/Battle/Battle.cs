using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LeanCloud.Play;

public class Battle : MonoBehaviour
{
    public GameObject ballTemplate;
    public GameObject[] foodTemplates;
    public UI ui;
    public Transform cameraTrans;

    public Dictionary<int, Ball> IdToBalls {
        get; set;
    }

    public Dictionary<int, Food> IdToFoods {
        get; set;
    }

    public int NextFoodId {
        get; set;
    }

    public int Duration {
        get; set;
    }

    void Start() {
        Physics2D.gravity = Vector2.zero;

        IdToBalls = new Dictionary<int, Ball>();
        IdToFoods = new Dictionary<int, Food>();
        var client = LeanCloudUtils.GetClient();
        if (client.Player.IsMaster) {
            var master = gameObject.AddComponent<Master>();
            master.InitGame();
        }
        client.OnMasterSwitched += Client_OnMasterSwitched;
        client.OnCustomEvent += Client_OnCustomEvent;
        client.OnPlayerRoomLeft += Client_OnPlayerRoomLeft;
        client.ResumeMessageQueue();
    }

    void OnDestroy() {
        var client = LeanCloudUtils.GetClient();
        client.OnMasterSwitched -= Client_OnMasterSwitched;
        client.OnCustomEvent -= Client_OnCustomEvent;
        client.OnPlayerRoomLeft -= Client_OnPlayerRoomLeft;
    }

    void Client_OnMasterSwitched(Player newMaster) {
        if (newMaster.IsLocal) {
            // 当前客户端被设置为 Master
            var master = gameObject.AddComponent<Master>();
            master.SwitchGame();
        }
    }

    void Client_OnCustomEvent(byte eventId, Dictionary<string, object> eventData, int senderId) {
        switch (eventId) {
            case Constants.BORN_EVENT:
                OnBornEvent(eventData);
                break;
            case Constants.PLAYER_JOINED_EVENT:
                OnPlayerJoinedEvent(eventData);
                break;
            case Constants.EAT_EVENT:
                OnEatEvent(eventData);
                break;
            case Constants.KILL_EVENT:
                OnKillEvent(eventData);
                break;
            case Constants.REBORN_EVENT:
                OnRebornEvent(eventData);
                break;
            case Constants.SPAWN_FOOD_EVENT:
                OnSpawnFoodEvent(eventData);
                break;
            default:
                break;
        }
    }

    void Client_OnPlayerRoomLeft(Player player) {
        // 清理工作
        var ball = IdToBalls[player.ActorId];
        if (ball != null) {
            Destroy(ball.gameObject);
            ui.RemovePlayerInfo();
            ui.UpdateList();
        }
    }

    void OnBornEvent(Dictionary<string, object> eventData) {
        Debug.Log("on born event");
        var client = LeanCloudUtils.GetClient();
        // 初始化 UI
        ui.Init();
        var playerList = client.Room.PlayerList;
        foreach (var player in playerList) {
            // 实例化球
            var ball = NewBall(player);
            if (player.IsLocal) {
                var ballCtrl = ball.gameObject.AddComponent<BallController>();
                ballCtrl.cameraTrans = cameraTrans;
            } else {
                ball.gameObject.AddComponent<BallSimulator>();
            }
        }
        ui.UpdateList();
        // 增加食物数据
        var foods = eventData["foods"] as List<object>;
        SpawnFoods(foods);
    }

    void OnPlayerJoinedEvent(Dictionary<string, object> eventData) {
        var client = LeanCloudUtils.GetClient();
        var playerId = int.Parse(eventData["pId"].ToString());
        var player = client.Room.GetPlayer(playerId);
        // 实例化一个新球
        var ball = NewBall(player);
        ball.gameObject.AddComponent<BallSimulator>();
        ui.UpdateList();
    }

    void OnEatEvent(Dictionary<string, object> eventData) {
        var ballId = int.Parse(eventData["pId"].ToString());
        var foodId = int.Parse(eventData["fId"].ToString());
        var ball = IdToBalls[ballId];
        // 吃
        ball.Eat();
        var food = IdToFoods[foodId];
        IdToFoods.Remove(foodId);
        Destroy(food.gameObject);
        ui.UpdateList();
    }

    void OnKillEvent(Dictionary<string, object> eventData) {
        var winnerId = int.Parse(eventData["winnerId"].ToString());
        var winner = IdToBalls[winnerId];
        winner.Win();
        var loserId = int.Parse(eventData["loserId"].ToString());
        var loser = IdToBalls[loserId];
        loser.Lose();
        ui.UpdateList();
    }

    void OnRebornEvent(Dictionary<string, object> eventData) {
        var playerId = int.Parse(eventData["playerId"].ToString());
        var ball = IdToBalls[playerId];
        ball.Reborn();
        ui.UpdateList();
    }

    void OnPlayerLeftEvent(Dictionary<string, object> eventData) {
        var playerId = int.Parse(eventData["playerId"].ToString());
        var ball = IdToBalls[playerId];
        Destroy(ball.gameObject);
        ui.RemovePlayerInfo();
    }

    void OnSpawnFoodEvent(Dictionary<string, object> eventData) {
        var foods = eventData["foods"] as List<object>;
        NextFoodId = int.Parse(eventData["nextFoodId"].ToString());
        SpawnFoods(foods);
    }

    void SpawnFoods(List<object> foods) {
        // 实例化食物对象
        foreach (Dictionary<string, object> foodData in foods) {
            var id = int.Parse(foodData["id"].ToString());
            var type = int.Parse(foodData["type"].ToString());
            var x = float.Parse(foodData["x"].ToString());
            var y = float.Parse(foodData["y"].ToString());
            var foodGo = Instantiate(foodTemplates[type]);
            foodGo.transform.parent = transform;
            foodGo.transform.localPosition = new Vector3(x, y, 0);
            var food = foodGo.GetComponent<Food>();
            food.Id = id;
            food.Type = type;
            IdToFoods[id] = food;
        }
    }

    Ball NewBall(Player player) {
        var ballGO = Instantiate(ballTemplate);
        ballGO.transform.parent = transform;
        var pos = player.CustomProperties["pos"] as Dictionary<string, object>;
        var x = float.Parse(pos["x"].ToString());
        var y = float.Parse(pos["y"].ToString());
        ballGO.transform.localPosition = new Vector2(x, y);
        var ball = ballGO.GetComponent<Ball>();
        ball.Player = player;
        IdToBalls[player.ActorId] = ball;
        ui.AddPlayerInfo(ball);
        return ball;
    }

    public List<object> GetFoods() {
        var foods = new List<object>();
        foreach (var food in IdToFoods) {
            foods.Add(food.Value.GetData());
        }
        return foods;
    }
}
