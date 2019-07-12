using LeanCloud.Play;

/// <summary>
/// 二维向量
/// </summary>
public class Vec2 {
    public float X {
        get; set;
    }

    public float Y {
        get; set;
    }

    public static byte[] Serialize(object obj) {
        var vec = obj as Vec2;
        var playObj = new PlayObject {
            { "x", vec.X },
            { "y", vec.Y }
        };
        return CodecUtils.SerializePlayObject(playObj);
    }

    public static Vec2 Deserialize(byte[] bytes) {
        var playObj = CodecUtils.DeserializePlayObject(bytes);
        return new Vec2 {
            X = playObj.GetFloat("x"),
            Y = playObj.GetFloat("y")
        };
    }
}
