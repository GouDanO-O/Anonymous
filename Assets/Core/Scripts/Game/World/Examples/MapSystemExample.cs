/**
 * MapSystemExample.cs
 * 混合系统使用示例
 * 
 * 展示如何使用 Tile 系统（静态）+ Entity 系统（动态）
 */

using UnityEngine;

namespace GDFramework.MapSystem.Examples
{
    /// <summary>
    /// 地图系统使用示例
    /// </summary>
    public class MapSystemExample : MonoBehaviour
    {
        private Map _map;
        
        void Start()
        {
            Debug.Log("=== 混合地图系统示例 ===\n");
            
            // 1. 创建地图
            CreateMap();
            
            // 2. 设置静态瓦片（Tile 系统）
            SetupTiles();
            
            // 3. 放置动态实体（Entity 系统）
            SetupEntities();
            
            // 4. 综合查询示例
            QueryExamples();
            
            // 5. 交互示例
            InteractionExamples();
        }
        
        /// <summary>
        /// 示例1：创建地图
        /// </summary>
        void CreateMap()
        {
            Debug.Log("--- 1. 创建地图 ---");
            
            // 创建 4x4 Chunk 的地图（64x64 Tiles）
            _map = new Map(
                mapId: "demo_town",
                mapName: "演示小镇",
                widthInChunks: 4,
                heightInChunks: 4,
                mapType: MapType.Town
            );
            
            Debug.Log($"地图创建完成: {_map}");
            Debug.Log($"  尺寸: {_map.WidthInTiles}x{_map.HeightInTiles} Tiles");
            Debug.Log($"  Chunk 数量: {_map.TotalChunks}");
            Debug.Log("");
        }
        
        /// <summary>
        /// 示例2：设置静态瓦片
        /// Tile 系统只负责：地形、地板、墙壁、屋顶
        /// </summary>
        void SetupTiles()
        {
            Debug.Log("--- 2. 设置静态瓦片 (Tile 系统) ---");
            
            // 填充整个地图的地形层为草地
            ushort grassTileId = 1;
            _map.FillLayer(MapConstants.LAYER_GROUND, TileLayerData.Create(grassTileId));
            Debug.Log("  填充地形层: 草地");
            
            // 建造一个小房子 (10,10) 到 (20,15)
            BuildHouse(10, 10, 10, 5);
            Debug.Log("  建造房屋: (10,10) 到 (20,15)");
            
            // 建造一条道路
            ushort roadTileId = 5;
            for (int x = 0; x < _map.WidthInTiles; x++)
            {
                // 水平道路 y=30
                _map.SetTileLayer(new TileCoord(x, 30), MapConstants.LAYER_FLOOR, 
                    TileLayerData.Create(roadTileId));
            }
            Debug.Log("  建造道路: y=30");
            
            Debug.Log("");
        }
        
        /// <summary>
        /// 建造房屋辅助方法
        /// </summary>
        void BuildHouse(int startX, int startY, int width, int height)
        {
            ushort floorTileId = 10;   // 木地板
            ushort wallTileId = 20;    // 墙壁
            ushort roofTileId = 30;    // 屋顶
            ushort doorFrameId = 25;   // 门框
            
            for (int y = startY; y < startY + height; y++)
            {
                for (int x = startX; x < startX + width; x++)
                {
                    var coord = new TileCoord(x, y);
                    
                    // 判断是否在边缘（墙壁位置）
                    bool isEdge = x == startX || x == startX + width - 1 ||
                                  y == startY || y == startY + height - 1;
                    
                    // 门的位置（南墙中间）
                    bool isDoorPosition = y == startY && x == startX + width / 2;
                    
                    if (isDoorPosition)
                    {
                        // 门框位置：放置地板和门框装饰，门本身作为 Entity
                        var tile = TileData.Empty
                            .WithFloor(floorTileId)
                            .WithWallDecor(doorFrameId)
                            .WithRoof(roofTileId);
                        _map.SetTile(coord, tile);
                    }
                    else if (isEdge)
                    {
                        // 墙壁位置
                        var tile = TileData.Empty
                            .WithFloor(floorTileId)
                            .WithWall(wallTileId)
                            .WithRoof(roofTileId);
                        _map.SetTile(coord, tile);
                    }
                    else
                    {
                        // 内部：只有地板和屋顶
                        var tile = TileData.Empty
                            .WithFloor(floorTileId)
                            .WithRoof(roofTileId);
                        _map.SetTile(coord, tile);
                    }
                }
            }
        }
        
