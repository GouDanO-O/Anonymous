using System;

namespace Core.Game.Navigation.Data
{
    /// <summary>
    /// 寻路请求
    /// </summary>
    public class PathRequest
    {
        /// <summary>
        /// 起点X
        /// </summary>
        public int StartX;

        /// <summary>
        /// 起点Y
        /// </summary>
        public int StartY;

        /// <summary>
        /// 起点楼层
        /// </summary>
        public int StartFloor;

        /// <summary>
        /// 终点X
        /// </summary>
        public int EndX;

        /// <summary>
        /// 终点Y
        /// </summary>
        public int EndY;

        /// <summary>
        /// 终点楼层
        /// </summary>
        public int EndFloor;

        /// <summary>
        /// 寻路完成回调
        /// </summary>
        public Action<PathResult> Callback;

        /// <summary>
        /// 请求者ID（用于标识是哪个Pawn发出的请求）
        /// </summary>
        public int RequesterId;

        public PathRequest() { }

        public PathRequest(int startX, int startY, int startFloor,
            int endX, int endY, int endFloor,
            Action<PathResult> callback, int requesterId = -1)
        {
            StartX = startX;
            StartY = startY;
            StartFloor = startFloor;
            EndX = endX;
            EndY = endY;
            EndFloor = endFloor;
            Callback = callback;
            RequesterId = requesterId;
        }
    }
}
