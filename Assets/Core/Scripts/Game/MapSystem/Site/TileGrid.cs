/*******************************************************************************
 * 文件名:    TileGrid.cs
 * 描述:      Tile数据网格类，存储单层单类型的Tile数据
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   TileGrid 存储一个楼层的一种Tile类型数据（如地形、地板、墙壁等）。
 *   使用一维数组存储DefId，通过DefDatabase查询对应的Def。
 *   
 *   数据布局：
 *   - 使用 string[] 存储 DefId（null 表示空）
 *   - 索引计算：index = z * sizeX + x
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// Tile数据网格
    /// </summary>
    public class TileGrid
    {
        #region 字段

        /// <summary>
        /// 所属楼层
        /// </summary>
        private Floor _parentFloor;

        /// <summary>
        /// Tile层级
        /// </summary>
        private TileLayer _layer;

        /// <summary>
        /// X方向尺寸
        /// </summary>
        private int _sizeX;

        /// <summary>
        /// Z方向尺寸
        /// </summary>
        private int _sizeZ;

        /// <summary>
        /// Tile数据数组（存储DefId）
        /// </summary>
        private string[] _tileData;

        /// <summary>
        /// 缓存的Def引用（懒加载）
        /// </summary>
        private TileDef[] _cachedDefs;

        /// <summary>
        /// 缓存是否有效
        /// </summary>
        private bool[] _cacheValid;

        /// <summary>
        /// 变更回调
        /// </summary>
        private Action<CellCoord, string, string> _onTileChanged;

        #endregion

        #region 属性

        /// <summary>
        /// 所属楼层
        /// </summary>
        public Floor ParentFloor => _parentFloor;

        /// <summary>
        /// Tile层级
        /// </summary>
        public TileLayer Layer => _layer;

        /// <summary>
        /// X方向尺寸
        /// </summary>
        public int SizeX => _sizeX;

        /// <summary>
        /// Z方向尺寸
        /// </summary>
        public int SizeZ => _sizeZ;

        /// <summary>
        /// 格子总数
        /// </summary>
        public int CellCount => _sizeX * _sizeZ;

        /// <summary>
        /// 原始数据数组（只读访问）
        /// </summary>
        public IReadOnlyList<string> RawData => _tileData;

        /// <summary>
        /// 变更回调（参数：坐标、旧DefId、新DefId）
        /// </summary>
        public event Action<CellCoord, string, string> OnTileChanged
        {
            add => _onTileChanged += value;
            remove => _onTileChanged -= value;
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="parentFloor">所属楼层</param>
        /// <param name="layer">Tile层级</param>
        /// <param name="sizeX">X方向尺寸</param>
        /// <param name="sizeZ">Z方向尺寸</param>
        public TileGrid(Floor parentFloor, TileLayer layer, int sizeX, int sizeZ)
        {
            _parentFloor = parentFloor;
            _layer = layer;
            _sizeX = sizeX;
            _sizeZ = sizeZ;

            int cellCount = sizeX * sizeZ;
            _tileData = new string[cellCount];
            _cachedDefs = new TileDef[cellCount];
            _cacheValid = new bool[cellCount];
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            // 清除缓存
            ClearCache();
        }

        /// <summary>
        /// 清除Def缓存
        /// </summary>
        public void ClearCache()
        {
            Array.Clear(_cachedDefs, 0, _cachedDefs.Length);
            Array.Clear(_cacheValid, 0, _cacheValid.Length);
        }

        #endregion

        #region 索引转换

        /// <summary>
        /// 检查坐标是否在范围内
        /// </summary>
        public bool InBounds(CellCoord cell)
        {
            return cell.x >= 0 && cell.x < _sizeX && 
                   cell.z >= 0 && cell.z < _sizeZ;
        }

        /// <summary>
        /// 检查坐标是否在范围内
        /// </summary>
        public bool InBounds(int x, int z)
        {
            return x >= 0 && x < _sizeX && z >= 0 && z < _sizeZ;
        }

        /// <summary>
        /// 坐标转索引
        /// </summary>
        public int ToIndex(CellCoord cell)
        {
            return cell.z * _sizeX + cell.x;
        }

        /// <summary>
        /// 坐标转索引
        /// </summary>
        public int ToIndex(int x, int z)
        {
            return z * _sizeX + x;
        }

        /// <summary>
        /// 索引转坐标
        /// </summary>
        public CellCoord ToCell(int index)
        {
            return new CellCoord(index % _sizeX, index / _sizeX);
        }

        #endregion

        #region 数据访问 - DefId

        /// <summary>
        /// 获取DefId
        /// </summary>
        public string GetDefId(CellCoord cell)
        {
            if (!InBounds(cell))
                return null;
            return _tileData[ToIndex(cell)];
        }

        /// <summary>
        /// 获取DefId
        /// </summary>
        public string GetDefId(int x, int z)
        {
            if (!InBounds(x, z))
                return null;
            return _tileData[ToIndex(x, z)];
        }

        /// <summary>
        /// 获取DefId（通过索引）
        /// </summary>
        public string GetDefIdByIndex(int index)
        {
            if (index < 0 || index >= _tileData.Length)
                return null;
            return _tileData[index];
        }

        /// <summary>
        /// 设置Tile
        /// </summary>
        public void SetTile(CellCoord cell, string defId)
        {
            if (!InBounds(cell))
                return;

            int index = ToIndex(cell);
            string oldDefId = _tileData[index];

            if (oldDefId == defId)
                return;

            _tileData[index] = defId;
            
            // 使缓存失效
            _cacheValid[index] = false;
            _cachedDefs[index] = null;

            // 触发回调
            _onTileChanged?.Invoke(cell, oldDefId, defId);

            // 标记脏
            _parentFloor?.MarkDirty(cell);
        }

        /// <summary>
        /// 设置Tile
        /// </summary>
        public void SetTile(int x, int z, string defId)
        {
            SetTile(new CellCoord(x, z), defId);
        }

        /// <summary>
        /// 设置Tile（通过索引）
        /// </summary>
        public void SetTileByIndex(int index, string defId)
        {
            if (index < 0 || index >= _tileData.Length)
                return;

            string oldDefId = _tileData[index];
            if (oldDefId == defId)
                return;

            _tileData[index] = defId;
            _cacheValid[index] = false;
            _cachedDefs[index] = null;

            var cell = ToCell(index);
            _onTileChanged?.Invoke(cell, oldDefId, defId);
            _parentFloor?.MarkDirty(cell);
        }

        #endregion

        #region 数据访问 - Def

        /// <summary>
        /// 获取Def（带缓存）
        /// </summary>
        public TileDef GetDef(CellCoord cell)
        {
            if (!InBounds(cell))
                return null;

            int index = ToIndex(cell);
            
            // 检查缓存
            if (_cacheValid[index])
                return _cachedDefs[index];

            // 查询并缓存
            string defId = _tileData[index];
            TileDef def = null;
            
            if (!string.IsNullOrEmpty(defId))
            {
                def = DefDatabase.GetDef<TileDef>(defId);
            }

            _cachedDefs[index] = def;
            _cacheValid[index] = true;

            return def;
        }

        /// <summary>
        /// 获取Def（带缓存）
        /// </summary>
        public TileDef GetDef(int x, int z)
        {
            return GetDef(new CellCoord(x, z));
        }

        /// <summary>
        /// 获取指定类型的Def
        /// </summary>
        public T GetDef<T>(CellCoord cell) where T : TileDef
        {
            return GetDef(cell) as T;
        }

        /// <summary>
        /// 获取指定类型的Def
        /// </summary>
        public T GetDef<T>(int x, int z) where T : TileDef
        {
            return GetDef(x, z) as T;
        }

        /// <summary>
        /// 获取Def（通过索引，带缓存）
        /// </summary>
        public TileDef GetDefByIndex(int index)
        {
            if (index < 0 || index >= _tileData.Length)
                return null;

            if (_cacheValid[index])
                return _cachedDefs[index];

            string defId = _tileData[index];
            TileDef def = null;
            
            if (!string.IsNullOrEmpty(defId))
            {
                def = DefDatabase.GetDef<TileDef>(defId);
            }

            _cachedDefs[index] = def;
            _cacheValid[index] = true;

            return def;
        }

        #endregion

        #region 批量操作

        /// <summary>
        /// 填充整个网格
        /// </summary>
        public void Fill(string defId)
        {
            for (int i = 0; i < _tileData.Length; i++)
            {
                _tileData[i] = defId;
                _cacheValid[i] = false;
                _cachedDefs[i] = null;
            }

            _parentFloor?.MarkAllDirty();
        }

        /// <summary>
        /// 填充矩形区域
        /// </summary>
        public void FillRect(CellCoord min, CellCoord max, string defId)
        {
            int minX = Mathf.Max(0, min.x);
            int minZ = Mathf.Max(0, min.z);
            int maxX = Mathf.Min(_sizeX - 1, max.x);
            int maxZ = Mathf.Min(_sizeZ - 1, max.z);

            for (int z = minZ; z <= maxZ; z++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    int index = z * _sizeX + x;
                    _tileData[index] = defId;
                    _cacheValid[index] = false;
                    _cachedDefs[index] = null;
                }
            }

            _parentFloor?.MarkDirtyRect(min, max);
        }

        /// <summary>
        /// 清空整个网格
        /// </summary>
        public void Clear()
        {
            Fill(null);
        }

        /// <summary>
        /// 从数组加载数据
        /// </summary>
        public void LoadFromArray(string[] data)
        {
            if (data == null || data.Length != _tileData.Length)
            {
                Debug.LogError($"[TileGrid] Invalid data length: expected {_tileData.Length}, got {data?.Length ?? 0}");
                return;
            }

            Array.Copy(data, _tileData, _tileData.Length);
            ClearCache();
            _parentFloor?.MarkAllDirty();
        }

        /// <summary>
        /// 从DefId索引数组加载数据（用于保存/加载）
        /// </summary>
        /// <param name="indices">DefId索引数组</param>
        /// <param name="idMapping">索引到DefId的映射</param>
        public void LoadFromIndices(int[] indices, string[] idMapping)
        {
            if (indices == null || indices.Length != _tileData.Length)
            {
                Debug.LogError($"[TileGrid] Invalid indices length");
                return;
            }

            for (int i = 0; i < indices.Length; i++)
            {
                int idx = indices[i];
                _tileData[i] = (idx >= 0 && idx < idMapping.Length) ? idMapping[idx] : null;
            }

            ClearCache();
            _parentFloor?.MarkAllDirty();
        }

        /// <summary>
        /// 导出为数组
        /// </summary>
        public string[] ToArray()
        {
            string[] result = new string[_tileData.Length];
            Array.Copy(_tileData, result, _tileData.Length);
            return result;
        }

        #endregion

        #region 查询

        /// <summary>
        /// 查找所有指定DefId的格子
        /// </summary>
        public IEnumerable<CellCoord> FindCellsWithDefId(string defId)
        {
            for (int i = 0; i < _tileData.Length; i++)
            {
                if (_tileData[i] == defId)
                {
                    yield return ToCell(i);
                }
            }
        }

        /// <summary>
        /// 查找所有非空格子
        /// </summary>
        public IEnumerable<CellCoord> FindNonEmptyCells()
        {
            for (int i = 0; i < _tileData.Length; i++)
            {
                if (!string.IsNullOrEmpty(_tileData[i]))
                {
                    yield return ToCell(i);
                }
            }
        }

        /// <summary>
        /// 查找所有空格子
        /// </summary>
        public IEnumerable<CellCoord> FindEmptyCells()
        {
            for (int i = 0; i < _tileData.Length; i++)
            {
                if (string.IsNullOrEmpty(_tileData[i]))
                {
                    yield return ToCell(i);
                }
            }
        }

        /// <summary>
        /// 统计指定DefId的数量
        /// </summary>
        public int CountDefId(string defId)
        {
            int count = 0;
            for (int i = 0; i < _tileData.Length; i++)
            {
                if (_tileData[i] == defId)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// 统计非空格子数量
        /// </summary>
        public int CountNonEmpty()
        {
            int count = 0;
            for (int i = 0; i < _tileData.Length; i++)
            {
                if (!string.IsNullOrEmpty(_tileData[i]))
                    count++;
            }
            return count;
        }

        /// <summary>
        /// 获取所有使用的DefId
        /// </summary>
        public HashSet<string> GetUsedDefIds()
        {
            var result = new HashSet<string>();
            for (int i = 0; i < _tileData.Length; i++)
            {
                if (!string.IsNullOrEmpty(_tileData[i]))
                {
                    result.Add(_tileData[i]);
                }
            }
            return result;
        }

        #endregion

        #region 邻居查询

        /// <summary>
        /// 获取四方向相同Tile的邻居
        /// </summary>
        public int GetSameNeighborMask4(CellCoord cell)
        {
            string defId = GetDefId(cell);
            int mask = 0;

            // 北 (bit 0)
            if (GetDefId(cell.x, cell.z + 1) == defId) mask |= 1;
            // 东 (bit 1)
            if (GetDefId(cell.x + 1, cell.z) == defId) mask |= 2;
            // 南 (bit 2)
            if (GetDefId(cell.x, cell.z - 1) == defId) mask |= 4;
            // 西 (bit 3)
            if (GetDefId(cell.x - 1, cell.z) == defId) mask |= 8;

            return mask;
        }

        /// <summary>
        /// 获取八方向相同Tile的邻居
        /// </summary>
        public int GetSameNeighborMask8(CellCoord cell)
        {
            string defId = GetDefId(cell);
            int mask = 0;

            // 北 (bit 0)
            if (GetDefId(cell.x, cell.z + 1) == defId) mask |= 1;
            // 东北 (bit 1)
            if (GetDefId(cell.x + 1, cell.z + 1) == defId) mask |= 2;
            // 东 (bit 2)
            if (GetDefId(cell.x + 1, cell.z) == defId) mask |= 4;
            // 东南 (bit 3)
            if (GetDefId(cell.x + 1, cell.z - 1) == defId) mask |= 8;
            // 南 (bit 4)
            if (GetDefId(cell.x, cell.z - 1) == defId) mask |= 16;
            // 西南 (bit 5)
            if (GetDefId(cell.x - 1, cell.z - 1) == defId) mask |= 32;
            // 西 (bit 6)
            if (GetDefId(cell.x - 1, cell.z) == defId) mask |= 64;
            // 西北 (bit 7)
            if (GetDefId(cell.x - 1, cell.z + 1) == defId) mask |= 128;

            return mask;
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return $"TileGrid[{_layer}] ({_sizeX}x{_sizeZ})";
        }

        #endregion
    }
}
