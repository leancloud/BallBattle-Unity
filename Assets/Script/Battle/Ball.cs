using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LeanCloud.Play;

public class Ball : MonoBehaviour
{
    public Player Player {
        get; set;
    }

    public float Speed { 
        get {
            return 2.0f;
        }
    }

    public int Weight { 
        get {
            return int.Parse(Player.CustomProperties["weight"].ToString());
        }
    }

    public void Eat() {
        UpdateScale();
    }

    public void Win() {
        UpdateScale();
    }

    public void Lose() {
        gameObject.SetActive(false);
    }

    public void Reborn() {
        UpdateScale();
        var pos = Player.CustomProperties["pos"] as Dictionary<string, object>;
        var x = float.Parse(pos["x"].ToString());
        var y = float.Parse(pos["y"].ToString());
        transform.localPosition = new Vector2(x, y);
        gameObject.SetActive(true);
    }

    void UpdateScale() {
        var scale = Mathf.Sqrt(Weight) / Constants.BORN_SIZE;
        transform.localScale = new Vector2(scale, scale);
    }

    void OnTriggerEnter2D(Collider2D other) {
        Debug.Log("colide 2d something");
        var otherTag = other.tag;
        if (otherTag.Equals(Constants.FOOD_TAG)) {
            var food = other.gameObject.GetComponent<Food>();
            OnCollideFood(food);
        } else if (otherTag.Equals(Constants.BALL_TAG)) {
            var ball = other.gameObject.GetComponent<Ball>();
            OnCollideBall(ball);
        }
    }

    void OnCollideFood(Food food) {
        Debug.Log("collide food");
        var args = new Dictionary<string, object> {
            { "ball", this },
            { "food", food }
        };
        SendMessageUpwards(Constants.OnBallAndFoodCollision, args, SendMessageOptions.DontRequireReceiver);
    }

    void OnCollideBall(Ball ball) {
        Debug.Log("collide ball");
        var args = new Dictionary<string, Ball> {
            { "b1", this },
            { "b2", ball },
        };
        SendMessageUpwards(Constants.OnBallAndBallCollision, args, SendMessageOptions.DontRequireReceiver);
    }
}
