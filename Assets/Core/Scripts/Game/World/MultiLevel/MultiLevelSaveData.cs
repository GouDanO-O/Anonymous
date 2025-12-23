/**
 * MultiLevelSaveData.cs
 * 多层地图存档数据结构
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using GDFramework.MapSystem.Saving;

namespace GDFramework.MapSystem.MultiLevel
{
    /// <summary>
    /// 多层地图存档数据
    /// </summary>
    [Serializable]
    public class MultiLevelMapSaveData
    {
        #region 元数据
        
        public int saveVersion;
        public long saveTimestamp;
        public string mapId;
        public string mapName;
        public int defaultWidthInChunks;
        public int defaultHeightInChunks;
        
        #endregion
        
        #region 楼层数据
        
        /// <summary>
        /// 所有楼层的存档数据
        /// </summary>
        public List<LevelSaveData> levels;
        
        /// <summary>
        /// 当前激活的层级
        /// </summary>
        public int activeLevel;
        
        #endregion
        
        #region 转换点数据
        
        /// <summary>
        /// 楼层转换点
        /// </summary>
        public List<TransitionSaveData> transitions;
        
        #endregion
        
        public MultiLevelMapSaveData()
        {
            saveVersion = MapSaveSystem.SAVE_VERSION;
            levels = new List<LevelSaveData>();
            transitions = new List<TransitionSaveData>();
        }
    }
    
    /// <summary>
    /// 单层存档数据
    /// </summary>
    [Serializable]
    public class LevelSaveData
    {
        public int levelIndex;
        public string levelName;
        public int levelType;
        public float ambientLight;
        public bool isOutdoor;
        
        /// <summary>
        /// 该层的地图存档数据
        /// </summary>
        public MapSaveData mapData;
        
        public LevelSaveData()
        {
            mapData = new MapSaveData();
        }
    }
    
    /// <summary>
    /// 转换点存档数据
    /// </summary>
    [Serializable]
    public struct TransitionSaveData
    {
        public int fromX, fromY, fromZ;
        public int toX, toY, toZ;
        public int transitionType;
        public bool requiresInteraction;
        public float transitionTime;
        
        public TransitionSaveData(LevelTransition transition)
        {
            fromX = transition.From.x;
            fromY = transition.From.y;
            fromZ = transition.From.z;
            toX = transition.To.x;
            toY = transition.To.y;
            toZ = transition.To.z;
            transitionType = (int)transition.Type;
            requiresInteraction = transition.RequiresInteraction;
            transitionTime = transition.TransitionTime;
        }
        
        public LevelTransition ToTransition()
        {
            return new LevelTransition(
                new LevelCoord(fromX, fromY, fromZ),
                new LevelCoord(toX, toY, toZ),
                (TransitionType)transitionType
            )
            {
                RequiresInteraction = requiresInteraction,
                TransitionTime = transitionTime
            };
        }
    }
    
    /// <summary>
    /// 多层地图存档系统
    /// </summary>
    public class MultiLevelSaveSystem
    {
        #region 单例
        
        private static MultiLevelSaveSystem _instance;
        public static MultiLevelSaveSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MultiLevelSaveSystem();
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region 保存
        
        /// <summary>
        /// 保存多层地图
        /// </summary>
        public MultiLevelMapSaveData SaveMap(MultiLevelMap map)
        {
            var saveData = new MultiLevelMapSaveData
            {
                saveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                mapId = map.MapId,
                mapName = map.MapName,
                activeLevel = map.ActiveLevel
            };
            
            // 保存每层数据
            foreach (var level in map.GetAllLevels())
            {
                var levelSave = new LevelSaveData
                {
                    levelIndex = level.LevelIndex,
                    levelName = level.LevelName,
                    levelType = (int)level.LevelType,
                    ambientLight = level.AmbientLight,
                    isOutdoor = level.IsOutdoor
                };
                
                // 使用适配器保存层数据
                var adapter = new MapLevelAdapter(level, map.MapId);
                levelSave.mapData = MapSaveSystem.Instance.SaveMap(adapter);
                
                saveData.levels.Add(levelSave);
            }
            
            // 保存转换点
            foreach (var transition in map.Transitions)
            {
                saveData.transitions.Add(new TransitionSaveData(transition));
            }
            
            Debug.Log($"[MultiLevelSaveSystem] 保存完成: {map.MapId}, 楼层数: {saveData.levels.Count}");
            return saveData;
        }
        
        /// <summary>
        /// 保存到文件
        /// </summary>
        public void SaveToFile(MultiLevelMap map, string filePath)
        {
            var saveData = SaveMap(map);
            
            try
            {
                string json = JsonUtility.ToJson(saveData, true);
                string fullPath = System.IO.Path.Combine(Application.persistentDataPath, filePath);
                
                string directory = System.IO.Path.GetDirectoryName(fullPath);
                if (!System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }
                
                System.IO.File.WriteAllText(fullPath, json);
                Debug.Log($"[MultiLevelSaveSystem] 已保存到: {fullPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MultiLevelSaveSystem] 保存失败: {e.Message}");
            }
        }
        
        #endregion
        
        #region 加载
        
        /// <summary>
        /// 加载多层地图
        /// </summary>
        public MultiLevelMap LoadMap(MultiLevelMapSaveData saveData)
        {
            if (saveData == null) return null;
            
            // 获取默认尺寸
            int width = saveData.defaultWidthInChunks;
            int height = saveData.defaultHeightInChunks;
            
            if (saveData.levels.Count > 0)
            {
                // 从第一层获取尺寸
                var firstLevel = saveData.levels[0];
                width = firstLevel.mapData.widthInChunks;
                height = firstLevel.mapData.heightInChunks;
            }
            
            var map = new MultiLevelMap(saveData.mapId, saveData.mapName, width, height);
            
            // 加载每层数据
            foreach (var levelSave in saveData.levels)
            {
                // 跳过地面层（已在构造函数中创建）
                MapLevel level;
                if (levelSave.levelIndex == 0)
                {
                    level = map.GroundLevel;
                }
                else
                {
                    level = map.CreateLevel(
                        levelSave.levelIndex,
                        levelSave.levelName,
                        (LevelType)levelSave.levelType
                    );
                }
                
                level.SetAmbientLight(levelSave.ambientLight);
                
                // 加载层的 Tile 和 Entity 数据
                LoadLevelData(level, levelSave.mapData);
            }
            
            // 加载转换点
            foreach (var transitionSave in saveData.transitions)
            {
                var transition = transitionSave.ToTransition();
                // 直接添加到列表（不重新放置实体）
                // map.Transitions 需要暴露添加方法
            }
            
            // 设置激活层
            map.SetActiveLevel(saveData.activeLevel);
            
            Debug.Log($"[MultiLevelSaveSystem] 加载完成: {saveData.mapId}");
            return map;
        }
        
        /// <summary>
        /// 加载层数据
        /// </summary>
        private void LoadLevelData(MapLevel level, MapSaveData mapData)
        {
            // 加载 Tile 数据
            foreach (var chunkSave in mapData.modifiedChunks)
            {
                foreach (var tileSave in chunkSave.modifiedTiles)
                {
                    TileCoord coord = new TileCoord(tileSave.x, tileSave.y);
                    TileData tileData = tileSave.ToTileData();
                    level.SetTile(coord, tileData);
                }
            }
            
            // 加载 Entity 数据
            var entities = level.Entities;
            entities.SetNextEntityId(mapData.nextEntityId);
            
            foreach (var entitySave in mapData.entities)
            {
                var entity = entities.CreateEntityWithId(
                    entitySave.entityId,
                    entitySave.configId,
                    (EntityType)entitySave.entityType,
                    new TileCoord(entitySave.tileX, entitySave.tileY)
                );
                entitySave.ApplyTo(entity);
            }
            
            foreach (var containerSave in mapData.containers)
            {
                var container = entities.CreateContainerWithId(
                    containerSave.entityId,
                    containerSave.configId,
                    new TileCoord(containerSave.tileX, containerSave.tileY),
                    containerSave.slots.Count > 0 ? containerSave.slots.Count + 10 : 20
                );
                containerSave.ApplyTo(container);
            }
            
            foreach (var doorSave in mapData.doors)
            {
                var door = entities.CreateDoorWithId(
                    doorSave.entityId,
                    doorSave.configId,
                    new TileCoord(doorSave.tileX, doorSave.tileY),
                    (DoorType)doorSave.doorType
                );
                doorSave.ApplyTo(door);
            }
        }
        
        /// <summary>
        /// 从文件加载
        /// </summary>
        public MultiLevelMap LoadFromFile(string filePath)
        {
            try
            {
                string fullPath = System.IO.Path.Combine(Application.persistentDataPath, filePath);
                
                if (!System.IO.File.Exists(fullPath))
                {
                    Debug.LogWarning($"[MultiLevelSaveSystem] 文件不存在: {fullPath}");
                    return null;
                }
                
                string json = System.IO.File.ReadAllText(fullPath);
                var saveData = JsonUtility.FromJson<MultiLevelMapSaveData>(json);
                
                return LoadMap(saveData);
            }
            catch (Exception e)
            {
                Debug.LogError($"[MultiLevelSaveSystem] 加载失败: {e.Message}");
                return null;
            }
        }
        
        #endregion
    }
}
