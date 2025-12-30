/*******************************************************************************
 * 文件名:    EntityGrid.cs
 * 描述:      实体空间索引，用于快速查询某位置的实体
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   EntityGrid 为每个楼层提供实体的空间索引，支持：
 *   - 按位置查询实体
 *   - 按区域查询实体
 *   - 通行性检查
 *   - 放置有效性检查
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// 实体空间索引
    /// </summary>
    public class EntityGrid
    {
        #region 字段

        /// <summary>
        /// 所属楼层
        /// </summary>
        private Floor _floor;

        /// <summary>
        /// 网格尺寸
        /// </summary>
        private int _sizeX;
        private int _sizeZ;

        /// <summary>
        /// 格子实体列表（每格可能有多个实体）
        /// </summary>
        private List<Entity>[] _grid;

        /// <summary>
        /// 所有实体集合
        /// </summary>
        private HashSet<Entity> _allEntities;

        /// <summary>
        /// 阻挡移动的实体网格（优化通行性查询）
        /// </summary>
        private Entity[] _blockingGrid;

        #endregion

        #region 属性

        /// <summary>
        /// 所属楼层
        /// </summary>
        public Floor Floor => _floor;

        /// <summary>
        /// X方向尺寸
        /// </summary>
        public int SizeX => _sizeX;

        /// <summary>
        /// Z方向尺寸
        /// </summary>
        public int SizeZ => _sizeZ;

        /// <summary>
        /// 实体总数
        /// </summary>
        public int EntityCount => _allEntities.Count;

        /// <summary>
        /// 所有实体
        /// </summary>
        public IEnumerable<Entity> AllEntities => _allEntities;

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public EntityGrid(Floor floor)
        {
            _floor = floor;
            _sizeX = floor.SizeX;
            _sizeZ = floor.SizeZ;

            int cellCount = _sizeX * _sizeZ;
            _grid = new List<Entity>[cellCount];
            _blockingGrid = new Entity[cellCount];
            _allEntities = new HashSet<Entity>();

            // 初始化格子列表
            for (int i = 0; i < cellCount; i++)
            {
                _grid[i] = new List<Entity>();
            }
        }

        #endregion

        #region 索引转换

        /// <summary>
        /// 检查坐标是否有效
        /// </summary>
        public bool InBounds(CellCoord cell)
        {
            return cell.x >= 0 && cell.x < _sizeX && 
                   cell.z >= 0 && cell.z < _sizeZ;
        }

        /// <summary>
        /// 坐标转索引
        /// </summary>
        private int ToIndex(CellCoord cell)
        {
            return cell.z * _sizeX + cell.x;
        }

        /// <summary>
        /// 坐标转索引
        /// </summary>
        private int ToIndex(int x, int z)
        {
            return z * _sizeX + x;
        }

        #endregion

        #region 注册/注销

        /// <summary>
        /// 注册实体
        /// </summary>
        public void Register(Entity entity)
        {
            if (entity == null || _allEntities.Contains(entity))
                return;

            _allEntities.Add(entity);

            // 添加到所有占据的格子
            foreach (var cell in entity.OccupiedCells())
            {
                if (!InBounds(cell))
                    continue;

                int index = ToIndex(cell);
                _grid[index].Add(entity);

                // 如果阻挡移动，更新阻挡网格
                if (entity.BlocksMovement && _blockingGrid[index] == null)
                {
                    _blockingGrid[index] = entity;
                }
            }
        }

        /// <summary>
        /// 注销实体
        /// </summary>
        public void Unregister(Entity entity)
        {
            if (entity == null || !_allEntities.Contains(entity))
                return;

            _allEntities.Remove(entity);

            // 从所有占据的格子移除
            foreach (var cell in entity.OccupiedCells())
            {
                if (!InBounds(cell))
                    continue;

                int index = ToIndex(cell);
                _grid[index].Remove(entity);

                // 更新阻挡网格
                if (_blockingGrid[index] == entity)
                {
                    _blockingGrid[index] = FindBlockingEntity(index);
                }
            }
        }

        /// <summary>
        /// 更新实体位置
        /// </summary>
        public void UpdatePosition(Entity entity, CellCoord oldPos, CellCoord newPos)
        {
            // 简单实现：先移除再添加
            // 注意：这里假设实体是单格的，多格实体需要更复杂的处理
            if (entity.IsSingleCell)
            {
                UpdateSingleCellPosition(entity, oldPos, newPos);
            }
            else
            {
                // 多格实体：重新注册
                Unregister(entity);
                Register(entity);
            }
        }

        /// <summary>
        /// 更新单格实体位置
        /// </summary>
        private void UpdateSingleCellPosition(Entity entity, CellCoord oldPos, CellCoord newPos)
        {
            // 从旧位置移除
            if (InBounds(oldPos))
            {
                int oldIndex = ToIndex(oldPos);
                _grid[oldIndex].Remove(entity);
                if (_blockingGrid[oldIndex] == entity)
                {
                    _blockingGrid[oldIndex] = FindBlockingEntity(oldIndex);
                }
            }

            // 添加到新位置
            if (InBounds(newPos))
            {
                int newIndex = ToIndex(newPos);
                _grid[newIndex].Add(entity);
                if (entity.BlocksMovement && _blockingGrid[newIndex] == null)
                {
                    _blockingGrid[newIndex] = entity;
                }
            }
        }

        /// <summary>
        /// 查找格子中的阻挡实体
        /// </summary>
        private Entity FindBlockingEntity(int index)
        {
            foreach (var e in _grid[index])
            {
                if (e.BlocksMovement)
                    return e;
            }
            return null;
        }

        #endregion

        #region 查询

        /// <summary>
        /// 获取指定位置的所有实体
        /// </summary>
        public IReadOnlyList<Entity> GetEntitiesAt(CellCoord cell)
        {
            if (!InBounds(cell))
                return Array.Empty<Entity>();

            return _grid[ToIndex(cell)];
        }

        /// <summary>
        /// 获取指定位置的所有实体
        /// </summary>
        public IReadOnlyList<Entity> GetEntitiesAt(int x, int z)
        {
            return GetEntitiesAt(new CellCoord(x, z));
        }

        /// <summary>
        /// 获取指定位置的第一个实体
        /// </summary>
        public Entity GetFirstEntityAt(CellCoord cell)
        {
            if (!InBounds(cell))
                return null;

            var list = _grid[ToIndex(cell)];
            return list.Count > 0 ? list[0] : null;
        }

        /// <summary>
        /// 获取指定位置的阻挡实体
        /// </summary>
        public Entity GetBlockingEntityAt(CellCoord cell)
        {
            if (!InBounds(cell))
                return null;

            return _blockingGrid[ToIndex(cell)];
        }

        /// <summary>
        /// 检查位置是否有实体
        /// </summary>
        public bool HasEntityAt(CellCoord cell)
        {
            if (!InBounds(cell))
                return false;

            return _grid[ToIndex(cell)].Count > 0;
        }

        /// <summary>
        /// 检查位置是否被阻挡
        /// </summary>
        public bool IsBlockedAt(CellCoord cell)
        {
            if (!InBounds(cell))
                return true;

            return _blockingGrid[ToIndex(cell)] != null;
        }

        /// <summary>
        /// 获取指定类型的实体
        /// </summary>
        public T GetEntityAt<T>(CellCoord cell) where T : Entity
        {
            if (!InBounds(cell))
                return null;

            foreach (var entity in _grid[ToIndex(cell)])
            {
                if (entity is T typed)
                    return typed;
            }
            return null;
        }

        /// <summary>
        /// 获取矩形区域内的所有实体
        /// </summary>
        public IEnumerable<Entity> GetEntitiesInRect(CellCoord min, CellCoord max)
        {
            var found = new HashSet<Entity>();

            int minX = Mathf.Max(0, min.x);
            int minZ = Mathf.Max(0, min.z);
            int maxX = Mathf.Min(_sizeX - 1, max.x);
            int maxZ = Mathf.Min(_sizeZ - 1, max.z);

            for (int z = minZ; z <= maxZ; z++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    foreach (var entity in _grid[ToIndex(x, z)])
                    {
                        if (found.Add(entity))
                        {
                            yield return entity;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取圆形区域内的所有实体
        /// </summary>
        public IEnumerable<Entity> GetEntitiesInRadius(CellCoord center, float radius)
        {
            var found = new HashSet<Entity>();
            int radiusInt = Mathf.CeilToInt(radius);
            float radiusSqr = radius * radius;

            int minX = Mathf.Max(0, center.x - radiusInt);
            int minZ = Mathf.Max(0, center.z - radiusInt);
            int maxX = Mathf.Min(_sizeX - 1, center.x + radiusInt);
            int maxZ = Mathf.Min(_sizeZ - 1, center.z + radiusInt);

            for (int z = minZ; z <= maxZ; z++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float dx = x - center.x;
                    float dz = z - center.z;
                    if (dx * dx + dz * dz > radiusSqr)
                        continue;

                    foreach (var entity in _grid[ToIndex(x, z)])
                    {
                        if (found.Add(entity))
                        {
                            yield return entity;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 查找最近的实体
        /// </summary>
        public Entity GetNearestEntity(CellCoord from, float maxRadius = float.MaxValue)
        {
            Entity nearest = null;
            float nearestDistSqr = maxRadius * maxRadius;

            foreach (var entity in _allEntities)
            {
                float distSqr = from.SqrDistance(entity.Position);
                if (distSqr < nearestDistSqr)
                {
                    nearestDistSqr = distSqr;
                    nearest = entity;
                }
            }

            return nearest;
        }

        /// <summary>
        /// 查找最近的指定类型实体
        /// </summary>
        public T GetNearestEntity<T>(CellCoord from, float maxRadius = float.MaxValue) where T : Entity
        {
            T nearest = null;
            float nearestDistSqr = maxRadius * maxRadius;

            foreach (var entity in _allEntities)
            {
                if (entity is not T typed)
                    continue;

                float distSqr = from.SqrDistance(entity.Position);
                if (distSqr < nearestDistSqr)
                {
                    nearestDistSqr = distSqr;
                    nearest = typed;
                }
            }

            return nearest;
        }

        #endregion

        #region 放置检查

        /// <summary>
        /// 检查是否可以放置实体
        /// </summary>
        public bool CanPlaceAt(EntityDef def, CellCoord position, Rotation rotation)
        {
            if (def == null)
                return false;

            foreach (var cell in def.GetOccupiedCells(position, rotation))
            {
                if (!CanPlaceCellAt(def, cell))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 检查单个格子是否可以放置
        /// </summary>
        private bool CanPlaceCellAt(EntityDef def, CellCoord cell)
        {
            // 检查边界
            if (!InBounds(cell))
                return false;

            // 检查楼层Tile层的承重和通行性
            if (_floor != null)
            {
                if (!_floor.CanBuildAt(cell, def.RequiredBearing))
                    return false;
            }

            // 检查是否已有阻挡实体
            if (def.Passability == Passability.Impassable)
            {
                if (IsBlockedAt(cell))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 获取放置失败原因
        /// </summary>
        public string GetPlaceFailReason(EntityDef def, CellCoord position, Rotation rotation)
        {
            if (def == null)
                return "无效的定义";

            foreach (var cell in def.GetOccupiedCells(position, rotation))
            {
                if (!InBounds(cell))
                    return "超出地图边界";

                if (_floor != null)
                {
                    var bearing = _floor.GetBearingCapacity(cell);
                    if (bearing < def.RequiredBearing)
                        return $"承重不足（需要{def.RequiredBearing.ToDisplayName()}，当前{bearing.ToDisplayName()}）";

                    if (!_floor.IsPassable(cell))
                        return "地形不可通行";
                }

                if (def.Passability == Passability.Impassable && IsBlockedAt(cell))
                    return "该位置已被占用";
            }

            return null;
        }

        #endregion

        #region 通行性

        /// <summary>
        /// 获取格子的实体通行性
        /// </summary>
        public Passability GetEntityPassability(CellCoord cell)
        {
            if (!InBounds(cell))
                return Passability.Impassable;

            var blocking = _blockingGrid[ToIndex(cell)];
            if (blocking != null)
                return blocking.Passability;

            // 检查所有实体，返回最严格的通行性
            Passability result = Passability.Passable;
            foreach (var entity in _grid[ToIndex(cell)])
            {
                if (entity.Passability > result)
                    result = entity.Passability;
            }

            return result;
        }

        /// <summary>
        /// 获取格子的综合通行性（Tile + Entity）
        /// </summary>
        public Passability GetCombinedPassability(CellCoord cell)
        {
            if (!InBounds(cell))
                return Passability.Impassable;

            // Tile层通行性
            Passability tilePass = _floor?.GetPassability(cell) ?? Passability.Passable;
            
            // 实体层通行性
            Passability entityPass = GetEntityPassability(cell);

            // 返回更严格的
            return (Passability)Mathf.Max((int)tilePass, (int)entityPass);
        }

        /// <summary>
        /// 获取格子的综合寻路代价
        /// </summary>
        public int GetCombinedPathCost(CellCoord cell)
        {
            if (!InBounds(cell))
                return int.MaxValue;

            int cost = _floor?.GetPathCost(cell) ?? 1;

            // 添加实体代价
            foreach (var entity in _grid[ToIndex(cell)])
            {
                cost += entity.PathCost;
            }

            return cost;
        }

        #endregion

        #region 清理

        /// <summary>
        /// 清空所有实体
        /// </summary>
        public void Clear()
        {
            foreach (var list in _grid)
            {
                list.Clear();
            }
            Array.Clear(_blockingGrid, 0, _blockingGrid.Length);
            _allEntities.Clear();
        }

        #endregion

        #region 调试

        /// <summary>
        /// 获取统计信息
        /// </summary>
        public Dictionary<string, int> GetStats()
        {
            var stats = new Dictionary<string, int>
            {
                ["TotalEntities"] = _allEntities.Count,
                ["BlockedCells"] = _blockingGrid.Count(e => e != null)
            };

            // 按类型统计
            var byCatgory = _allEntities.GroupBy(e => e.Category);
            foreach (var group in byCatgory)
            {
                stats[group.Key.ToString()] = group.Count();
            }

            return stats;
        }

        #endregion
    }
}
