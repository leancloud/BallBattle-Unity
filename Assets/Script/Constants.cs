using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    public const float WIDTH = 30f;
    public const float HEIGHT = 16f;
    // 边界
    public const float LEFT = -WIDTH / 2;
    public const float RIGHT = -LEFT;
    public const float TOP = HEIGHT / 2;
    public const float BOTTOM = -TOP;
    // 最小容忍误差
    public const float DISTANCE_MAG = 0.1f;
    // 初始生成食物数量
    public const int INIT_FOOD_COUNT = 20;
    // 同步食物间隔
    public const int SYNC_FOOD_DURATION = 200000;
    // 补充食物间隔
    public const int SPWAN_FOOD_DURATION = 30;

    // 碰撞 Tag
    public const string BALL_TAG = "ball";
    public const string FOOD_TAG = "food";

    // 初始尺寸
    public const int BORN_SIZE = 48;
    // 速度因子，实际速度 = 速度因子 / 体重
    public const int SPEED_FACTOR = 300000;
    // 最小速率
    public const int MIN_SPEED = 30;
    // 食物重量
    public const int FOOD_WEIGHT = 100;
    // 圆周率
    public const float PI = 3.14f;

    // 自定义通信事件
    // 玩家出生事件
    public const byte BORN_EVENT = 10;
    // 新玩家加入
    public const byte PLAYER_JOINED_EVENT = 7;
    // 吃食物
    public const byte EAT_EVENT = 1;
    // 杀死玩家
    public const byte KILL_EVENT = 2;
    // 玩家离开
    public const byte PLAYER_LEFT_EVENT = 3;
    // 重生
    public const byte REBORN_EVENT = 4;
    // 生成食物
    public const byte SPAWN_FOOD_EVENT = 5;
    // 游戏结束
    public const byte GAME_OVER_EVENT = 6;

    // 自定义逻辑事件
    // 球和食物碰撞事件
    public const string OnBallAndFoodCollision = "OnBallAndFoodCollision";
    // 球和球碰撞事件
    public const string OnBallAndBallCollision = "OnBallAndBallCollision";
}
