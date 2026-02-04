using System;
using System.Collections.Generic;
using Core.Game.Map.Define;

namespace Core.Game.Map.Data
{
    /// <summary>
    /// 单个格子的数据
    /// </summary>
    [Serializable]
    public class CellData
    {
        #region 位置

        /// <summary>
        /// X坐标
        /// </summary>
        public int X;

        /// <summary>
        /// Y坐标
        /// </summary>
        public int Y;

        /// <summary>
        /// Z坐标（楼层）
        /// </summary>
        public int Z;

        #endregion

        #region 地面与地板

        /// <summary>
        /// 地面类型（自然地形）
        /// </summary>
        public EGroundType GroundType;

        /// <summary>
        /// 地板类型（人造地板）
        /// </summary>
        public EFloorType FloorType;

        #endregion

        #region 墙体

        /// <summary>
        /// 北墙（格子上边缘）
        /// </summary>
        public WallData WallNorth;

        /// <summary>
        /// 西墙（格子左边缘）
        /// </summary>
        public WallData WallWest;

        #endregion

        #region 物体

        /// <summary>
        /// 格子内的物体ID列表
        /// </summary>
        public List<int> ObjectIds;

        #endregion

        #region 标记与属性

        /// <summary>
        /// 格子标记
        /// </summary>
        public ECellFlags Flags;

        /// <summary>
        /// 所属房间ID（-1表示室外）
        /// </summary>
        public int RoomId;

        /// <summary>
        /// 移动代价（用于A*寻路，默认为1）
        /// </summary>
        public byte MoveCost;

        #endregion

        #region 属性访问器

        /// <summary>
        /// 是否可通行
        /// </summary>
        public bool IsWalkable => (Flags & ECellFlags.Walkable) != 0;

        /// <summary>
        /// 是否室内
        /// </summary>
        public bool IsIndoor => (Flags & ECellFlags.Indoor) != 0;

        /// <summary>
        /// 是否有屋顶
        /// </summary>
        public bool HasRoof => (Flags & ECellFlags.HasRoof) != 0;

        /// <summary>
        /// 是否可建造
        /// </summary>
        public bool IsBuildable => (Flags & ECellFlags.Buildable) != 0;

        /// <summary>
        /// 是否有地板
        /// </summary>
        public bool HasFloor => FloorType != EFloorType.None;

        /// <summary>
        /// 是否有北墙
        /// </summary>
        public bool HasWallNorth => WallNorth.HasWall;

        /// <summary>
        /// 是否有西墙
        /// </summary>
        public bool HasWallWest => WallWest.HasWall;

        /// <summary>
        /// 是否有任何墙体
        /// </summary>
        public bool HasAnyWall => HasWallNorth || HasWallWest;

        /// <summary>
        /// 是否有物体
        /// </summary>
        public bool HasObjects => ObjectIds != null && ObjectIds.Count > 0;

        #endregion

        #region 构造函数

        public CellData()
        {
            ObjectIds = new List<int>();
            RoomId = -1;
            MoveCost = 1;
            Flags = ECellFlags.Walkable | ECellFlags.Buildable;
            WallNorth = WallData.Empty;
            WallWest = WallData.Empty;
        }

        public CellData(int x, int y, int z) : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        #endregion

        #region 标记操作

        /// <summary>
        /// 设置标记
        /// </summary>
        public void SetFlag(ECellFlags flag, bool value)
        {
            if (value)
                Flags |= flag;
            else
                Flags &= ~flag;
        }

        /// <summary>
        /// 检查标记
        /// </summary>
        public bool HasFlag(ECellFlags flag)
        {
            return (Flags & flag) != 0;
        }

        /// <summary>
        /// 添加标记
        /// </summary>
        public void AddFlag(ECellFlags flag)
        {
            Flags |= flag;
        }

        /// <summary>
        /// 移除标记
        /// </summary>
        public void RemoveFlag(ECellFlags flag)
        {
            Flags &= ~flag;
        }

        #endregion

        #region 物体操作

        /// <summary>
        /// 添加物体
        /// </summary>
        public void AddObject(int objectId)
        {
            ObjectIds ??= new List<int>();
            if (!ObjectIds.Contains(objectId))
            {
                ObjectIds.Add(objectId);
            }
        }

        /// <summary>
        /// 移除物体
        /// </summary>
        public bool RemoveObject(int objectId)
        {
            return ObjectIds?.Remove(objectId) ?? false;
        }

        /// <summary>
        /// 清空物体
        /// </summary>
        public void ClearObjects()
        {
            ObjectIds?.Clear();
        }

        /// <summary>
        /// 检查是否有指定物体
        /// </summary>
        public bool ContainsObject(int objectId)
        {
            return ObjectIds?.Contains(objectId) ?? false;
        }

        #endregion

        #region 墙体操作

        /// <summary>
        /// 获取墙体数据
        /// </summary>
        public WallData GetWall(EWallDirection direction)
        {
            return direction == EWallDirection.North ? WallNorth : WallWest;
        }

        /// <summary>
        /// 设置墙体数据
        /// </summary>
        public void SetWall(EWallDirection direction, WallData wallData)
        {
            if (direction == EWallDirection.North)
                WallNorth = wallData;
            else
                WallWest = wallData;
        }

        /// <summary>
        /// 移除墙体
        /// </summary>
        public void RemoveWall(EWallDirection direction)
        {
            SetWall(direction, WallData.Empty);
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 重置格子到初始状态
        /// </summary>
        public void Reset()
        {
            GroundType = EGroundType.None;
            FloorType = EFloorType.None;
            WallNorth = WallData.Empty;
            WallWest = WallData.Empty;
            ObjectIds?.Clear();
            Flags = ECellFlags.Walkable | ECellFlags.Buildable;
            RoomId = -1;
            MoveCost = 1;
        }

        public override string ToString()
        {
            return $"Cell({X}, {Y}, {Z}) Ground={GroundType} Floor={FloorType} Room={RoomId}";
        }

        #endregion
    }
}
