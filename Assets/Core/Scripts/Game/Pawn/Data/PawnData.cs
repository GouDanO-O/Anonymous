using System;
using Core.Game.Pawn.Define;

namespace Core.Game.Pawn.Data
{
    /// <summary>
    /// Pawn核心数据
    /// </summary>
    [Serializable]
    public class PawnData
    {
        #region 基础属性

        /// <summary>
        /// 唯一ID
        /// </summary>
        public int PawnId;

        /// <summary>
        /// 配置定义名（关联Luban PawnKindDef）
        /// </summary>
        public string DefName;

        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName;

        #endregion

        #region 逻辑位置（格子级）

        /// <summary>
        /// 当前所在格子X
        /// </summary>
        public int CellX;

        /// <summary>
        /// 当前所在格子Y
        /// </summary>
        public int CellY;

        /// <summary>
        /// 当前楼层
        /// </summary>
        public int Floor;

        #endregion

        #region 视觉位置（浮点，用于平滑插值）

        /// <summary>
        /// 视觉X坐标（格子坐标系，浮点精度）
        /// </summary>
        public float VisualX;

        /// <summary>
        /// 视觉Y坐标
        /// </summary>
        public float VisualY;

        #endregion

        #region 状态

        /// <summary>
        /// 当前状态
        /// </summary>
        public EPawnState State;

        /// <summary>
        /// 朝向
        /// </summary>
        public EFacingDirection Facing;

        /// <summary>
        /// 是否是固定不动的Pawn（用于渲染优化）
        /// </summary>
        public bool IsStatic;

        #endregion

        #region 属性

        /// <summary>
        /// 移动速度（格/秒）
        /// </summary>
        public float MoveSpeed;

        /// <summary>
        /// 最大生命值
        /// </summary>
        public int MaxHealth;

        /// <summary>
        /// 当前生命值
        /// </summary>
        public int CurrentHealth;

        #endregion

        #region 路径状态

        /// <summary>
        /// 移动路径数据
        /// </summary>
        public PawnPathState PathState;

        #endregion

        #region 构造函数

        public PawnData()
        {
            PathState = new PawnPathState();
            State = EPawnState.Idle;
            Facing = EFacingDirection.South;
            MoveSpeed = PawnDefine.DefaultMoveSpeed;
            IsStatic = false;
        }

        public PawnData(int pawnId, string defName, int cellX, int cellY, int floor) : this()
        {
            PawnId = pawnId;
            DefName = defName;
            CellX = cellX;
            CellY = cellY;
            Floor = floor;
            VisualX = cellX;
            VisualY = cellY;
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置逻辑位置
        /// </summary>
        public void SetCellPosition(int x, int y, int floor)
        {
            CellX = x;
            CellY = y;
            Floor = floor;
        }

        /// <summary>
        /// 设置视觉位置
        /// </summary>
        public void SetVisualPosition(float x, float y)
        {
            VisualX = x;
            VisualY = y;
        }

        /// <summary>
        /// 将视觉位置对齐到逻辑位置
        /// </summary>
        public void SnapVisualToCell()
        {
            VisualX = CellX;
            VisualY = CellY;
        }

        /// <summary>
        /// 是否存活
        /// </summary>
        public bool IsAlive => State != EPawnState.Dead;

        /// <summary>
        /// 是否可以移动
        /// </summary>
        public bool CanMove => State == EPawnState.Idle || State == EPawnState.Moving;

        public override string ToString()
        {
            return $"Pawn({PawnId}, {DefName}, Cell({CellX},{CellY},F{Floor}), {State})";
        }

        #endregion
    }
}
