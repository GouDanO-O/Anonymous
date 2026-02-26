using GDFrameworkExtend.PoolKit;

namespace Core.Game.Navigation.Data
{
    /// <summary>
    /// A*寻路节点（支持对象池复用）
    /// </summary>
    public class PathNode : IPoolable
    {
        #region 位置

        /// <summary>
        /// 格子X坐标
        /// </summary>
        public int X;

        /// <summary>
        /// 格子Y坐标
        /// </summary>
        public int Y;

        /// <summary>
        /// 楼层
        /// </summary>
        public int Floor;

        #endregion

        #region A*代价

        /// <summary>
        /// 从起点到当前节点的实际代价
        /// </summary>
        public int G;

        /// <summary>
        /// 从当前节点到终点的启发式估计代价
        /// </summary>
        public int H;

        /// <summary>
        /// 总代价 F = G + H
        /// </summary>
        public int F => G + H;

        #endregion

        #region 链表

        /// <summary>
        /// 父节点（用于回溯路径）
        /// </summary>
        public PathNode Parent;

        /// <summary>
        /// 是否在开放列表中
        /// </summary>
        public bool IsInOpenList;

        /// <summary>
        /// 是否在关闭列表中
        /// </summary>
        public bool IsInClosedList;

        #endregion

        #region 对象池

        public bool IsRecycled { get; set; }

        public void OnRecycled()
        {
            X = 0;
            Y = 0;
            Floor = 0;
            G = 0;
            H = 0;
            Parent = null;
            IsInOpenList = false;
            IsInClosedList = false;
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置节点位置
        /// </summary>
        public void SetPosition(int x, int y, int floor)
        {
            X = x;
            Y = y;
            Floor = floor;
        }

        /// <summary>
        /// 设置代价
        /// </summary>
        public void SetCost(int g, int h)
        {
            G = g;
            H = h;
        }

        /// <summary>
        /// 生成节点的唯一Key（用于字典快速查找）
        /// floor * 1000000 + y * 1000 + x（支持最大1000×1000地图）
        /// </summary>
        public long GetKey()
        {
            return GetKey(X, Y, Floor);
        }

        /// <summary>
        /// 静态方法生成Key
        /// </summary>
        public static long GetKey(int x, int y, int floor)
        {
            return (long)floor * 1000000L + (long)y * 1000L + x;
        }

        public override string ToString()
        {
            return $"PathNode({X}, {Y}, F{Floor}) G={G} H={H} F={F}";
        }

        #endregion
    }
}