        /// <summary>
        /// 示例3：放置动态实体
        /// Entity 系统负责：家具、容器、门、可交互对象
        /// </summary>
        void SetupEntities()
        {
            Debug.Log("--- 3. 放置动态实体 (Entity 系统) ---");
            
            var entities = _map.Entities;
            
            // 在门框位置放置门（Entity）
            var doorPos = new TileCoord(15, 10);
            var door = entities.CreateDoor(
                configId: 1001,
                position: doorPos,
                doorType: DoorType.Wooden
            );
            Debug.Log($"  放置门: {door}");
            
            // 在房间内放置家具
            var tablePos = new TileCoord(13, 12);
            var table = entities.CreateEntity(
                configId: 2001,
                type: EntityType.Furniture,
                position: tablePos
            );
            table.AddFlag(EntityFlags.Blocking);
            Debug.Log($"  放置桌子: {table}");
            
            // 放置容器（冰箱）
            var fridgePos = new TileCoord(18, 13);
            var fridge = entities.CreateContainer(
                configId: 3001,
                position: fridgePos,
                capacity: 30
            );
            // 添加一些物品到冰箱
            fridge.AddItem(itemId: 100, count: 5);  // 5个食物
            fridge.AddItem(itemId: 101, count: 3);  // 3瓶水
            Debug.Log($"  放置冰箱: {fridge}");
            
            // 在室外放置一些掉落物
            var dropPos = new TileCoord(25, 25);
            var droppedItem = entities.CreateEntity(
                configId: 4001,
                type: EntityType.DroppedItem,
                position: dropPos
            );
            droppedItem.AddFlag(EntityFlags.Pickupable);
            Debug.Log($"  放置掉落物: {droppedItem}");
            
            Debug.Log($"  实体总数: {entities.EntityCount}");
            Debug.Log("");
        }
        
        /// <summary>
        /// 示例4：综合查询
        /// </summary>
        void QueryExamples()
        {
            Debug.Log("--- 4. 综合查询示例 ---");
            
            // 查询某位置是否可行走（综合检查 Tile + Entity）
            var testPositions = new TileCoord[]
            {
                new TileCoord(14, 12),  // 房间内部（应该可走）
                new TileCoord(10, 12),  // 墙壁位置（不可走 - Tile 阻挡）
                new TileCoord(13, 12),  // 桌子位置（不可走 - Entity 阻挡）
                new TileCoord(15, 10),  // 门位置（关闭时不可走）
            };
            
            foreach (var pos in testPositions)
            {
                bool walkable = _map.IsWalkable(pos);
                TileData tile = _map.GetTile(pos);
                bool hasEntity = _map.Entities.HasEntityAt(pos);
                
                Debug.Log($"  位置 {pos}: 可行走={walkable}, 有墙={tile.HasWall}, 有实体={hasEntity}");
            }
            
            // 按位置查询实体
            var doorPos = new TileCoord(15, 10);
            var entitiesAtDoor = _map.Entities.GetEntitiesAt(doorPos);
            Debug.Log($"  门位置的实体: {entitiesAtDoor.Count} 个");
            
            // 按类型查询实体
            var allDoors = _map.Entities.GetEntitiesByType<DoorEntity>();
            var allContainers = _map.Entities.GetEntitiesByType<ContainerEntity>();
            Debug.Log($"  所有门: {allDoors.Count} 个");
            Debug.Log($"  所有容器: {allContainers.Count} 个");
            
            // 查询某 Chunk 内的实体
            var chunkCoord = new ChunkCoord(0, 0);
            var entitiesInChunk = _map.Entities.GetEntitiesInChunk(chunkCoord);
            Debug.Log($"  Chunk(0,0) 内的实体: {entitiesInChunk.Count} 个");
            
            Debug.Log("");
        }
        
