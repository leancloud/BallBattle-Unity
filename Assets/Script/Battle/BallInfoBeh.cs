using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 玩家信息，包括名字，位置
/// </summary>
public class BallInfoBeh : MonoBehaviour
{
    public Text nameText;
    public Text coordinateText;

    public BallBeh ball;

    Canvas canvas;

    void Start() {
        canvas = transform.parent.GetComponent<Canvas>();
        nameText.text = ball.Player.UserId;
    }

    void Update() {
        var x = System.Math.Round(ball.transform.localPosition.x, 2);
        var y = System.Math.Round(ball.transform.localPosition.y, 2);
        coordinateText.text = $"({x}, {y})";
        // 位置跟随
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, 
            Camera.main.WorldToScreenPoint(ball.transform.position),
            null, out Vector2 pos)) {
            (transform as RectTransform).anchoredPosition = pos;
        }
    }
}
