using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoItem : MonoBehaviour
{
    public Text rankText;
    public Text nameText;
    public Text weightText;

    static string ToReadableWeight(int weight) { 
        if (weight < 1000) {
            return weight.ToString();
        }
        if (weight < 1000000) {
            return $"{weight / 1000}k";
        }
        return $"{weight / 1000 / 1000}m";
    }
}
