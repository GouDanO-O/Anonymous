using System;
using System.Collections.Generic;
using Core.Game.Config;
using Core.Game.Map.Define;

namespace Core.Game.Map.Data
{
    /// <summary>
    /// 单元格数据
    /// </summary>
    [Serializable]
    public class CellData
    {
        public int X;
        public int Y;
        public int Z; // 楼层

        /// <summary>
        /// 自然地形 Luban TerrainDef ID, 0 = 无
        /// </summary>
        public int TerrainDefId;

        /// <summary>
        /// 人造地板 Luban FloorDef ID, 0 = 无
        /// </summary>
        public int FloorDefId;

        /// <summary>
        /// 结构 (墙/门/窗), 独占一格, 0 = 无结构
        /// </summary>
        public int StructureDefId;

        /// <summary>
        /// 门状态 (仅当结构类型为Door时有意义)
        /// </summary>
        public EDoorState DoorState;

        /// <summary>
        /// 结构耐久
        /// </summary>
        public int StructureHealth;

        /// <summary>
        /// 本格上的物体实体ID列表
        /// </summary>
        public List<long> ObjectIds;

        /// <summary>
        /// 单元格位标志
        /// </summary>
        public ECellFlags Flags;

        /// <summary>
        /// 所属房间ID, -1 = 室外
        /// </summary>
        public int RoomId;

        /// <summary>
        /// 移动代价, 0 = 不可通行, 1 = 正常
        /// </summary>
        public int MoveCost;

        public bool IsWalkable => (Flags & ECellFlags.Walkable) != 0 && MoveCost > 0;
        public bool IsIndoor => (Flags & ECellFlags.Indoor) != 0;
        public bool HasRoof => (Flags & ECellFlags.HasRoof) != 0;
        public bool IsBuildable => (Flags & ECellFlags.Buildable) != 0;
        public bool HasTerrain => TerrainDefId != MapConst.InvalidDefId;
        public bool HasFloor => FloorDefId != MapConst.InvalidDefId;
        public bool HasStructure => StructureDefId != MapConst.InvalidDefId;

        /// <summary>
        /// 结构是否允许通行 (无结构=可通行, 门=非锁定时可通行, 墙/窗=不可通行)
        /// </summary>
        public bool IsStructurePassable
        {
            get
            {
                if (!HasStructure) return true;
                var def = ConfigManager.GetStructureDef(StructureDefId);
                if ((EStructureType)def.StructureType == EStructureType.Door)
                    return DoorState != EDoorState.Locked;
                return !def.BlocksMovement;
            }
        }

        public CellData(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
            TerrainDefId = MapConst.InvalidDefId;
            FloorDefId = MapConst.InvalidDefId;
            StructureDefId = MapConst.InvalidDefId;
            DoorState = EDoorState.None;
            StructureHealth = 0;
            ObjectIds = null;
            Flags = ECellFlags.None;
            RoomId = MapConst.InvalidRoomId;
            MoveCost = 0;
        }

        public void SetFlag(ECellFlags flag, bool value)
        {
            if (value)
                Flags |= flag;
            else
                Flags &= ~flag;
        }

        public bool HasFlag(ECellFlags flag)
        {
            return (Flags & flag) != 0;
        }

        public void AddObject(long objectId)
        {
            ObjectIds ??= new List<long>();
            if (!ObjectIds.Contains(objectId))
                ObjectIds.Add(objectId);
        }

        public void RemoveObject(long objectId)
        {
            ObjectIds?.Remove(objectId);
        }

        public bool HasObjects => ObjectIds != null && ObjectIds.Count > 0;
    }
}
