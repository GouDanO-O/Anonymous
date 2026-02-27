namespace Core.Game.Item.Data
{
    /// <summary>
    /// 单个物品/家具的数据
    /// </summary>
    public class ItemData
    {
        public long ItemId;
        public int ItemDefId;

        /// <summary>
        /// 锚点坐标 (左下角)
        /// </summary>
        public int AnchorX;
        public int AnchorY;
        public int Floor;

        /// <summary>
        /// 实际占用尺寸 (旋转后)
        /// </summary>
        public int Width;
        public int Height;

        /// <summary>
        /// 旋转角度: 0, 90, 180, 270
        /// </summary>
        public int Rotation;

        public int Health;
    }
}
