using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace LeanCloud.Play {
    internal class LobbyConnection : Connection {
        internal LobbyConnection() {

        }

        internal static async Task<LobbyConnection> Connect(string appId, string server, string userId, string gameVersion) {
            LobbyConnection connection = new LobbyConnection();
            await connection.Connect(server, userId);
            await connection.OpenSession(appId, userId, gameVersion);
            return connection;
        }

        internal async Task JoinLobby() {
            var msg = Message.NewRequest("lobby", "add");
            await Send(msg);
        }

        internal async Task<LobbyRoomResult> CreateRoom(string roomName, RoomOptions roomOptions, List<string> expectedUserIds) {
            var msg = Message.NewRequest("conv", "start");
            if (!string.IsNullOrEmpty(roomName)) {
                msg["cid"] = roomName;
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
            return new LobbyRoomResult {
                RoomId = res["cid"].ToString(),
                PrimaryUrl = res["addr"].ToString(),
                SecondaryUrl = res["secureAddr"].ToString()
            };
        }

        internal async Task<LobbyRoomResult> JoinRoom(string roomName, List<string> expectedUserIds) {
            var msg = Message.NewRequest("conv", "add");
            msg["cid"] = roomName;
            if (expectedUserIds != null) {
                List<object> expecteds = expectedUserIds.Cast<object>().ToList();
                msg["expectMembers"] = expecteds;
            }
            var res = await Send(msg);
            return new LobbyRoomResult {
                RoomId = res["cid"].ToString(),
                PrimaryUrl = res["addr"].ToString(),
                SecondaryUrl = res["secureAddr"].ToString()
            };
        }

        internal async Task<LobbyRoomResult> RejoinRoom(string roomName) {
            var msg = Message.NewRequest("conv", "add");
            msg["cid"] = roomName;
            msg["rejoin"] = true;
            var res = await Send(msg);
            return new LobbyRoomResult {
                RoomId = res["cid"].ToString(),
                PrimaryUrl = res["addr"].ToString(),
                SecondaryUrl = res["secureAddr"].ToString()
            };
        }

        internal async Task<LobbyRoomResult> JoinRandomRoom(Dictionary<string, object> matchProperties, List<string> expectedUserIds) {
            var msg = Message.NewRequest("conv", "add-random");
            if (matchProperties != null) {
                msg["expectAttr"] = matchProperties;
            }
            if (expectedUserIds != null) {
                msg["expectMembers"] = expectedUserIds;
            }
            var res = await Send(msg);
            return new LobbyRoomResult {
                RoomId = res["cid"].ToString(),
                PrimaryUrl = res["addr"].ToString(),
                SecondaryUrl = res["secureAddr"].ToString()
            };
        }

        internal async Task<LobbyRoomResult> JoinOrCreateRoom(string roomName, RoomOptions roomOptions, List<string> expectedUserIds)  {
            var msg = Message.NewRequest("conv", "add");
            msg["cid"] = roomName;
            msg["createOnNotFound"] = true;
            if (roomOptions != null) {
                var roomOptionsDict = roomOptions.ToDictionary();
                foreach (var entry in roomOptionsDict) {
                    msg[entry.Key] = entry.Value;
                }
            }
            if (expectedUserIds != null) {
                List<object> expecteds = expectedUserIds.Cast<object>().ToList();
                msg["expectMembers"] = expecteds;
            }
            var res = await Send(msg);
            return new LobbyRoomResult {
                Create = res.Op == "started",
                RoomId = res["cid"].ToString(),
                PrimaryUrl = res["addr"].ToString(),
                SecondaryUrl = res["secureAddr"].ToString()
            };
        }

        internal async Task<LobbyRoom> MatchRandom(Dictionary<string, object> matchProperties, List<string> expectedUserIds) {
            var msg = Message.NewRequest("conv", "match-random");
            if (matchProperties != null) {
                msg["expectAttr"] = matchProperties;
            }
            if (expectedUserIds != null) {
                msg["expectMembers"] = expectedUserIds;
            }
            var res = await Send(msg);
            return LobbyRoom.NewFromDictionary(res.Data);
        }

        protected override int GetPingDuration() {
            return 20;
        }
    }
}