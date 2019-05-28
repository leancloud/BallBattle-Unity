using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoItem : MonoBehaviour
{
    public Text rankText;
    public Text nameText;
    public Text weightText;

    public void SetInfo(int rank, string name, int weight, bool isLocal) {
        var color = Color.white;
        if (isLocal) {
            color = Color.yellow;
        }
        rankText.color = color;
        nameText.color = color;
        weightText.color = color;

        rankText.text = rank.ToString();
        nameText.text = name;
        weightText.text = BattleHelper.ToReadableWeight(weight);
    }
}
