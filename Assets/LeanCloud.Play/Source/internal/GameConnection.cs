using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace LeanCloud.Play {
    internal class GameConnection : Connection {
        internal Room Room {
            get; private set;
        }

        internal GameConnection() {
        
        }

        internal static async Task<GameConnection> Connect(string appId, string server, string userId, string gameVersion) {
            var tcs = new TaskCompletionSource<GameConnection>();
            var connection = new GameConnection();
            await connection.Connect(server, userId);
            await connection.OpenSession(appId, userId, gameVersion);
            return connection;
        }

        internal async Task<Room> CreateRoom(string roomId, RoomOptions roomOptions, List<string> expectedUserIds) {
            var msg = Message.NewRequest("conv", "start");
            if (roomId != null) {
                msg["cid"] = roomId;
            }
            if (roomOptions != null) {
                var roomOptionsDict = roomOptions.ToDictionary();
                foreach (var entry in roomOptionsDict) {
                    msg[entry.Key] = entry.Value;
                }
            }
            if (expectedUserIds != null) {
                var expecteds = expectedUserIds.Cast<object>().ToList();
                msg["expectMembers"] = expecteds;
            }
            var res = await Send(msg);
            return Room.NewFromDictionary(res.Data);
        }

        internal async Task<Room> JoinRoom(string roomId, List<string> expectedUserIds) {
            var msg = Message.NewRequest("conv", "add");
            msg["cid"] = roomId;
            if (expectedUserIds != null) {
                List<object> expecteds = expectedUserIds.Cast<object>().ToList();
                msg["expectMembers"] = expecteds;
            }
            var res = await Send(msg);
            return Room.NewFromDictionary(res.Data);
        }

        internal async Task LeaveRoom() {
            var msg = Message.NewRequest("conv", "remove");
            await Send(msg);
        }

        internal async Task<bool> SetRoomOpened(bool opened) {
            var msg = Message.NewRequest("conv", "open");
            msg["toggle"] = opened;
            var res = await Send(msg);
            if (res.TryGetValue("toggle", out object openedObj) &&
                bool.TryParse(openedObj.ToString(), out bool open)) {
                return open;
            }
            return opened;
        }

        internal async Task<bool> SetRoomVisible(bool visible) {
            var msg = Message.NewRequest("conv", "visible");
            msg["toggle"] = visible;
            var res = await Send(msg);
            if (res.TryGetValue("toggle", out object visibleObj) &&
                bool.TryParse(visibleObj.ToString(), out bool v)) {
                return v;
            }
            return visible;
        }

        internal async Task<int> SetMaster(int newMasterId) {
            var msg = Message.NewRequest("conv", "update-master-client");
            msg["masterActorId"] = newMasterId;
            var res = await Send(msg);
            if (res.TryGetValue("masterActorId", out object masterIdObj) &&
                int.TryParse(masterIdObj.ToString(), out int masterId)) {
                return masterId;
            }
            return -1;
        }

        internal async Task<int> KickPlayer(int actorId, int code, string reason) {
            var msg = Message.NewRequest("conv", "kick");
            msg["targetActorId"] = actorId;
            msg["appCode"] = code;
            msg["appMsg"] = reason;
            var res = await Send(msg);
            if (res.TryGetValue("targetActorId", out object actorIdObj) &&
                int.TryParse(actorIdObj.ToString(), out int kickedActorId)) {
                return kickedActorId;
            }
            return actorId;
        }

        internal async Task SendEvent(byte eventId, Dictionary<string, object> eventData, SendEventOptions options) {
            var msg = Message.NewRequest("direct", null);
            msg["eventId"] = eventId;
            msg["msg"] = eventData;
            msg["receiverGroup"] = (int) options.ReceiverGroup;
            if (options.targetActorIds != null) {
                msg["toActorIds"] = options.targetActorIds.Cast<object>().ToList();
            }
            await Send(msg);
        }

        internal async Task<Dictionary<string, object>> SetRoomCustomProperties(Dictionary<string, object> properties, Dictionary<string, object> expectedValues) {
            var msg = Message.NewRequest("conv", "update");
            msg["attr"] = properties;
            if (expectedValues != null) {
                msg["expectAttr"] = expectedValues;
            }
            var res = await Send(msg);
            if (res.TryGetValue("attr", out object attrObj)) {
                return attrObj as Dictionary<string, object>;
            }
            return null;
        }

        internal async Task<Dictionary<string, object>> SetPlayerCustomProperties(int playerId, Dictionary<string, object> properties, Dictionary<string, object> expectedValues) {
            var msg = Message.NewRequest("conv", "update-player-prop");
            msg["targetActorId"] = playerId;
            msg["attr"] = properties;
            if (expectedValues != null) {
                msg["expectAttr"] = expectedValues;
            }
            var res = await Send(msg);
            if (res.TryGetValue("actorId", out object actorIdObj) &&
                int.TryParse(actorIdObj.ToString(), out int actorId) &&
                res.TryGetValue("attr", out object attrObj)) {
                return new Dictionary<string, object> {
                        { "actorId", actorId },
                        { "changedProps", attrObj },
                };
            }
            return null;
        }

        protected override int GetPingDuration() {
            return 7;
        }
    }
}
