using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSimulator : MonoBehaviour {
    Ball ball;

    void Start() {
        ball = GetComponent<Ball>();
        var player = ball.Player;
        var pos = player.CustomProperties["pos"] as Dictionary<string, object>;
        var x = float.Parse(pos["x"].ToString());
        var y = float.Parse(pos["y"].ToString());
        transform.localPosition = new Vector2(x, y);
    }

    void Update() {
        if (ball.Player.CustomProperties.TryGetValue("move", out object m)) {
            if (!(m is Dictionary<string, object> move)) {
                return;
            }
            // 计算当前位置
            var now = BattleHelper.Now;
            var speed = ball.Speed;
            var delta = (now - long.Parse(move["t"].ToString())) / 1000.0f;
            var pos = move["p"] as Dictionary<string, object>;
            var start = new Vector2(float.Parse(pos["x"].ToString()), float.Parse(pos["y"].ToString()));
            var dir = move["d"] as Dictionary<string, object>;
            var direction = new Vector2(float.Parse(dir["x"].ToString()), float.Parse(dir["y"].ToString()));
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
