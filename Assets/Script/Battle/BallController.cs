using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using LeanCloud.Play;

/// <summary>
/// 玩家控制器
/// </summary>
public class BallController : MonoBehaviour
{
    public Transform cameraTrans;

    BallBeh ball;
    int horizontal;
    int vertical;
    
    void Start() {
        ball = GetComponent<BallBeh>();
        horizontal = 0;
        vertical = 0;
        var pos = transform.localPosition;
        cameraTrans.localPosition = new Vector3(pos.x, pos.y, -10);
    }

    void Update() {
        // 移动
        var speed = ball.Speed;
        if (horizontal != 0 || vertical != 0) {
            var direction = new Vector2(horizontal, vertical);
            var delta = direction * speed * Time.deltaTime;
            var position = transform.localPosition + new Vector3(delta.x, delta.y, 0);
            // 判断边界
            var newPosition = new Vector2(
                Math.Min(Math.Max(position.x, Constants.LEFT), Constants.RIGHT),
                Math.Min(Math.Max(position.y, Constants.BOTTOM), Constants.TOP));
            transform.localPosition = newPosition;
            cameraTrans.localPosition = new Vector3(newPosition.x, newPosition.y, -10);
        }

        // 监听输入操作
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
            vertical = 1;
            SynchMove();
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
            vertical = -1;
            SynchMove();
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
            horizontal = -1;
            SynchMove();
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
            horizontal = 1;
            SynchMove();
        }
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow) && vertical == 1) {
            vertical = 0;
            SynchMove();
        }
        if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow) && vertical == -1) {
            vertical = 0;
            SynchMove();
        }
        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow) && horizontal == -1) {
            horizontal = 0;
            SynchMove();
        }
        if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow) && horizontal == 1) {
            horizontal = 0;
            SynchMove();
        }
    }

    void SynchMove() {
        var client = LeanCloudUtils.GetClient();
        var pos = new Vec2 {
            X = transform.localPosition.x,
            Y = transform.localPosition.y
        };
        var dir = new Vec2 { 
            X = horizontal,
            Y = vertical
        };
        var move = new Move {
            Pos = pos,
            Dir = dir,
            Time = BattleHelper.Now
        };
        var props = new PlayObject {
            { "move", move }
        };
        client.Player.SetCustomProperties(props);
    }

    public void Reborn() {
        var pos = transform.localPosition;
        cameraTrans.localPosition = new Vector3(pos.x, pos.y, -10);
    }
}
