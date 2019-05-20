using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCloud.Play {
    /// <summary>
    /// 房间类
    /// </summary>
	public class Room {
        internal Client Client {
            get; set;
        }

        Dictionary<int, Player> players = null;

        /// <summary>
        /// 房间名称
        /// </summary>
        /// <value>The name.</value>
		public string Name {
            get; private set;
        }
        /// <summary>
        /// 房间是否开启
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
        /// 房间允许的最大玩家数量
        /// </summary>
        /// <value>The max player count.</value>
		public int MaxPlayerCount {
            get; private set;
		}

        /// <summary>
        /// 房间主机玩家 ID
        /// </summary>
        /// <value>The master actor identifier.</value>
		public int MasterActorId {
            get; internal set;
		}

        /// <summary>
        /// 获取房主
        /// </summary>
        /// <value>The master.</value>
        public Player Master {
            get {
                return GetPlayer(MasterActorId);
            }
        }

        /// <summary>
        /// 邀请的好友 ID 列表
        /// </summary>
        /// <value>The expected user identifiers.</value>
        public List<string> ExpectedUserIds {
            get; private set;
		}

        Dictionary<string, object> customProperties;

        /// <summary>
        /// 获取自定义属性
        /// </summary>
        /// <value>The custom properties.</value>
        public Dictionary<string, object> CustomProperties {
            get {
                return customProperties;
            } internal set {
                customProperties = value;
            }
        }

        /// <summary>
        /// 获取房间内的玩家列表
        /// </summary>
        /// <value>The player list.</value>
        public List<Player> PlayerList {
            get {
                lock (players) {
                    return players.Values.ToList();
                }
            }
        }

        /// <summary>
        /// 设置房间的自定义属性
        /// </summary>
        /// <param name="properties">自定义属性</param>
        /// <param name="expectedValues">期望属性，用于 CAS 检测</param>
        public Task SetCustomProperties(Dictionary<string, object> properties, Dictionary<string, object> expectedValues = null) {
            return Client.SetRoomCustomProperties(properties, expectedValues);
        }

        /// <summary>
        /// 根据 actorId 获取 Player 对象
        /// </summary>
        /// <returns>玩家对象</returns>
        /// <param name="actorId">玩家在房间中的 Id</param>
        public Player GetPlayer(int actorId) {
            lock (players) {
                if (!players.TryGetValue(actorId, out Player player)) {
                    throw new Exception(string.Format("no player: {0}", actorId));
                }
                return player;
            }
        }

        public Task<bool> SetOpened(bool opened) {
            return Client.SetRoomOpened(opened);
        }

        public Task<bool> SetVisible(bool visible) {
            return Client.SetRoomVisible(visible);
        }

        internal static Room NewFromDictionary(Client client, Dictionary<string, object> roomDict) {
            Room room = NewFromDictionary(roomDict);
            room.Client = client;
            return room;
        }

        internal static Room NewFromDictionary(Dictionary<string, object> roomDict) {
            if (roomDict == null) {
                throw new ArgumentException("Room data is null");
            }

            Room room = new Room() {
                Name = roomDict["cid"] as string,
                Opened = bool.Parse(roomDict["open"].ToString()),
                Visible = bool.Parse(roomDict["visible"].ToString()),
                MaxPlayerCount = int.Parse(roomDict["maxMembers"].ToString()),
                MasterActorId = int.Parse(roomDict["masterActorId"].ToString())
            };
            if (roomDict.TryGetValue("expectMembers", out object expectedsObj)) {
                var expecteds = expectedsObj as List<object>;
                room.ExpectedUserIds = expecteds.Cast<string>().ToList();
            }
            room.players = new Dictionary<int, Player>();
            List<object> players = roomDict["members"] as List<object>;
            foreach (Dictionary<string, object> playerDict in players) {
                Player player = Player.NewFromDictionary(playerDict);
                room.players.Add(player.ActorId, player);
            }
            if (roomDict.TryGetValue("attr", out object propsObj)) {
                var props = propsObj as Dictionary<string, object>;
                room.CustomProperties = props;
            } else {
                room.CustomProperties = new Dictionary<string, object>();
            }
            return room;
        }

        internal void AddPlayer(Player player) {
			if (player == null) {
				throw new Exception(string.Format("player is null"));
			}
            lock (players) {
                players.Add(player.ActorId, player);
            }
		}

        internal void RemovePlayer(int actorId) {
            lock (players) {
                if (!players.Remove(actorId)) {
                    throw new Exception(string.Format("no player: {0}", actorId));
                }
            }
		}

        internal void MergeProperties(Dictionary<string, object> changedProps) {
            if (changedProps == null)
                return;

            lock (customProperties) {
                foreach (KeyValuePair<string, object> entry in changedProps) {
                    customProperties[entry.Key] = entry.Value;
                }
            }
        }
	}
}
