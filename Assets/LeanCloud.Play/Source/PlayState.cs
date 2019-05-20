using System;

namespace LeanCloud.Play
{
    /// <summary>
    /// 连接状态
    /// </summary>
    internal enum PlayState
    {
        /// <summary>
        /// 断开
        /// </summary>
        INIT = 0,
        /// <summary>
        /// 连接中
        /// </summary>
        CONNECTING = 1,
        /// <summary>
        /// 大厅连接成功
        /// </summary>
        LOBBY = 2,
        /// <summary>
        /// 大厅到游戏服务器的过渡
        /// </summary>
        LOBBY_TO_GAME = 3,
        /// <summary>
        /// 房间连接成功
        /// </summary>
        GAME = 4,
        /// <summary>
        /// 游戏到大厅服务器的过渡
        /// </summary>
        GAME_TO_LOBBY = 5,
        /// <summary>
        /// 断开连接
        /// </summary>
        DISCONNECT = 6,
        /// <summary>
        /// 断开
        /// </summary>
        CLOSE = 7,
    }
}
