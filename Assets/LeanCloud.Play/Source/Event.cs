using System;

namespace LeanCloud.Play {
    /// <summary>
    /// 事件
    /// </summary>
	public enum Event {
		/// <summary>
        /// 连接成功
        /// </summary>
		CONNECTED = 1,
        /// <summary>
        /// 连接失败
        /// </summary>
		CONNECT_FAILED = 2,
        /// <summary>
        /// 断开连接
        /// </summary>
		DISCONNECTED = 3,
        /// <summary>
        /// 加入大厅
        /// </summary>
		LOBBY_JOINED = 4,
        /// <summary>
        /// 离开大厅
        /// </summary>
        LOBBY_LEFT = 5,
        /// <summary>
        /// 大厅房间列表变化
        /// </summary>
        LOBBY_ROOM_LIST_UPDATED = 6,
        /// <summary>
        /// 创建房间成功
        /// </summary>
        ROOM_CREATED = 7,
        /// <summary>
        /// 创建房间失败
        /// </summary>
        ROOM_CREATE_FAILED = 8,
        /// <summary>
        /// 加入房间成功
        /// </summary>
        ROOM_JOINED = 9,
        /// <summary>
        /// 加入房间失败
        /// </summary>
        ROOM_JOIN_FAILED = 10,
        /// <summary>
        /// 有新玩家加入房间
        /// </summary>
        PLAYER_ROOM_JOINED = 11,
        /// <summary>
        /// 有玩家离开房间
        /// </summary>
        PLAYER_ROOM_LEFT = 12,
        /// <summary>
        /// 玩家活跃属性变化
        /// </summary>
        PLAYER_ACTIVITY_CHANGED = 13,
        /// <summary>
        /// 主机变更
        /// </summary>
        MASTER_SWITCHED = 14,
        /// <summary>
        /// 房间「开启 / 关闭」
        /// </summary>
        ROOM_OPEN_CHANGED = 15,
        /// <summary>
        /// 房间「可见 / 不可见」
        /// </summary>
        ROOM_VISIBLE_CHANGED = 16,
        /// <summary>
        /// 离开房间
        /// </summary>
        ROOM_LEFT = 17,
        /// <summary>
        /// 房间自定义属性变化
        /// </summary>
        ROOM_CUSTOM_PROPERTIES_CHANGED = 18,
        /// <summary>
        /// 玩家自定义属性变化
        /// </summary>
        PLAYER_CUSTOM_PROPERTIES_CHANGED = 19,
        /// <summary>
        /// 自定义事件
        /// </summary>
        CUSTOM_EVENT = 20,
        /// <summary>
        /// 错误事件
        /// </summary>
		ERROR = 21,
	}
}
