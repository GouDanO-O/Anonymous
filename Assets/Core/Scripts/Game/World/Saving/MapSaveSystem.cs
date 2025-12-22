/**
 * MapSaveSystem.cs
 * 地图存档系统
 * 
 * 负责：
 * - 收集地图数据并生成存档
 * - 从存档恢复地图状态
 * - 差异化保存（只保存修改过的 Tile）
 * - 支持 Easy Save 2 集成
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GDFramework.MapSystem.Saving
{
    /// <summary>
    /// 地图存档系统
    /// </summary>
    public class MapSaveSystem
    {
        #region 常量
        
        /// <summary>
        /// 存档版本号
        /// </summary>
        public const int SAVE_VERSION = 1;
        
        #endregion
        
        #region 单例
        
        private static MapSaveSystem _instance;
        public static MapSaveSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MapSaveSystem();
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region 字段
        
        /// <summary>
        /// 原始地图数据缓存（用于差异化保存）
        /// 键: "{mapId}_{chunkX}_{chunkY}_{tileLocalX}_{tileLocalY}"
        /// </summary>
        private Dictionary<string, TileData> _originalTileData;
        
        /// <summary>
        /// 是否启用差异化保存
        /// </summary>
        private bool _useDifferentialSave = true;
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 是否启用差异化保存
        /// </summary>
        public bool UseDifferentialSave
        {
            get => _useDifferentialSave;
            set => _useDifferentialSave = value;
        }
        
        #endregion
        
        #region 构造函数
        
        private MapSaveSystem()
        {
            _originalTileData = new Dictionary<string, TileData>();
        }
        
        #endregion
        
        #region 原始数据缓存
        
        /// <summary>
        /// 缓存地图的原始状态（用于差异化保存）
        /// 应该在地图加载完成后调用
        /// </summary>
        public void CacheOriginalMapData(Map map)
        {
            if (map == null) return;
            
            string mapId = map.MapId;
            
            for (int cy = 0; cy < map.HeightInChunks; cy++)
            {
                for (int cx = 0; cx < map.WidthInChunks; cx++)
                {
                    Chunk chunk = map.GetChunk(new ChunkCoord(cx, cy));
                    if (chunk == null) continue;
                    
                    for (int ly = 0; ly < MapConstants.CHUNK_SIZE; ly++)
                    {
                        for (int lx = 0; lx < MapConstants.CHUNK_SIZE; lx++)
                        {
                            string key = GetTileKey(mapId, cx, cy, lx, ly);
                            _originalTileData[key] = chunk.GetTile(lx, ly);
                        }
                    }
                }
            }
            
            Debug.Log($"[MapSaveSystem] 已缓存地图原始数据: {mapId}");
        }
        
        /// <summary>
        /// 清除指定地图的原始数据缓存
        /// </summary>
        public void ClearOriginalMapData(string mapId)
        {
            var keysToRemove = new List<string>();
            string prefix = $"{mapId}_";
            
            foreach (var key in _originalTileData.Keys)
            {
                if (key.StartsWith(prefix))
                {
                    keysToRemove.Add(key);
                }
            }
            
            foreach (var key in keysToRemove)
            {
                _originalTileData.Remove(key);
            }
        }
        
        /// <summary>
        /// 清除所有原始数据缓存
        /// </summary>
        public void ClearAllOriginalData()
        {
            _originalTileData.Clear();
        }
        
        /// <summary>
        /// 生成 Tile 缓存键
        /// </summary>
        private string GetTileKey(string mapId, int cx, int cy, int lx, int ly)
        {
            return $"{mapId}_{cx}_{cy}_{lx}_{ly}";
        }
        
        /// <summary>
        /// 检查 Tile 是否被修改
        /// </summary>
        private bool IsTileModified(string mapId, int cx, int cy, int lx, int ly, TileData currentData)
        {
            string key = GetTileKey(mapId, cx, cy, lx, ly);
            
            if (!_originalTileData.TryGetValue(key, out var originalData))
            {
                // 没有原始数据，认为是新建的，需要保存
                return true;
            }
            
            return !currentData.Equals(originalData);
        }
        
        #endregion
        
        #region 保存
        
        /// <summary>
        /// 保存单个地图
        /// </summary>
        public MapSaveData SaveMap(Map map)
        {
            if (map == null)
            {
                Debug.LogError("[MapSaveSystem] Map is null");
                return null;
            }
            
            var saveData = new MapSaveData
            {
                saveVersion = SAVE_VERSION,
                saveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                mapId = map.MapId,
                mapName = map.MapName,
                widthInChunks = map.WidthInChunks,
                heightInChunks = map.HeightInChunks,
                mapType = (int)map.MapType
            };
            
            // 保存 Tile 数据
            SaveTileData(map, saveData);
            
            // 保存 Entity 数据
            SaveEntityData(map, saveData);
            
            Debug.Log($"[MapSaveSystem] 地图已保存: {map.MapId}, " +
                      $"修改的Chunk: {saveData.modifiedChunks.Count}, " +
                      $"实体: {saveData.entities.Count + saveData.containers.Count + saveData.doors.Count}");
            
            return saveData;
        }
        
        /// <summary>
        /// 保存 Tile 数据（差异化）
        /// </summary>
        private void SaveTileData(Map map, MapSaveData saveData)
        {
            string mapId = map.MapId;
            
            for (int cy = 0; cy < map.HeightInChunks; cy++)
            {
                for (int cx = 0; cx < map.WidthInChunks; cx++)
                {
                    Chunk chunk = map.GetChunk(new ChunkCoord(cx, cy));
                    if (chunk == null) continue;
                    
                    ChunkSaveData chunkSave = null;
                    
                    for (int ly = 0; ly < MapConstants.CHUNK_SIZE; ly++)
                    {
                        for (int lx = 0; lx < MapConstants.CHUNK_SIZE; lx++)
                        {
                            TileData tileData = chunk.GetTile(lx, ly);
                            
                            // 差异化检查
                            bool shouldSave = !_useDifferentialSave || 
                                             IsTileModified(mapId, cx, cy, lx, ly, tileData);
                            
                            if (shouldSave && !tileData.IsEmpty)
                            {
                                // 创建 Chunk 存档（如果还没有）
                                if (chunkSave == null)
                                {
                                    chunkSave = new ChunkSaveData(cx, cy);
                                }
                                
                                // 计算全局坐标
                                int globalX = cx * MapConstants.CHUNK_SIZE + lx;
                                int globalY = cy * MapConstants.CHUNK_SIZE + ly;
                                
                                chunkSave.modifiedTiles.Add(new TileSaveData(globalX, globalY, tileData));
                            }
                        }
                    }
                    
                    // 只添加有修改的 Chunk
                    if (chunkSave != null && chunkSave.modifiedTiles.Count > 0)
                    {
                        saveData.modifiedChunks.Add(chunkSave);
                    }
                }
            }
        }
        
        /// <summary>
        /// 保存 Entity 数据
        /// </summary>
        private void SaveEntityData(Map map, MapSaveData saveData)
        {
            var entityManager = map.Entities;
            saveData.nextEntityId = entityManager.NextEntityId;
            
            var allEntities = entityManager.GetAllEntities();
            
            foreach (var entity in allEntities)
            {
                if (entity is ContainerEntity container)
                {
                    saveData.containers.Add(new ContainerSaveData(container));
                }
                else if (entity is DoorEntity door)
                {
                    saveData.doors.Add(new DoorSaveData(door));
                }
                else
                {
                    saveData.entities.Add(new EntitySaveData(entity));
                }
            }
        }
        
        #endregion
        
        #region 加载
        
        /// <summary>
        /// 从存档加载地图
        /// </summary>
        /// <param name="saveData">存档数据</param>
        /// <param name="existingMap">已存在的地图（如果有），否则创建新地图</param>
        /// <returns>加载后的地图</returns>
        public Map LoadMap(MapSaveData saveData, Map existingMap = null)
        {
            if (saveData == null)
            {
                Debug.LogError("[MapSaveSystem] SaveData is null");
                return null;
            }
            
            // 创建或使用已存在的地图
            Map map = existingMap;
            if (map == null)
            {
                map = new Map(
                    saveData.mapId,
                    saveData.mapName,
                    saveData.widthInChunks,
                    saveData.heightInChunks,
                    (MapType)saveData.mapType
                );
            }
            
            // 加载 Tile 数据
            LoadTileData(map, saveData);
            
            // 加载 Entity 数据
            LoadEntityData(map, saveData);
            
            Debug.Log($"[MapSaveSystem] 地图已加载: {saveData.mapId}");
            
            return map;
        }
        
        /// <summary>
        /// 加载 Tile 数据
        /// </summary>
        private void LoadTileData(Map map, MapSaveData saveData)
        {
            foreach (var chunkSave in saveData.modifiedChunks)
            {
                foreach (var tileSave in chunkSave.modifiedTiles)
                {
                    TileCoord coord = new TileCoord(tileSave.x, tileSave.y);
                    TileData tileData = tileSave.ToTileData();
                    map.SetTile(coord, tileData);
                }
            }
        }
        
        /// <summary>
        /// 加载 Entity 数据
        /// </summary>
        private void LoadEntityData(Map map, MapSaveData saveData)
        {
            var entityManager = map.Entities;
            
            // 清除现有实体
            entityManager.ClearAll();
            
            // 设置下一个实体 ID
            entityManager.SetNextEntityId(saveData.nextEntityId);
            
            // 加载普通实体
            foreach (var entitySave in saveData.entities)
            {
                var entity = entityManager.CreateEntityWithId(
                    entitySave.entityId,
                    entitySave.configId,
                    (EntityType)entitySave.entityType,
                    new TileCoord(entitySave.tileX, entitySave.tileY)
                );
                
                if (entity != null)
                {
                    entitySave.ApplyTo(entity);
                }
            }
            
            // 加载容器实体
            foreach (var containerSave in saveData.containers)
            {
                var container = entityManager.CreateContainerWithId(
                    containerSave.entityId,
                    containerSave.configId,
                    new TileCoord(containerSave.tileX, containerSave.tileY),
                    containerSave.slots.Count > 0 ? containerSave.slots.Count + 10 : 20
                );
                
                if (container != null)
                {
                    containerSave.ApplyTo(container);
                }
            }
            
            // 加载门实体
            foreach (var doorSave in saveData.doors)
            {
                var door = entityManager.CreateDoorWithId(
                    doorSave.entityId,
                    doorSave.configId,
                    new TileCoord(doorSave.tileX, doorSave.tileY),
                    (DoorType)doorSave.doorType
                );
                
                if (door != null)
                {
                    doorSave.ApplyTo(door);
                }
            }
        }
        
        #endregion
        
        #region Easy Save 2 集成
        
        /// <summary>
        /// 使用 Easy Save 2 保存地图
        /// </summary>
        /// <param name="map">要保存的地图</param>
        /// <param name="filePath">文件路径（相对于 persistentDataPath）</param>
        public void SaveMapToFile(Map map, string filePath)
        {
            var saveData = SaveMap(map);
            
            if (saveData == null) return;
            
            // 使用 ES2 保存
            // ES2.Save(saveData, filePath);
            
            // 如果没有 Easy Save 2，使用 JSON 保存
            SaveToJson(saveData, filePath);
        }
        
        /// <summary>
        /// 使用 Easy Save 2 加载地图
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="existingMap">已存在的地图</param>
        public Map LoadMapFromFile(string filePath, Map existingMap = null)
        {
            // 使用 ES2 加载
            // if (!ES2.Exists(filePath)) return null;
            // var saveData = ES2.Load<MapSaveData>(filePath);
            
            // 如果没有 Easy Save 2，使用 JSON 加载
            var saveData = LoadFromJson(filePath);
            
            if (saveData == null) return null;
            
            return LoadMap(saveData, existingMap);
        }
        
        /// <summary>
        /// 检查存档文件是否存在
        /// </summary>
        public bool SaveFileExists(string filePath)
        {
            // ES2.Exists(filePath);
            string fullPath = System.IO.Path.Combine(Application.persistentDataPath, filePath);
            return System.IO.File.Exists(fullPath);
        }
        
        /// <summary>
        /// 删除存档文件
        /// </summary>
        public void DeleteSaveFile(string filePath)
        {
            // ES2.Delete(filePath);
            string fullPath = System.IO.Path.Combine(Application.persistentDataPath, filePath);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
        
        #endregion
        
        #region JSON 备选方案
        
        /// <summary>
        /// 保存到 JSON 文件
        /// </summary>
        private void SaveToJson(MapSaveData saveData, string filePath)
        {
            try
            {
                string json = JsonUtility.ToJson(saveData, true);
                string fullPath = System.IO.Path.Combine(Application.persistentDataPath, filePath);
                
                // 确保目录存在
                string directory = System.IO.Path.GetDirectoryName(fullPath);
                if (!System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }
                
                System.IO.File.WriteAllText(fullPath, json);
                Debug.Log($"[MapSaveSystem] 存档已保存到: {fullPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MapSaveSystem] 保存失败: {e.Message}");
            }
        }
        
        /// <summary>
        /// 从 JSON 文件加载
        /// </summary>
        private MapSaveData LoadFromJson(string filePath)
        {
            try
            {
                string fullPath = System.IO.Path.Combine(Application.persistentDataPath, filePath);
                
                if (!System.IO.File.Exists(fullPath))
                {
                    Debug.LogWarning($"[MapSaveSystem] 存档文件不存在: {fullPath}");
                    return null;
                }
                
                string json = System.IO.File.ReadAllText(fullPath);
                return JsonUtility.FromJson<MapSaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[MapSaveSystem] 加载失败: {e.Message}");
                return null;
            }
        }
        
        #endregion
    }
}
