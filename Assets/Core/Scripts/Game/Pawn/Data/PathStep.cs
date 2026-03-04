namespace Core.Game.Pawn.Data
{
    /// <summary>
    /// 多层路径节点: 包含楼层信息和楼梯过渡标记
    /// </summary>
    public struct PathStep
    {
        public int X;
        public int Y;
        public int Floor;

        /// <summary>
        /// 是否为楼梯过渡步骤 (楼层切换)
        /// </summary>
        public bool IsStairTransition;

        public PathStep(int x, int y, int floor, bool isStairTransition = false)
        {
            X = x;
            Y = y;
            Floor = floor;
            IsStairTransition = isStairTransition;
        }
    }
}
