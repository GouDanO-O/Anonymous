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
        /// X 坐标
        /// </summary>
        public int X;

        /// <summary>
        /// Y 坐标
        /// </summary>
        public int Y;

        /// <summary>
        /// Z 坐标（楼层）
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
        /// 格子内的物体列表
        /// </summary>
        public List<CellObjectData> Objects;

        #endregion

        #region 标记与属性

        /// <summary>
        /// 格子标记
        /// </summary>
        public ECellFlags Flags;

        /// <summary>
        /// 所属房间 ID（-1 表示室外）
        /// </summary>
        public int RoomId;

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

        #endregion

        #region 构造函数

        public CellData()
        {
            Objects = new List<CellObjectData>();
            RoomId = -1;
            Flags = ECellFlags.Walkable | ECellFlags.Buildable;
        }

        public CellData(int x, int y, int z) : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        #endregion

        #region 方法

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
        /// 添加物体
        /// </summary>
        public void AddObject(CellObjectData obj)
        {
            Objects ??= new List<CellObjectData>();
            Objects.Add(obj);
        }

        /// <summary>
        /// 移除物体
        /// </summary>
        public bool RemoveObject(CellObjectData obj)
        {
            return Objects?.Remove(obj) ?? false;
        }

        /// <summary>
        /// 清空物体
        /// </summary>
        public void ClearObjects()
        {
            Objects?.Clear();
        }

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

        #endregion
    }

    /// <summary>
    /// 格子内物体数据
    /// </summary>
    [Serializable]
    public class CellObjectData
    {
        /// <summary>
        /// 物体类型
        /// </summary>
        public EObjectType ObjectType;

        /// <summary>
        /// 物体唯一 ID
        /// </summary>
        public int ObjectId;

        /// <summary>
        /// 旋转角度（0, 90, 180, 270）
        /// </summary>
        public byte Rotation;

        /// <summary>
        /// 耐久度
        /// </summary>
        public byte Health;

        public CellObjectData()
        {
            Health = 100;
        }

        public CellObjectData(EObjectType objectType, int objectId) : this()
        {
            ObjectType = objectType;
            ObjectId = objectId;
        }
    }
}
