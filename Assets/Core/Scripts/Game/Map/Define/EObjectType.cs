namespace Core.Game.Map.Define
{
    /// <summary>
    /// 物体类型
    /// </summary>
    public enum EObjectType : byte
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,

        /// <summary>
        /// 家具
        /// </summary>
        Furniture = 1,

        /// <summary>
        /// 建筑（蓝图占位）
        /// </summary>
        Blueprint = 2,

        /// <summary>
        /// 资源（树木、矿石等）
        /// </summary>
        Resource = 3,

        /// <summary>
        /// 容器
        /// </summary>
        Container = 4,

        /// <summary>
        /// 工作站
        /// </summary>
        Workstation = 5,

        /// <summary>
        /// 装饰物
        /// </summary>
        Decoration = 6,

        /// <summary>
        /// 植物
        /// </summary>
        Plant = 7,

        /// <summary>
        /// 掉落物品
        /// </summary>
        DroppedItem = 8
    }
}
