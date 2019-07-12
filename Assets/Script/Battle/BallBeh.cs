using UnityEngine;
using LeanCloud.Play;

/// <summary>
/// Ball beh.
/// </summary>
public class BallBeh : MonoBehaviour
{
    public Player Player {
        get; set;
    }

    public int Id { 
        get {
            return Player.ActorId;
        }
    }

    public float Speed { 
        get {
            return 2.0f;
        }
    }

    public float Weight { 
        get {
            return Player.CustomProperties.GetFloat("weight");
        }
    }

    public void Eat() {
        UpdateScale();
    }

    public void Win() {
        UpdateScale();
    }

    public void Lose() {
        var sprite = GetComponent<SpriteRenderer>();
        sprite.enabled = false;
        var pos = Player.CustomProperties.Get<Vec2>("pos");
        transform.localPosition = new Vector2(pos.X, pos.Y);
    }

    public void Reborn() {
        UpdateScale();
        var pos = Player.CustomProperties.Get<Vec2>("pos");
        transform.localPosition = new Vector2(pos.X, pos.Y);
        var sprite = GetComponent<SpriteRenderer>();
        sprite.enabled = true;
    }

    void UpdateScale() {
        var scale = Mathf.Sqrt(Weight) / Constants.BORN_SIZE;
        transform.localScale = new Vector2(scale, scale);
    }

    void OnTriggerEnter2D(Collider2D other) {
        var otherTag = other.tag;
        if (otherTag.Equals(Constants.FOOD_TAG)) {
            var food = other.gameObject.GetComponent<FoodBeh>();
            OnCollideFood(food);
        } else if (otherTag.Equals(Constants.BALL_TAG)) {
            var ball = other.gameObject.GetComponent<BallBeh>();
            OnCollideBall(ball);
        }
    }

    void OnCollideFood(FoodBeh food) {
        var args = new FoodCollisionArgs {
            Ball = this,
            FoodBeh = food
        };
        SendMessageUpwards(Constants.OnBallAndFoodCollision, args, SendMessageOptions.DontRequireReceiver);
    }

    void OnCollideBall(BallBeh ball) {
        var args = new BallCollisionArgs { 
            Ball1 = this,
            Ball2 = ball
        };
        SendMessageUpwards(Constants.OnBallAndBallCollision, args, SendMessageOptions.DontRequireReceiver);
    }
}
