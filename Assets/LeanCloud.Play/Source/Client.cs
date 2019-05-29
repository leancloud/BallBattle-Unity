using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace LeanCloud.Play {
    public class Client {
        // 事件
        public event Action<List<LobbyRoom>> OnLobbyRoomListUpdated;
        public event Action<Player> OnPlayerRoomJoined;
        public event Action<Player> OnPlayerRoomLeft;
        public event Action<Player> OnMasterSwitched;
        public event Action<bool> OnRoomOpenChanged;
        public event Action<bool> OnRoomVisibleChanged;
        public event Action<Dictionary<string, object>> OnRoomCustomPropertiesChanged;
        public event Action<Player, Dictionary<string, object>> OnPlayerCustomPropertiesChanged;
        public event Action<Player> OnPlayerActivityChanged;
        public event Action<byte, Dictionary<string, object>, int> OnCustomEvent;
        public event Action<int, string> OnRoomKicked;
        public event Action OnDisconnected;
        public event Action<int, string> OnError;

        readonly PlayContext context;

        public string AppId {
            get; private set;
        }

        public string AppKey {
            get; private set;
        }

        public string UserId {
            get; private set;
        }

        public bool Ssl {
            get; private set;
        }

        public string GameVersion {
            get; private set;
        }

        PlayRouter playRouter;
        LobbyRouter lobbyRouter;
        LobbyConnection lobbyConn;
        GameConnection gameConn;

        PlayState state;

        public List<LobbyRoom> LobbyRoomList;

        public Room Room {
            get; private set;
        }

        public Player Player {
            get; internal set;
        }

        public Client(string appId, string appKey, string userId, bool ssl = true, string gameVersion = "0.0.1", string playServer = null) {
            AppId = appId;
            AppKey = appKey;
            UserId = userId;
            Ssl = ssl;
            GameVersion = gameVersion;

            state = PlayState.INIT;
            Logger.Debug("start at {0}", Thread.CurrentThread.ManagedThreadId);

            var playGO = new GameObject("LeanCloud.Play");
            UnityEngine.Object.DontDestroyOnLoad(playGO);
            context = playGO.AddComponent<PlayContext>();

            playRouter = new PlayRouter(appId, playServer);
            lobbyRouter = new LobbyRouter(appId, false, null);
            lobbyConn = new LobbyConnection();
            gameConn = new GameConnection();
        }

        public async Task<Client> Connect() {
            if (state == PlayState.CONNECTING) {
                // 
                Logger.Debug("it is connecting...");
                return null;
            }
            if (state != PlayState.INIT && state != PlayState.DISCONNECT) {
                throw new PlayException(PlayExceptionCode.StateError,
                    string.Format("You cannot call Connect() on {0} state", state.ToString()));
            }
            try {
                state = PlayState.CONNECTING;
                lobbyConn = await ConnectLobby();
                state = PlayState.LOBBY;
                Logger.Debug("connected at: {0}", Thread.CurrentThread.ManagedThreadId);
                lobbyConn.OnMessage += OnLobbyConnMessage;
                lobbyConn.OnClose += OnLobbyConnClose;
                return this;
            } catch (Exception e) {
                state = PlayState.INIT;
                throw e;
            }
        }

        public async Task JoinLobby() {
            if (state != PlayState.LOBBY) {
                throw new PlayException(PlayExceptionCode.StateError,
                    string.Format("You cannot call JoinLobby() on {0} state", state.ToString()));
            }
            await lobbyConn.JoinLobby();
        }

        public async Task<Room> CreateRoom(string roomName = null, RoomOptions roomOptions = null, List<string> expectedUserIds = null) {
            if (state != PlayState.LOBBY) {
                throw new PlayException(PlayExceptionCode.StateError,
                    string.Format("You cannot call CreateRoom() on {0} state", state.ToString()));
            }
            try {
                state = PlayState.LOBBY_TO_GAME;
                var lobbyRoom = await lobbyConn.CreateRoom(roomName, roomOptions, expectedUserIds);
                var roomId = lobbyRoom.RoomId;
                var server = lobbyRoom.PrimaryUrl;
                gameConn = await GameConnection.Connect(AppId, server, UserId, GameVersion);
                var room = await gameConn.CreateRoom(roomId, roomOptions, expectedUserIds);
                LobbyToGame(gameConn, room);
                return room;
            } catch (Exception e) {
                state = PlayState.LOBBY;
                throw e;
            }
        }

        public async Task<Room> JoinRoom(string roomName, List<string> expectedUserIds = null) {
            if (state != PlayState.LOBBY) {
                throw new PlayException(PlayExceptionCode.StateError,
                    string.Format("You cannot call JoinRoom() on {0} state", state.ToString()));
            }
            try {
                state = PlayState.LOBBY_TO_GAME;
                var lobbyRoom = await lobbyConn.JoinRoom(roomName, expectedUserIds);
                var roomId = lobbyRoom.RoomId;
                var server = lobbyRoom.PrimaryUrl;
                gameConn = await GameConnection.Connect(AppId, server, UserId, GameVersion);
                Room = await gameConn.JoinRoom(roomId, expectedUserIds);
                LobbyToGame(gameConn, Room);
                return Room;
            } catch (Exception e) {
                state = PlayState.LOBBY;
                throw e;
            }
        }

        public async Task<Room> RejoinRoom(string roomName) {
            if (state != PlayState.LOBBY) {
                throw new PlayException(PlayExceptionCode.StateError,
                    string.Format("You cannot call RejoinRoom() on {0} state", state.ToString()));
            }
            try {
                state = PlayState.LOBBY_TO_GAME;
                var lobbyRoom = await lobbyConn.RejoinRoom(roomName);
                var roomId = lobbyRoom.RoomId;
                var server = lobbyRoom.PrimaryUrl;
                gameConn = await GameConnection.Connect(AppId, server, UserId, GameVersion);
                Room = await gameConn.JoinRoom(roomId, null);
                LobbyToGame(gameConn, Room);
                return Room;
            } catch (Exception e) {
                state = PlayState.LOBBY;
                throw e;
            }
        }

        public async Task<Room> JoinOrCreateRoom(string roomName, RoomOptions roomOptions = null, List<string> expectedUserIds = null) {
            if (state != PlayState.LOBBY) {
                throw new PlayException(PlayExceptionCode.StateError,
                    string.Format("You cannot call JoinOrCreateRoom() on {0} state", state.ToString()));
            }
            try {
                state = PlayState.LOBBY_TO_GAME;
                var lobbyRoom = await lobbyConn.JoinOrCreateRoom(roomName, roomOptions, expectedUserIds);
                var create = lobbyRoom.Create;
                var roomId = lobbyRoom.RoomId;
                var server = lobbyRoom.PrimaryUrl;
                gameConn = await GameConnection.Connect(AppId, server, UserId, GameVersion);
                if (create) {
                    Room = await gameConn.CreateRoom(roomId, roomOptions, expectedUserIds);
                } else {
                    Room = await gameConn.JoinRoom(roomId, expectedUserIds);
                }
                LobbyToGame(gameConn, Room);
                return Room;
            } catch (Exception e) {
                state = PlayState.LOBBY;
                throw e;
            }
        }

        public async Task<Room> JoinRandomRoom(Dictionary<string, object> matchProperties = null, List<string> expectedUserIds = null) {
            if (state != PlayState.LOBBY) {
                throw new PlayException(PlayExceptionCode.StateError,
                    string.Format("You cannot call JoinRandomRoom() on {0} state", state.ToString()));
            }
            try {
                state = PlayState.LOBBY_TO_GAME;
                var lobbyRoom = await lobbyConn.JoinRandomRoom(matchProperties, expectedUserIds);
                var roomId = lobbyRoom.RoomId;
                var server = lobbyRoom.PrimaryUrl;
                gameConn = await GameConnection.Connect(AppId, server, UserId, GameVersion);
                Room = await gameConn.JoinRoom(roomId, expectedUserIds);
                LobbyToGame(gameConn, Room);
                return Room;
            } catch (Exception e) {
                state = PlayState.LOBBY;
                throw e;
            }
        }

        public async Task<Room> ReconnectAndRejoin() {
            if (state != PlayState.DISCONNECT) {
                throw new PlayException(PlayExceptionCode.StateError,
                    string.Format("You cannot call ReconnectAndRejoin() on {0} state", state.ToString()));
            }
            if (Room == null) {
                throw new ArgumentNullException(nameof(Room));
            }
            await Connect();
            Logger.Debug("Connect at {0}", Thread.CurrentThread.ManagedThreadId);
            var room = await RejoinRoom(Room.Name);
            Logger.Debug("Rejoin at {0}", Thread.CurrentThread.ManagedThreadId);
            return room;
        }

        public async Task<LobbyRoom> MatchRandom(Dictionary<string, object> matchProperties = null, List<string> expectedUserIds = null) {
            if (state != PlayState.LOBBY) {
                throw new PlayException(PlayExceptionCode.StateError,
                    string.Format("You cannot call MatchRandom() on {0} state", state.ToString()));
            }
            var lobbyRoom = await lobbyConn.MatchRandom(matchProperties, expectedUserIds);
            return lobbyRoom;
        }

        public async Task LeaveRoom() {
            if (state != PlayState.GAME) {
                throw new PlayException(PlayExceptionCode.StateError,
                    string.Format("You cannot call LeaveRoom() on {0} state", state.ToString()));
            }
            state = PlayState.GAME_TO_LOBBY;
            try {
                await gameConn.LeaveRoom();
            } catch (Exception e) {
                state = PlayState.GAME;
                throw e;
            }
            try {
                lobbyConn = await ConnectLobby();
                GameToLobby(lobbyConn);
            } catch (Exception e) {
                state = PlayState.INIT;
                throw e;
            }
        }

        public async Task<bool> SetRoomOpened(bool opened) {
            if (state != PlayState.GAME) {
                throw new PlayException(PlayExceptionCode.StateError,
                    string.Format("You cannot call SetRoomOpened() on {0} state", state.ToString()));
            }
            Room.Opened = await gameConn.SetRoomOpened(opened);
            return Room.Opened;
        }

        public async Task<bool> SetRoomVisible(bool visible) {
            if (state != PlayState.GAME) {
                throw new PlayException(PlayExceptionCode.StateError,
                    string.Format("You cannot call SetRoomVisible() on {0} state", state.ToString()));
            }
            Room.Visible = await gameConn.SetRoomVisible(visible);
            return Room.Visible;
        }

        public async Task<Player> SetMaster(int newMasterId) {
            if (state != PlayState.GAME) {
                throw new PlayException(PlayExceptionCode.StateError,
                    string.Format("You cannot call SetMaster() on {0} state", state.ToString()));
            }
            Room.MasterActorId = await gameConn.SetMaster(newMasterId);
            return Room.Master;
        }

        public async Task KickPlayer(int actorId, int code = 0, string reason = null) {
            if (state != PlayState.GAME) {
                throw new PlayException(PlayExceptionCode.StateError,
                    string.Format("You cannot call KickPlayer() on {0} state", state.ToString()));
            }
            var playerId = await gameConn.KickPlayer(actorId, code, reason);
            Room.RemovePlayer(playerId);
        }

        public Task SendEvent(byte eventId, Dictionary<string, object> eventData = null, SendEventOptions options = null) {
            if (state != PlayState.GAME) {
                throw new PlayException(PlayExceptionCode.StateError,
                    string.Format("You cannot call SendEvent() on {0} state", state.ToString()));
            }
            var opts = options;
            if (opts == null) {
                opts = new SendEventOptions {
                    ReceiverGroup = ReceiverGroup.All
                };
            }
            gameConn.SendEvent(eventId, eventData, opts).OnSuccess(_ => { });
            return Task.FromResult(true);
        }

        public async Task SetRoomCustomProperties(Dictionary<string, object> properties, Dictionary<string, object> expectedValues = null) {
            if (state != PlayState.GAME) {
                throw new PlayException(PlayExceptionCode.StateError,
                    string.Format("You cannot call SetRoomCustomProperties() on {0} state", state.ToString()));
            }
            var changedProps = await gameConn.SetRoomCustomProperties(properties, expectedValues);
            Room.MergeProperties(changedProps);
        }

        public async Task SetPlayerCustomProperties(int actorId, Dictionary<string, object> properties, Dictionary<string, object> expectedValues = null) {
            if (state != PlayState.GAME) {
                throw new PlayException(PlayExceptionCode.StateError,
                    string.Format("You cannot call SetPlayerCustomProperties() on {0} state", state.ToString()));
            }
            var res = await gameConn.SetPlayerCustomProperties(actorId, properties, expectedValues);
            if (res != null) {
                // 如果不为空，则进行属性更新
                var aId = int.Parse(res["actorId"].ToString());
                var player = Room.GetPlayer(aId);
                player.MergeProperties(res["changedProps"] as Dictionary<string, object>);
            }
        }

        public void PauseMessageQueue() { 
            if (state == PlayState.LOBBY) {
                lobbyConn.PauseMessageQueue();
            } else if (state == PlayState.GAME) {
                gameConn.PauseMessageQueue();
            }
        }

        public void ResumeMessageQueue() {
            if (state == PlayState.LOBBY) {
                lobbyConn.ResumeMessageQueue();
            } else if (state == PlayState.GAME) {
                gameConn.ResumeMessageQueue();
            }
        }

        public void Close() {
            if (state == PlayState.LOBBY) {
                lobbyConn.Close();
            } else if (state == PlayState.GAME) {
                gameConn.Close();
            }
            state = PlayState.CLOSE;
        }

        void OnLobbyConnMessage(Message msg) {
            context.Post(() => { 
                switch (msg.Cmd) {
                    case "lobby":
                        switch (msg.Op) {
                            case "room-list":
                                HandleRoomListMsg(msg);
                                break;
                            default:
                                HandleUnknownMsg(msg);
                                break;
                        }
                        break;
                    case "events":
                        break;
                    case "statistic":
                        break;
                    case "conv":
                        break;
                    case "error":
                        HandleErrorMsg(msg);
                        break;
                    default:
                        HandleUnknownMsg(msg);
                        break;
                }
            });
        }

        void HandleRoomListMsg(Message msg) {
            LobbyRoomList = new List<LobbyRoom>();
            if (msg.Data.TryGetValue("list", out object roomsObj)) {
                List<object> rooms = roomsObj as List<object>;
                foreach (Dictionary<string, object> room in rooms) {
                    var lobbyRoom = LobbyRoom.NewFromDictionary(room);
                    LobbyRoomList.Add(lobbyRoom);
                }
            }
            OnLobbyRoomListUpdated?.Invoke(LobbyRoomList);
        }

        void HandleErrorMsg(Message msg) {
            Logger.Error("error msg: {0}", msg.ToJson());
            if (msg.TryGetValue("reasonCode", out object codeObj) &&
                int.TryParse(codeObj.ToString(), out int code)) {
                var detail = msg["detail"] as string;
                OnError?.Invoke(code, detail); 
            }
        }

        void HandleUnknownMsg(Message msg) {
            try {
                Logger.Error("unknown msg: {0}", msg);
            } catch (Exception e) {
                Logger.Error(e.Message);
            }
        }

        void OnLobbyConnClose(int code, string reason) {
            context.Post(() => {
                state = PlayState.DISCONNECT;
                OnDisconnected?.Invoke();
            });
        }

        void OnGameConnMessage(Message msg) {
            context.Post(() => {
                switch (msg.Cmd) {
                    case "conv":
                        switch (msg.Op) {
                            case "members-joined":
                                HandlePlayerJoinedRoom(msg);
                                break;
                            case "members-left":
                                HandlePlayerLeftRoom(msg);
                                break;
                            case "master-client-changed":
                                HandleMasterChanged(msg);
                                break;
                            case "opened-notify":
                                HandleRoomOpenChanged(msg);
                                break;
                            case "visible-notify":
                                HandleRoomVisibleChanged(msg);
                                break;
                            case "updated-notify":
                                HandleRoomCustomPropertiesChanged(msg);
                                break;
                            case "player-props":
                                HandlePlayerCustomPropertiesChanged(msg);
                                break;
                            case "members-offline":
                                HandlePlayerOffline(msg);
                                break;
                            case "members-online":
                                HandlePlayerOnline(msg);
                                break;
                            case "kicked-notice":
                                HandleRoomKicked(msg);
                                break;
                            default:
                                HandleUnknownMsg(msg);
                                break;
                        }
                        break;
                    case "events":
                        break;
                    case "direct":
                        HandleSendEvent(msg);
                        break;
                    case "error":
                        HandleErrorMsg(msg);
                        break;
                    default:
                        HandleUnknownMsg(msg);
                        break;
                }
            });
        }   

        void OnGameConnClose(int code, string reason) {
            context.Post(() => {
                state = PlayState.DISCONNECT;
                OnDisconnected?.Invoke();
            });
        }

        void HandlePlayerJoinedRoom(Message msg) { 
            if (msg.TryGetValue("member", out object playerObj)) {
                var player = Player.NewFromDictionary(playerObj as Dictionary<string, object>);
                player.Client = this;
                Room.AddPlayer(player);
                OnPlayerRoomJoined?.Invoke(player);
            } else {
                Logger.Error("Handle player joined room error: {0}", msg.ToJson());
            }
        }

        void HandlePlayerLeftRoom(Message msg) { 
            if (msg.TryGetValue("actorId", out object playerIdObj) &&
                int.TryParse(playerIdObj.ToString(), out int playerId)) {
                try {
                    var leftPlayer = Room.GetPlayer(playerId);
                    Room.RemovePlayer(playerId);
                    OnPlayerRoomLeft?.Invoke(leftPlayer);
                } catch (Exception e) {
                    Logger.Error(e.Message);
                }
            } else {
                Logger.Error("Handle player left room error: {0}", msg.ToJson());
            }
        }

        void HandleMasterChanged(Message msg) {
            if (msg.Data.ContainsKey("masterActorId")) {
                if (msg.Data["masterActorId"] != null &&
                    msg.TryGetValue("masterActorId", out object newMasterIdObj) &&
                    int.TryParse(newMasterIdObj.ToString(), out int newMasterId)) {
                    Room.MasterActorId = newMasterId;
                    var newMaster = Room.GetPlayer(newMasterId);
                    OnMasterSwitched?.Invoke(newMaster);
                } else {
                    Room.MasterActorId = -1;
                    OnMasterSwitched?.Invoke(null);
                }
            } else {
                Logger.Error("Handle room open changed error: {0}", msg.ToJson());
            }
        }

        void HandleRoomOpenChanged(Message msg) { 
            if (msg.TryGetValue("toggle", out object openedObj) &&
                bool.TryParse(openedObj.ToString(), out bool opened)) {
                Room.Opened = opened;
                OnRoomOpenChanged?.Invoke(opened);
            } else {
                Logger.Error("Handle room open changed error: {0}", msg.ToJson());
            }
        }

        void HandleRoomVisibleChanged(Message msg) { 
            if (msg.TryGetValue("toggle", out object visibleObj) &&
                bool.TryParse(visibleObj.ToString(), out bool visible)) {
                Room.Visible = visible;
                OnRoomVisibleChanged?.Invoke(visible);
            } else {
                Logger.Error("Handle room visible changed error: {0}", msg.ToJson());
            }
        }

        void HandleRoomCustomPropertiesChanged(Message msg) { 
            if (msg.TryGetValue("attr", out object attrObj)) {
                var changedProps = attrObj as Dictionary<string, object>;
                Room.MergeProperties(changedProps);
                OnRoomCustomPropertiesChanged?.Invoke(changedProps);
            } else {
                Logger.Error("Handle room custom properties changed error: {0}", msg.ToJson());
            }
        }

        void HandlePlayerCustomPropertiesChanged(Message msg) { 
            if (msg.TryGetValue("actorId", out object playerIdObj) && 
                int.TryParse(playerIdObj.ToString(), out int playerId) &&
                msg.TryGetValue("attr", out object attrObj)) {
                var player = Room.GetPlayer(playerId);
                if (player == null) {
                    Logger.Error("No player id: {0} when player properties changed", msg.ToJson());
                    return;
                }
                var changedProps = attrObj as Dictionary<string, object>;
                player.MergeProperties(changedProps);
                OnPlayerCustomPropertiesChanged?.Invoke(player, changedProps);
            } else {
                Logger.Error("Handle player custom properties changed error: {0}", msg.ToJson());
            }
        }

        void HandlePlayerOffline(Message msg) {
            if (msg.TryGetValue("initByActor", out object playerIdObj) &&
                int.TryParse(playerIdObj.ToString(), out int playerId)) {
                var player = Room.GetPlayer(playerId);
                if (player == null) {
                    Logger.Error("No player id: {0} when player is offline");
                    return;
                }
                player.IsActive = false;
                OnPlayerActivityChanged?.Invoke(player);
            } else {
                Logger.Error("Handle player offline error: {0}", msg.ToJson());
            }
        }

        void HandlePlayerOnline(Message msg) { 
            if (msg.TryGetValue("member", out object memberObj)) {
                var member = memberObj as Dictionary<string, object>;
                if (member.TryGetValue("actorId", out object playerIdObj) &&
                    int.TryParse(playerIdObj.ToString(), out int playerId)) {
                    var player = Player.NewFromDictionary(member);
                    player.Client = this;
                    OnPlayerActivityChanged?.Invoke(player);
                } else {
                    Logger.Error("Handle player online error: {0}", msg.ToJson());
                }
            } else {
                Logger.Error("Handle player online error: {0}", msg.ToJson());
            }
        }

        void HandleSendEvent(Message msg) { 
            if (msg.TryGetValue("eventId", out object eventIdObj) && 
                byte.TryParse(eventIdObj.ToString(), out byte eventId)) {
                var senderId = -1;
                if (msg.TryGetValue("fromActorId", out object senderIdObj)) {
                    int.TryParse(senderIdObj.ToString(), out senderId);
                }
                Dictionary<string, object> eventData = null;
                if (msg.TryGetValue("msg", out object eventDataObj)) {
                    eventData = eventDataObj as Dictionary<string, object>;
                }
                OnCustomEvent?.Invoke(eventId, eventData, senderId);
            } else {
                Logger.Error("Handle custom event error: {0}", msg.ToJson());
            }
        }

        void HandleRoomKicked(Message msg) {
            state = PlayState.GAME_TO_LOBBY;
            // 建立连接
            ConnectLobby().ContinueWith(t => {
                context.Post(() => {
                    if (t.IsFaulted) {
                        state = PlayState.INIT;
                        throw t.Exception.InnerException;
                    }
                    GameToLobby(t.Result);
                    int code = -1;
                    string reason = string.Empty;
                    if (msg.TryGetValue("appCode", out object codeObj) &&
                        int.TryParse(codeObj.ToString(), out code)) { }
                    if (msg.TryGetValue("appMsg", out object reasonObj)) {
                        reason = reasonObj.ToString();
                    }
                    OnRoomKicked?.Invoke(code, reason);
                });
            });
        }

        Task<LobbyConnection> ConnectLobby() {
            return playRouter.Fetch().OnSuccess(t => {
                var serverUrl = t.Result;
                Logger.Debug("play server: {0} at {1}", serverUrl, Thread.CurrentThread.ManagedThreadId);
                return lobbyRouter.Fetch(serverUrl);
            }).Unwrap().OnSuccess(t => {
                var lobbyUrl = t.Result;
                Logger.Debug("wss server: {0} at {1}", lobbyUrl, Thread.CurrentThread.ManagedThreadId);
                return LobbyConnection.Connect(AppId, lobbyUrl, UserId, GameVersion);
            }).Unwrap();
        }

        void LobbyToGame(GameConnection gc, Room room) {
            state = PlayState.GAME;
            lobbyConn.OnMessage -= OnLobbyConnMessage;
            lobbyConn.OnClose -= OnLobbyConnClose;
            lobbyConn.Close();
            gameConn = gc;
            gameConn.OnMessage += OnGameConnMessage;
            gameConn.OnClose += OnGameConnClose;
            Room = room;
            Room.Client = this;
            foreach (var player in Room.PlayerList) { 
                if (player.UserId == UserId) {
                    Player = player;
                }
                player.Client = this;
            }
        }

        void GameToLobby(LobbyConnection lc) {
            state = PlayState.LOBBY;
            gameConn.OnMessage -= OnGameConnMessage;
            gameConn.OnClose -= OnGameConnClose;
            gameConn.Close();
            Logger.Debug("connected at: {0}", Thread.CurrentThread.ManagedThreadId);
            lobbyConn = lc;
            lobbyConn.OnMessage += OnLobbyConnMessage;
            lobbyConn.OnClose += OnLobbyConnClose;
        }

        // 调试时模拟断线
        public void _Disconnect() { 
            if (state == PlayState.LOBBY) {
                lobbyConn.Disconnect();
            } else if (state == PlayState.GAME) {
                gameConn.Disconnect();
            }
        }
    }
}
