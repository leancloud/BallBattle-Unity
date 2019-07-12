using LeanCloud.Play;

/// <summary>
/// 食物数据
/// </summary>
public class Food {
    public int Id {
        get; set;
    }

    public int Type {
        get; set;
    }

    public float X {
        get; set;
    }

    public float Y {
        get; set;
    }

    public static byte[] Serialize(object obj) {
        Food food = obj as Food;
        var playObj = new PlayObject {
                { "id", food.Id },
                { "type", food.Type },
                { "x", food.X },
                { "y", food.Y }
            };
        return CodecUtils.SerializePlayObject(playObj);
    }

    public static Food Deserialize(byte[] bytes) {
        var playObj = CodecUtils.DeserializePlayObject(bytes);
        var food = new Food {
            Id = playObj.GetInt("id"),
            Type = playObj.GetInt("type"),
            X = playObj.GetFloat("x"),
            Y = playObj.GetFloat("y")
        };
        return food;
    }
}
