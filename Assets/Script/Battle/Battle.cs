using System.Collections.Generic;
using UnityEngine;
using LeanCloud.Play;

public class Battle : MonoBehaviour
{
    public GameObject ballTemplate;
    public GameObject[] foodTemplates;
    public UI ui;
    public Transform cameraTrans;

    public Dictionary<int, BallBeh> IdToBalls {
        get; set;
    }

    public Dictionary<int, FoodBeh> IdToFoods {
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

        IdToBalls = new Dictionary<int, BallBeh>();
        IdToFoods = new Dictionary<int, FoodBeh>();
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

    void OnApplicationQuit() {
        var client = LeanCloudUtils.GetClient();
        if (client != null) {
            client.Close();
        }
    }

    void Client_OnMasterSwitched(Player newMaster) {
        if (newMaster.IsLocal) {
            // 当前客户端被设置为 Master
            var master = gameObject.AddComponent<Master>();
            master.SwitchGame();
        }
    }

    void Client_OnCustomEvent(byte eventId, PlayObject eventData, int senderId) {
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
            ui.RemovePlayerInfo(ball);
            ui.UpdateList();
        }
    }

    void OnBornEvent(PlayObject eventData) {
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
        var foods = eventData.GetPlayArray("foods");
        SpawnFoods(foods);
    }

    void OnPlayerJoinedEvent(PlayObject eventData) {
        var client = LeanCloudUtils.GetClient();
        var playerId = eventData.GetInt("pId");
        var player = client.Room.GetPlayer(playerId);
        // 实例化一个新球
        var ball = NewBall(player);
        ball.gameObject.AddComponent<BallSimulator>();
        ui.UpdateList();
    }

    void OnEatEvent(PlayObject eventData) {
        var ballId = eventData.GetInt("pId");
        var foodId = eventData.GetInt("fId");
        var ball = IdToBalls[ballId];
        // 吃
        ball.Eat();
        var food = IdToFoods[foodId];
        IdToFoods.Remove(foodId);
        Destroy(food.gameObject);
        ui.UpdateList();
    }

    void OnKillEvent(PlayObject eventData) {
        var winnerId = eventData.GetInt("winnerId");
        var winner = IdToBalls[winnerId];
        winner.Win();
        var loserId = eventData.GetInt("loserId");
        var loser = IdToBalls[loserId];
        loser.Lose();
        ui.UpdateList();
    }

    void OnRebornEvent(PlayObject eventData) {
        var playerId = eventData.GetInt("playerId");
        var ball = IdToBalls[playerId];
        ball.SendMessage("Reborn");
        ui.UpdateList();
    }

    void OnPlayerLeftEvent(PlayObject eventData) {
        var playerId = eventData.GetInt("playerId");
        var ball = IdToBalls[playerId];
        Destroy(ball.gameObject);
        ui.RemovePlayerInfo(ball);
    }

    void OnSpawnFoodEvent(PlayObject eventData) {
        var foods = eventData.GetPlayArray("foods");
        NextFoodId = eventData.GetInt("nextFoodId");
        SpawnFoods(foods);
    }

    void SpawnFoods(PlayArray foods) {
        // 实例化食物对象
        foreach (Food foodData in foods) {
            var foodGo = Instantiate(foodTemplates[foodData.Type]);
            foodGo.transform.parent = transform;
            foodGo.transform.localPosition = new Vector3(foodData.X, foodData.Y, 0);
            var food = foodGo.GetComponent<FoodBeh>();
            food.Data = foodData;
            IdToFoods[foodData.Id] = food;
        }
    }

    BallBeh NewBall(Player player) {
        var ballGO = Instantiate(ballTemplate);
        ballGO.transform.parent = transform;
        var pos = player.CustomProperties.Get<Vec2>("pos");
        var x = pos.X;
        var y = pos.Y;
        ballGO.transform.localPosition = new Vector2(x, y);
        var ball = ballGO.GetComponent<BallBeh>();
        ball.Player = player;
        IdToBalls[player.ActorId] = ball;
        ui.AddPlayerInfo(ball);
        return ball;
    }

    public PlayArray GetFoods() {
        var foods = new PlayArray();
        foreach (var food in IdToFoods) {
            foods.Add(food.Value.Data);
        }
        return foods;
    }
}
