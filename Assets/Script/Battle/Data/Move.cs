using LeanCloud.Play;

public class Move {
    public Vec2 Pos {
        get; set;
    }

    public Vec2 Dir {
        get; set;
    }

    public long Time {
        get; set;
    }

    public static byte[] Serialize(object obj) {
        var move = obj as Move;
        var playObj = new PlayObject {
            { "p", move.Pos },
            { "d", move.Dir },
            { "t", move.Time }
        };
        return CodecUtils.SerializePlayObject(playObj);
    }

    public static Move Deserialize(byte[] bytes) {
        var playObj = CodecUtils.DeserializePlayObject(bytes);
        return new Move {
            Pos = playObj.Get<Vec2>("p"),
            Dir = playObj.Get<Vec2>("d"),
            Time = playObj.GetLong("t")
        };
    }
}
