using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家模拟器，主要模拟运动
/// </summary>
public class BallSimulator : MonoBehaviour {
    BallBeh ball;

    void Start() {
        ball = GetComponent<BallBeh>();
        var player = ball.Player;
        var pos = player.CustomProperties.Get<Vec2>("pos");
        transform.localPosition = new Vector2(pos.X, pos.Y);
    }

    void Update() {
        if (!ball.Player.CustomProperties.IsNull("move")) {
            var move = ball.Player.CustomProperties.Get<Move>("move");
            // 计算当前位置
            var now = BattleHelper.Now;
            var speed = ball.Speed;
            var delta = (now - move.Time) / 1000.0f;
            var pos = move.Pos;
            var start = new Vector2(pos.X, pos.Y);
            var dir = move.Dir;
            var direction = new Vector2(dir.X, dir.Y);
            var end = start + direction.normalized * speed * delta;
            // 计算模拟运动
            var curPos = new Vector2(transform.localPosition.x, transform.localPosition.y);
            var mag = (end - curPos).magnitude;
            if (mag < Constants.DISTANCE_MAG) {
                return;
            }
            direction = end - curPos;
            var newPos = curPos + direction.normalized * speed * Time.deltaTime;
            newPos = new Vector2(
                Mathf.Min(Mathf.Max(newPos.x, Constants.LEFT), Constants.RIGHT),
                Mathf.Min(Mathf.Max(newPos.y, Constants.BOTTOM), Constants.TOP)
            );
            transform.localPosition = newPos;
        }
    }
}