        /// <summary>
        /// 示例5：交互示例
        /// </summary>
        void InteractionExamples()
        {
            Debug.Log("--- 5. 交互示例 ---");
            
            // 获取门实体
            var doorPos = new TileCoord(15, 10);
            var door = _map.Entities.GetEntityAt<DoorEntity>(doorPos);
            
            if (door != null)
            {
                Debug.Log($"  门状态: 打开={door.IsOpen}, 锁定={door.IsLocked}");
                
                // 开门
                bool opened = door.TryOpen();
                Debug.Log($"  尝试开门: {(opened ? "成功" : "失败")}");
                Debug.Log($"  开门后可行走: {_map.IsWalkable(doorPos)}");
                
                // 关门
                door.Close();
                door.ForceClose(); // 立即关闭（跳过动画）
                Debug.Log($"  关门后可行走: {_map.IsWalkable(doorPos)}");
                
                // 锁门
                door.Lock();
                Debug.Log($"  锁门后状态: 锁定={door.IsLocked}");
                
                // 尝试用钥匙开门
                door.ChangeLock("house_key");
                bool unlockedWithKey = door.TryUnlock("wrong_key");
                Debug.Log($"  用错误钥匙: {(unlockedWithKey ? "解锁成功" : "解锁失败")}");
                
                unlockedWithKey = door.TryUnlock("house_key");
                Debug.Log($"  用正确钥匙: {(unlockedWithKey ? "解锁成功" : "解锁失败")}");
            }
            
            // 与容器交互
            var fridgePos = new TileCoord(18, 13);
            var fridge = _map.Entities.GetEntityAt<ContainerEntity>(fridgePos);
            
            if (fridge != null)
            {
                Debug.Log($"  冰箱容量: {fridge.UsedSlots}/{fridge.Capacity}");
                Debug.Log($"  冰箱物品数量 (ID:100): {fridge.GetItemCount(100)}");
                
                // 取出物品
                int taken = fridge.RemoveItem(itemId: 100, count: 2);
                Debug.Log($"  取出物品: {taken} 个");
                Debug.Log($"  剩余数量: {fridge.GetItemCount(100)}");
                
                // 放入物品
                int added = fridge.AddItem(itemId: 102, count: 10);
                Debug.Log($"  放入新物品: {added} 个");
            }
            
            Debug.Log("");
        }
        
        void Update()
        {
            // 更新地图（主要是 Entity 动画等）
            _map?.Update(Time.deltaTime);
        }
        
        /// <summary>
        /// 额外示例：坐标转换
        /// </summary>
        void CoordinateExamples()
        {
            Debug.Log("--- 坐标转换示例 ---");
            
            // 世界坐标 -> Tile 坐标
            Vector2 worldPos = new Vector2(15.5f, 10.3f);
            TileCoord tileCoord = MapCoordUtility.WorldToTile(worldPos);
            Debug.Log($"  世界坐标 {worldPos} -> Tile {tileCoord}");
            
            // Tile 坐标 -> Chunk 坐标
            ChunkCoord chunkCoord = tileCoord.ToChunkCoord();
            Debug.Log($"  Tile {tileCoord} -> Chunk {chunkCoord}");
            
            // Tile 坐标 -> 局部坐标
            LocalTileCoord localCoord = tileCoord.ToLocalCoord();
            Debug.Log($"  Tile {tileCoord} -> Local {localCoord}");
            
            // 反向：Chunk + Local -> Tile
            TileCoord rebuilt = MapCoordUtility.ChunkLocalToTile(chunkCoord, localCoord);
            Debug.Log($"  重建: Chunk{chunkCoord} + Local{localCoord} -> Tile{rebuilt}");
        }
    }
}
