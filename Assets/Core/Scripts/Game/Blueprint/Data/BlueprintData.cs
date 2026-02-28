using Core.Game.Blueprint.Define;

namespace Core.Game.Blueprint.Data
{
    /// <summary>
    /// 蓝图数据: 表示一个待建造/拆除/拆卸的指令
    /// </summary>
    public class BlueprintData
    {
        public long BlueprintId;
        public EBlueprintType Type;
        public EBlueprintState State;

        /// <summary>
        /// 目标Def ID (StructureDefId 或 ItemDefId)
        /// </summary>
        public int DefId;

        public int X;
        public int Y;
        public int Floor;

        /// <summary>
        /// 家具旋转 (0/90/180/270)
        /// </summary>
        public int Rotation;

        /// <summary>
        /// 总工作量
        /// </summary>
        public float WorkRequired;

        /// <summary>
        /// 已完成工作量
        /// </summary>
        public float WorkDone;

        /// <summary>
        /// 指派的Pawn ID (0=未指派)
        /// </summary>
        public long AssignedPawnId;

        /// <summary>
        /// 拆卸重装时: 源物品ID
        /// </summary>
        public long SourceItemId;

        /// <summary>
        /// 工作进度 (0~1)
        /// </summary>
        public float Progress => WorkRequired > 0 ? WorkDone / WorkRequired : 0f;

        public bool IsComplete => WorkDone >= WorkRequired;
    }
}
