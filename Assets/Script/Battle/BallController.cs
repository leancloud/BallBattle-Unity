using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public Transform cameraTrans;

    Vector3 direction;
    
    void Start() {
        direction = Vector3.zero;
    }

    void Update() {
        // 移动
        var speed = 2.0f;
        var delta = direction * speed * Time.deltaTime;
        var position = transform.localPosition + delta;
        // TODO 判断边界

        transform.localPosition = position;
        cameraTrans.localPosition = new Vector3(position.x, position.y, -10);

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
        var props = new Dictionary<string, object> {
            ["move"] = new Dictionary<string, object> {
                { "p", ToDict(transform.localPosition) },
                { "d", ToDict(direction) },
                { "t", (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds }
            }
        };
        // TODO 同步移动信息

    }

    Dictionary<string, object> ToDict(Vector2 vec) {
        return new Dictionary<string, object> {
            { "x", vec.x },
            { "y", vec.y }
        };
    }
}
