using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class BattleHelper
{
    public static Dictionary<string, object> RandomPos() {
        var x = Constants.LEFT + UnityEngine.Random.Range(0f, 1f) * Constants.WIDTH;
        var y = Constants.BOTTOM + UnityEngine.Random.Range(0f, 1f) * Constants.HEIGHT;
        return new Dictionary<string, object> {
            { "x", Math.Round(x, 1) },
            { "y", Math.Round(y, 1) }
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
