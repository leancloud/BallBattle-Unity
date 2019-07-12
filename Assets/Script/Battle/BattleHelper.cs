using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using LeanCloud.Play;

public static class BattleHelper
{
    public static Vec2 RandomPos() {
        var x = Constants.LEFT + UnityEngine.Random.Range(0f, 1f) * Constants.WIDTH;
        var y = Constants.BOTTOM + UnityEngine.Random.Range(0f, 1f) * Constants.HEIGHT;
        return new Vec2 { 
            X = Convert.ToSingle(Math.Round(x, 1)),
            Y = Convert.ToSingle(Math.Round(y, 1))
        };
    }

    public static long Now {
        get {
            return (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
        }
    }

    public static string ToReadableWeight(int weight) {
        if (weight < 1000) {
            return weight.ToString();
        }
        if (weight < 1000000) {
            return $"{Math.Round(weight / 1000.0f, 2)}k";
        }
        return $"{Math.Round(weight / 1000.0f / 1000, 2)}m";
    }
}
