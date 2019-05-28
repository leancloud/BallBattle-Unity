using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    public int Id {
        get; set;
    }

    public int Type {
        get; set;
    }

    public Dictionary<string, object> GetData() {
        float x = transform.localPosition.x;
        float y = transform.localPosition.y;
        return new Dictionary<string, object> {
            { "id", Id },
            { "type", Type },
            { "x", x },
            { "y", y }
        };
    }
}
