using System;
using System.Collections.Generic;
using System.Linq;

namespace LeanCloud.Play 
{
    /// <summary>
    /// 大厅房间数据类
    /// </summary>
    public class LobbyRoom
    {
        /// <summary>
        /// 房间名称
        /// </summary>
        /// <value>The name of the room.</value>
        public string RoomName {
            get; internal set;
        }

        /// <summary>
        /// 房间最大玩家数
        /// </summary>
        /// <value>The max player count.</value>
        public int MaxPlayerCount {
            get; internal set;
        }

        /// <summary>
        /// 邀请好友 ID 数组
        /// </summary>
        /// <value>The expected user identifiers.</value>
        public List<string> ExpectedUserIds {
            get; internal set;
        }

        /// <summary>
        /// 房间置空后销毁时间（秒）
        /// </summary>
        /// <value>The empty room ttl.</value>
        public int EmptyRoomTtl {
            get; internal set;
        }

        /// <summary>
        /// 玩家离线后踢出房间时间（秒）
        /// </summary>
        /// <value>The player ttl.</value>
        public int PlayerTtl {
            get; internal set;
        }

        /// <summary>
        /// 当前房间玩家数量
        /// </summary>
        /// <value>The player count.</value>
        public int PlayerCount {
            get; internal set;
        }

        /// <summary>
        /// 房间是否开放
        /// </summary>
        /// <value><c>true</c> if opened; otherwise, <c>false</c>.</value>
        public bool Opened {
            get; internal set;
        }

        /// <summary>
        /// 房间是否可见
        /// </summary>
        /// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
        public bool Visible {
            get; internal set;
        }

        /// <summary>
        /// 房间属性
        /// </summary>
        /// <value>The custom room properties.</value>
        public Dictionary<string, object> CustomRoomProperties {
            get; internal set;
        }

        internal static LobbyRoom NewFromDictionary(Dictionary<string, object> roomDict) {
            LobbyRoom lobbyRoom = new LobbyRoom {
                RoomName = roomDict["cid"] as string
            };
            if (roomDict.TryGetValue("maxMembers", out object maxPlayerCountObj) && 
                int.TryParse(maxPlayerCountObj.ToString(), out int maxPlayerCount)) {
                lobbyRoom.MaxPlayerCount = maxPlayerCount;
            }
            if (roomDict.TryGetValue("expectMembers", out object expectedsObj)) {
                lobbyRoom.ExpectedUserIds = (expectedsObj as List<object>).Cast<string>().ToList();
            }
            if (roomDict.TryGetValue("emptyRoomTtl", out object roomTtlObj) &&
                int.TryParse(roomTtlObj.ToString(), out int emptyRoomTtl)) {
                lobbyRoom.EmptyRoomTtl = emptyRoomTtl;
            }
            if (roomDict.TryGetValue("playerTtl", out object playerTtlObj) &&
                int.TryParse(playerTtlObj.ToString(), out int playerTtl)) {
                lobbyRoom.PlayerTtl = playerTtl;
            }
            if (roomDict.TryGetValue("playerCount", out object playerCountObj) &&
                int.TryParse(playerCountObj.ToString(), out int playerCount)) {
                lobbyRoom.PlayerCount = playerCount;
            }
            if (roomDict.TryGetValue("attr", out object propsObj)) {
                lobbyRoom.CustomRoomProperties = propsObj as Dictionary<string, object>;
            }
            if (roomDict.TryGetValue("open", out object openObj) &&
                bool.TryParse(openObj.ToString(), out bool opened)) {
                lobbyRoom.Opened = opened;
            }
            if (roomDict.TryGetValue("visible", out object visibleObj) &&
                bool.TryParse(visibleObj.ToString(), out bool visible)) {
                lobbyRoom.Visible = visible;
            }
            return lobbyRoom;
        }

        internal LobbyRoom() { }
    }
}
