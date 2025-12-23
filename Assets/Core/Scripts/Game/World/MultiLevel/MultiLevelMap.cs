/**
 * MultiLevelMap.cs
 * 多层地图系统
 * 
 * 设计理念：
 * - 每个 MapLevel 是一个独立的楼层（类似 PZ 的 z-level）
 * - Level 0 = 地面层
 * - Level > 0 = 上层（2楼、3楼...）
 * - Level < 0 = 地下层（地下室、地窖...）
 * - 楼层间通过楼梯/电梯连接
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GDFramework.MapSystem.MultiLevel
{
    /// <summary>
    /// 楼层坐标（包含 z 层级）
    /// </summary>
    [Serializable]
    public struct LevelCoord : IEquatable<LevelCoord>
    {
        public int x;
        public int y;
        public int z;  // 层级：0=地面，正=上层，负=地下
        
        public LevelCoord(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        
        public LevelCoord(TileCoord tile, int z)
        {
            this.x = tile.x;
            this.y = tile.y;
            this.z = z;
        }
        
        public TileCoord ToTileCoord() => new TileCoord(x, y);
        
        public bool Equals(LevelCoord other) => x == other.x && y == other.y && z == other.z;
        public override bool Equals(object obj) => obj is LevelCoord other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(x, y, z);
        public override string ToString() => $"({x}, {y}, L{z})";
        
        public static bool operator ==(LevelCoord a, LevelCoord b) => a.Equals(b);
        public static bool operator !=(LevelCoord a, LevelCoord b) => !a.Equals(b);
    }
    
    /// <summary>
    /// 单个楼层
    /// </summary>
    [Serializable]
    public class MapLevel
    {
        #region 字段
        
        /// <summary>
        /// 层级索引（0=地面，正=上层，负=地下）
        /// </summary>
        [SerializeField]
        private int _levelIndex;
        
        /// <summary>
        /// 楼层名称
        /// </summary>
        [SerializeField]
        private string _levelName;
        
        /// <summary>
        /// 楼层类型
        /// </summary>
        [SerializeField]
        private LevelType _levelType;
        
        /// <summary>
        /// Chunk 数据（与 Map 类似）
        /// </summary>
        private Chunk[,] _chunks;
        
        /// <summary>
        /// 宽度（Chunk 数量）
        /// </summary>
        [SerializeField]
        private int _widthInChunks;
        
        /// <summary>
        /// 高度（Chunk 数量）
        /// </summary>
        [SerializeField]
        private int _heightInChunks;
        
        /// <summary>
        /// 该层的实体管理器
        /// </summary>
        private EntityManager _entities;
        
        /// <summary>
        /// 环境光强度（地下室更暗）
        /// </summary>
        [SerializeField]
        private float _ambientLight;
        
        /// <summary>
        /// 是否为室外（影响天气、光照）
        /// </summary>
        [SerializeField]
        private bool _isOutdoor;
        
        #endregion
        
        #region 属性
        
        public int LevelIndex => _levelIndex;
        public string LevelName => _levelName;
        public LevelType LevelType => _levelType;
        public int WidthInChunks => _widthInChunks;
        public int HeightInChunks => _heightInChunks;
        public int WidthInTiles => _widthInChunks * MapConstants.CHUNK_SIZE;
        public int HeightInTiles => _heightInChunks * MapConstants.CHUNK_SIZE;
        public EntityManager Entities => _entities;
        public float AmbientLight => _ambientLight;
        public bool IsOutdoor => _isOutdoor;
        
        /// <summary>
        /// 是否为地面层
        /// </summary>
        public bool IsGroundLevel => _levelIndex == 0;
        
        /// <summary>
        /// 是否为地下层
        /// </summary>
        public bool IsUnderground => _levelIndex < 0;
        
        /// <summary>
        /// 是否为上层
        /// </summary>
        public bool IsUpperLevel => _levelIndex > 0;
        
        #endregion
        
        #region 构造函数
        
        public MapLevel(int levelIndex, string levelName, int widthInChunks, int heightInChunks,
            LevelType levelType = LevelType.Indoor)
        {
            _levelIndex = levelIndex;
            _levelName = levelName;
            _widthInChunks = widthInChunks;
            _heightInChunks = heightInChunks;
            _levelType = levelType;
            
            // 初始化 Chunk 数组
            _chunks = new Chunk[widthInChunks, heightInChunks];
            for (int y = 0; y < heightInChunks; y++)
            {
                for (int x = 0; x < widthInChunks; x++)
                {
                    _chunks[x, y] = new Chunk(new ChunkCoord(x, y),"");
                }
            }
            
            // 初始化实体管理器
            _entities = new EntityManager($"Level_{levelIndex}");
            
            // 设置默认环境
            SetupEnvironment();
        }
        
        #endregion
        
        #region 环境设置
        
        private void SetupEnvironment()
        {
            switch (_levelType)
            {
                case LevelType.Outdoor:
                    _ambientLight = 1.0f;
                    _isOutdoor = true;
                    break;
                    
                case LevelType.Indoor:
                    _ambientLight = 0.7f;
                    _isOutdoor = false;
                    break;
                    
                case LevelType.Basement:
                    _ambientLight = 0.3f;
                    _isOutdoor = false;
                    break;
                    
                case LevelType.Cave:
                    _ambientLight = 0.1f;
                    _isOutdoor = false;
                    break;
                    
                case LevelType.Rooftop:
                    _ambientLight = 1.0f;
                    _isOutdoor = true;
                    break;
            }
        }
        
        /// <summary>
        /// 设置环境光强度
        /// </summary>
        public void SetAmbientLight(float light)
        {
            _ambientLight = Mathf.Clamp01(light);
        }
        
        #endregion
        
        #region Tile 操作
        
        /// <summary>
        /// 获取 Chunk
        /// </summary>
        public Chunk GetChunk(ChunkCoord coord)
        {
            if (!IsChunkCoordValid(coord)) return null;
            return _chunks[coord.x, coord.y];
        }
        
        /// <summary>
        /// 获取 Tile
        /// </summary>
        public TileData GetTile(TileCoord coord)
        {
            if (!IsTileCoordValid(coord)) return TileData.Empty;
            
            ChunkCoord chunkCoord = coord.ToChunkCoord();
            LocalTileCoord localCoord = coord.ToLocalCoord();
            
            return _chunks[chunkCoord.x, chunkCoord.y].GetTile(localCoord.x, localCoord.y);
        }
        
        /// <summary>
        /// 设置 Tile
        /// </summary>
        public void SetTile(TileCoord coord, TileData data)
        {
            if (!IsTileCoordValid(coord)) return;
            
            ChunkCoord chunkCoord = coord.ToChunkCoord();
            LocalTileCoord localCoord = coord.ToLocalCoord();
            
            _chunks[chunkCoord.x, chunkCoord.y].SetTile(localCoord.x, localCoord.y, data);
        }
        
        /// <summary>
        /// 设置 Tile 层
        /// </summary>
        public void SetTileLayer(TileCoord coord, int layerIndex, TileLayerData layerData)
        {
            if (!IsTileCoordValid(coord)) return;
            
            ChunkCoord chunkCoord = coord.ToChunkCoord();
            LocalTileCoord localCoord = coord.ToLocalCoord();
            
            _chunks[chunkCoord.x, chunkCoord.y].SetTileLayer(
                localCoord.x, localCoord.y, layerIndex, layerData);
        }
        
        /// <summary>
        /// 填充整层
        /// </summary>
        public void FillLayer(int layerIndex, TileLayerData layerData)
        {
            for (int y = 0; y < _heightInChunks; y++)
            {
                for (int x = 0; x < _widthInChunks; x++)
                {
                    _chunks[x, y].FillLayer(layerIndex, layerData);
                }
            }
        }
        
        #endregion
        
        #region 坐标验证
        
        public bool IsTileCoordValid(TileCoord coord)
        {
            return coord.x >= 0 && coord.x < WidthInTiles &&
                   coord.y >= 0 && coord.y < HeightInTiles;
        }
        
        public bool IsChunkCoordValid(ChunkCoord coord)
        {
            return coord.x >= 0 && coord.x < _widthInChunks &&
                   coord.y >= 0 && coord.y < _heightInChunks;
        }
        
        /// <summary>
        /// 检查位置是否可行走
        /// </summary>
        public bool IsWalkable(TileCoord coord)
        {
            if (!IsTileCoordValid(coord)) return false;
            
            TileData tile = GetTile(coord);
            if (tile.IsBlocking) return false;
            
            if (_entities.HasBlockingEntityAt(coord)) return false;
            
            return true;
        }
        
        #endregion
        
        #region 更新
        
        public void Update(float deltaTime)
        {
            // 更新实体
            // TODO: 实体动画、状态更新等
        }
        
        #endregion
    }
    
    /// <summary>
    /// 楼层类型
    /// </summary>
    public enum LevelType
    {
        /// <summary>室外（地面）</summary>
        Outdoor,
        
        /// <summary>室内（普通楼层）</summary>
        Indoor,
        
        /// <summary>地下室</summary>
        Basement,
        
        /// <summary>洞穴/矿井</summary>
        Cave,
        
        /// <summary>屋顶</summary>
        Rooftop
    }
    
    /// <summary>
    /// 多层地图
    /// </summary>
    [Serializable]
    public class MultiLevelMap
    {
        #region 字段
        
        /// <summary>
        /// 地图 ID
        /// </summary>
        [SerializeField]
        private string _mapId;
        
        /// <summary>
        /// 地图名称
        /// </summary>
        [SerializeField]
        private string _mapName;
        
        /// <summary>
        /// 所有楼层（按层级索引）
        /// </summary>
        private Dictionary<int, MapLevel> _levels;
        
        /// <summary>
        /// 最低层级
        /// </summary>
        [SerializeField]
        private int _minLevel;
        
        /// <summary>
        /// 最高层级
        /// </summary>
        [SerializeField]
        private int _maxLevel;
        
        /// <summary>
        /// 默认尺寸
        /// </summary>
        [SerializeField]
        private int _defaultWidthInChunks;
        
        [SerializeField]
        private int _defaultHeightInChunks;
        
        /// <summary>
        /// 楼层转换点
        /// </summary>
        private List<LevelTransition> _transitions;
        
        /// <summary>
        /// 当前激活的层级
        /// </summary>
        [SerializeField]
        private int _activeLevel;
        
        #endregion
        
        #region 属性
        
        public string MapId => _mapId;
        public string MapName => _mapName;
        public int MinLevel => _minLevel;
        public int MaxLevel => _maxLevel;
        public int LevelCount => _levels.Count;
        public int ActiveLevel => _activeLevel;
        public IReadOnlyList<LevelTransition> Transitions => _transitions;
        
        /// <summary>
        /// 获取当前激活的楼层
        /// </summary>
        public MapLevel ActiveMapLevel => GetLevel(_activeLevel);
        
        /// <summary>
        /// 地面层
        /// </summary>
        public MapLevel GroundLevel => GetLevel(0);
        
        #endregion
        
        #region 构造函数
        
        public MultiLevelMap(string mapId, string mapName, int widthInChunks, int heightInChunks)
        {
            _mapId = mapId;
            _mapName = mapName;
            _defaultWidthInChunks = widthInChunks;
            _defaultHeightInChunks = heightInChunks;
            _levels = new Dictionary<int, MapLevel>();
            _transitions = new List<LevelTransition>();
            _minLevel = 0;
            _maxLevel = 0;
            _activeLevel = 0;
            
            // 创建地面层
            CreateLevel(0, "地面", LevelType.Outdoor);
        }
        
        #endregion
        
        #region 楼层管理
        
        /// <summary>
        /// 创建新楼层
        /// </summary>
        public MapLevel CreateLevel(int levelIndex, string name, LevelType type = LevelType.Indoor)
        {
            if (_levels.ContainsKey(levelIndex))
            {
                Debug.LogWarning($"[MultiLevelMap] Level {levelIndex} already exists");
                return _levels[levelIndex];
            }
            
            var level = new MapLevel(levelIndex, name, _defaultWidthInChunks, _defaultHeightInChunks, type);
            _levels[levelIndex] = level;
            
            // 更新层级范围
            _minLevel = Mathf.Min(_minLevel, levelIndex);
            _maxLevel = Mathf.Max(_maxLevel, levelIndex);
            
            Debug.Log($"[MultiLevelMap] Created level {levelIndex}: {name}");
            return level;
        }
        
        /// <summary>
        /// 获取楼层
        /// </summary>
        public MapLevel GetLevel(int levelIndex)
        {
            _levels.TryGetValue(levelIndex, out var level);
            return level;
        }
        
        /// <summary>
        /// 检查楼层是否存在
        /// </summary>
        public bool HasLevel(int levelIndex)
        {
            return _levels.ContainsKey(levelIndex);
        }
        
        /// <summary>
        /// 获取所有楼层
        /// </summary>
        public IEnumerable<MapLevel> GetAllLevels()
        {
            return _levels.Values;
        }
        
        /// <summary>
        /// 设置激活的楼层
        /// </summary>
        public void SetActiveLevel(int levelIndex)
        {
            if (!HasLevel(levelIndex))
            {
                Debug.LogWarning($"[MultiLevelMap] Level {levelIndex} does not exist");
                return;
            }
            
            _activeLevel = levelIndex;
        }
        
        /// <summary>
        /// 移动到上一层
        /// </summary>
        public bool GoUp()
        {
            int nextLevel = _activeLevel + 1;
            if (HasLevel(nextLevel))
            {
                SetActiveLevel(nextLevel);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 移动到下一层
        /// </summary>
        public bool GoDown()
        {
            int nextLevel = _activeLevel - 1;
            if (HasLevel(nextLevel))
            {
                SetActiveLevel(nextLevel);
                return true;
            }
            return false;
        }
        
        #endregion
        
        #region Tile 操作（带层级）
        
        /// <summary>
        /// 获取 Tile（使用 LevelCoord）
        /// </summary>
        public TileData GetTile(LevelCoord coord)
        {
            var level = GetLevel(coord.z);
            if (level == null) return TileData.Empty;
            return level.GetTile(coord.ToTileCoord());
        }
        
        /// <summary>
        /// 设置 Tile（使用 LevelCoord）
        /// </summary>
        public void SetTile(LevelCoord coord, TileData data)
        {
            var level = GetLevel(coord.z);
            level?.SetTile(coord.ToTileCoord(), data);
        }
        
        /// <summary>
        /// 获取当前层的 Tile
        /// </summary>
        public TileData GetTileOnActiveLevel(TileCoord coord)
        {
            return ActiveMapLevel?.GetTile(coord) ?? TileData.Empty;
        }
        
        /// <summary>
        /// 设置当前层的 Tile
        /// </summary>
        public void SetTileOnActiveLevel(TileCoord coord, TileData data)
        {
            ActiveMapLevel?.SetTile(coord, data);
        }
        
        #endregion
        
        #region 楼层转换
        
        /// <summary>
        /// 添加楼层转换点（楼梯）
        /// </summary>
        public LevelTransition AddTransition(LevelCoord from, LevelCoord to, 
            TransitionType type = TransitionType.Stairs)
        {
            var transition = new LevelTransition(from, to, type);
            _transitions.Add(transition);
            
            // 在两端放置楼梯实体
            PlaceTransitionEntity(from, to, type);
            
            return transition;
        }
        
        /// <summary>
        /// 添加双向楼梯
        /// </summary>
        public void AddBidirectionalStairs(TileCoord position, int lowerLevel, int upperLevel)
        {
            var lower = new LevelCoord(position, lowerLevel);
            var upper = new LevelCoord(position, upperLevel);
            
            // 上行楼梯
            AddTransition(lower, upper, TransitionType.StairsUp);
            // 下行楼梯
            AddTransition(upper, lower, TransitionType.StairsDown);
        }
        
        /// <summary>
        /// 放置楼梯实体
        /// </summary>
        private void PlaceTransitionEntity(LevelCoord from, LevelCoord to, TransitionType type)
        {
            // 在起点层放置楼梯实体
            var fromLevel = GetLevel(from.z);
            if (fromLevel != null)
            {
                // 根据类型创建不同的楼梯实体
                int configId = type switch
                {
                    TransitionType.StairsUp => MapConstants.STAIRS_UP_CONFIG_ID,
                    TransitionType.StairsDown => MapConstants.STAIRS_DOWN_CONFIG_ID,
                    TransitionType.Ladder => MapConstants.LADDER_CONFIG_ID,
                    TransitionType.Elevator => MapConstants.ELEVATOR_CONFIG_ID,
                    _ => MapConstants.STAIRS_UP_CONFIG_ID
                };
                
                var entity = fromLevel.Entities.CreateEntity(
                    configId,
                    EntityType.Transition,
                    from.ToTileCoord()
                );
                
                // 存储目标层级信息
                // TODO: 使用自定义数据存储目标信息
            }
        }
        
        /// <summary>
        /// 获取指定位置的转换点
        /// </summary>
        public LevelTransition GetTransitionAt(LevelCoord coord)
        {
            foreach (var transition in _transitions)
            {
                if (transition.From == coord)
                {
                    return transition;
                }
            }
            return null;
        }
        
        /// <summary>
        /// 执行楼层转换
        /// </summary>
        public bool TryTransition(LevelCoord from, out LevelCoord to)
        {
            var transition = GetTransitionAt(from);
            if (transition != null)
            {
                to = transition.To;
                SetActiveLevel(to.z);
                return true;
            }
            
            to = from;
            return false;
        }
        
        #endregion
        
        #region 更新
        
        public void Update(float deltaTime)
        {
            // 只更新激活的楼层
            ActiveMapLevel?.Update(deltaTime);
            
            // 或者更新所有楼层（取决于游戏需求）
            // foreach (var level in _levels.Values)
            // {
            //     level.Update(deltaTime);
            // }
        }
        
        #endregion
        
        #region 调试
        
        public override string ToString()
        {
            return $"MultiLevelMap({_mapId}, Levels:{_minLevel}~{_maxLevel}, Active:{_activeLevel})";
        }
        
        #endregion
    }
    
    /// <summary>
    /// 楼层转换点
    /// </summary>
    [Serializable]
    public class LevelTransition
    {
        /// <summary>
        /// 起点
        /// </summary>
        public LevelCoord From;
        
        /// <summary>
        /// 终点
        /// </summary>
        public LevelCoord To;
        
        /// <summary>
        /// 转换类型
        /// </summary>
        public TransitionType Type;
        
        /// <summary>
        /// 是否需要交互才能使用
        /// </summary>
        public bool RequiresInteraction;
        
        /// <summary>
        /// 转换时间（秒）
        /// </summary>
        public float TransitionTime;
        
        public LevelTransition(LevelCoord from, LevelCoord to, TransitionType type)
        {
            From = from;
            To = to;
            Type = type;
            RequiresInteraction = true;
            TransitionTime = 0.5f;
        }
        
        /// <summary>
        /// 层级差
        /// </summary>
        public int LevelDifference => To.z - From.z;
        
        /// <summary>
        /// 是否上行
        /// </summary>
        public bool IsAscending => LevelDifference > 0;
        
        /// <summary>
        /// 是否下行
        /// </summary>
        public bool IsDescending => LevelDifference < 0;
    }
    
    /// <summary>
    /// 转换类型
    /// </summary>
    public enum TransitionType
    {
        /// <summary>楼梯（通用）</summary>
        Stairs,
        
        /// <summary>上行楼梯</summary>
        StairsUp,
        
        /// <summary>下行楼梯</summary>
        StairsDown,
        
        /// <summary>梯子</summary>
        Ladder,
        
        /// <summary>电梯</summary>
        Elevator,
        
        /// <summary>传送门</summary>
        Portal,
        
        /// <summary>洞穴入口</summary>
        CaveEntrance,
        
        /// <summary>天井/天窗</summary>
        Hatch
    }
}
