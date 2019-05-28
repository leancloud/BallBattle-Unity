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
            var move = m as Dictionary<string, object>;
            var now = BattleHelper.Now;
            var speed = ball.Speed;
            var delta = now - long.Parse(move["t"].ToString());
            var pos = move["p"] as Dictionary<string, object>;
            var start = new Vector2((float)pos["x"], (float)pos["y"]);
            var dir = move["d"] as Dictionary<string, object>;
            var direction = new Vector2((float)dir["x"], (float)dir["y"]);
            var end = start + direction * speed * Time.deltaTime;
            var curPos = new Vector2(transform.localPosition.x, transform.localPosition.y);
            var mag = (end - curPos).magnitude;
            if (mag < Constants.DISTANCE_MAG) {
                return;
            }
            direction = (end - curPos).normalized;
            var posDelta = direction * speed * Time.deltaTime;
            var newPos = curPos + posDelta;
            newPos = new Vector2(
                Mathf.Min(Mathf.Max(newPos.x, Constants.LEFT), Constants.RIGHT),
                Mathf.Min(Mathf.Max(newPos.y, Constants.BOTTOM), Constants.TOP)
            );
            transform.localPosition = newPos;
        }
    }
}
