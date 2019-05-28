using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public Transform cameraTrans;

    Ball ball;
    Vector3 direction;
    
    void Start() {
        ball = GetComponent<Ball>();
        direction = Vector3.zero;
    }

    void Update() {
        // 移动
        var speed = 2.0f;
        var delta = direction * speed * Time.deltaTime;
        var position = transform.localPosition + delta;
        // 判断边界
        var newPosition = new Vector2(
            Math.Min(Math.Max(position.x, Constants.LEFT), Constants.RIGHT), 
            Math.Min(Math.Max(position.y, Constants.BOTTOM), Constants.TOP));
        transform.localPosition = newPosition;
        cameraTrans.localPosition = new Vector3(newPosition.x, newPosition.y, -10);
        // 监听输入操作
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
            var dir = direction;
            dir.y = 1;
            SynchMove(dir);
        } else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
            var dir = direction;
            dir.y = -1;
            SynchMove(dir);
        } else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
            var dir = direction;
            dir.x = -1;
            SynchMove(dir);
        } else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
            var dir = direction;
            dir.x = 1;
            SynchMove(dir);
        } else if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow) ||
                   Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow)) {
            var dir = direction;
            dir.y = 0;
            SynchMove(dir);
        } else if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow) ||
                   Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow)) {
            var dir = direction;
            dir.x = 0;
            SynchMove(dir);
        }
    }

    void SynchMove(Vector2 dir) {
        direction = dir;
        float x = transform.localPosition.x;
        float y = transform.localPosition.y;
        float dx = direction.x;
        float dy = direction.y;
        var client = LeanCloudUtils.GetClient();
        var move = new Dictionary<string, object> {
            { "p", ToDict(transform.localPosition) },
            { "d", ToDict(direction) },
            { "t", BattleHelper.Now }
        };
        var props = new Dictionary<string, object> {
            { "move", move }
        };
        // 同步移动信息
        client.Player.SetCustomProperties(props);
    }

    Dictionary<string, object> ToDict(Vector2 vec) {
        return new Dictionary<string, object> {
            { "x", vec.x },
            { "y", vec.y }
        };
    }
}
