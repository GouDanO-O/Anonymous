using System;
using System.Collections.Generic;
using Core.Game.Map.Define;
using UnityEngine;

namespace Core.Game.Map.Data
{
    /// <summary>
    /// 地图数据（管理所有楼层的Chunk）
    /// </summary>
    [Serializable]
    public class MapData
    {
        #region 基础属性

        /// <summary>
        /// 地图名称
        /// </summary>
        public string MapName;

        /// <summary>
        /// 地图宽度（格子数）
        /// </summary>
        public int Width;

        /// <summary>
        /// 地图高度（格子数）
        /// </summary>
        public int Height;

        /// <summary>
        /// 楼层数
        /// </summary>
        public int FloorCount;

        /// <summary>
        /// 随机种子
        /// </summary>
        public int Seed;

        #endregion

        #region Chunk数据

        /// <summary>
        /// Chunk数据 [floor, chunkY, chunkX]
        /// </summary>
        private ChunkData[,,] _chunks;

        /// <summary>
        /// Chunk列数
        /// </summary>
        public int ChunkCountX { get; private set; }

        /// <summary>
        /// Chunk行数
        /// </summary>
        public int ChunkCountY { get; private set; }

        #endregion

        #region 房间数据

        /// <summary>
        /// 每层楼的房间数据
        /// </summary>
        private Dictionary<int, FloorRoomData> _floorRoomData;

        #endregion

        #region 构造函数

        public MapData()
        {
            _floorRoomData = new Dictionary<int, FloorRoomData>();
        }

        public MapData(string mapName, int width, int height, int floorCount, int seed) : this()
        {
            MapName = mapName;
            Width = width;
            Height = height;
            FloorCount = Mathf.Clamp(floorCount, 1, MapDefine.MaxFloorCount);
            Seed = seed;

            InitializeChunks();
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化Chunk数组
        /// </summary>
        private void InitializeChunks()
        {
            // 计算需要多少个Chunk（向上取整）
            ChunkCountX = Mathf.CeilToInt((float)Width / MapDefine.ChunkSize);
            ChunkCountY = Mathf.CeilToInt((float)Height / MapDefine.ChunkSize);

            _chunks = new ChunkData[FloorCount, ChunkCountY, ChunkCountX];

            // 创建所有Chunk
            for (int floor = 0; floor < FloorCount; floor++)
            {
                for (int cy = 0; cy < ChunkCountY; cy++)
                {
                    for (int cx = 0; cx < ChunkCountX; cx++)
                    {
                        _chunks[floor, cy, cx] = new ChunkData(cx, cy, floor);
                    }
                }
            }
        }

        #endregion

        #region Chunk访问

        /// <summary>
        /// 获取Chunk
        /// </summary>
        public ChunkData GetChunk(int chunkX, int chunkY, int floor)
        {
            if (!IsValidChunkPos(chunkX, chunkY, floor))
                return null;

            return _chunks[floor, chunkY, chunkX];
        }

        /// <summary>
        /// 根据世界坐标获取Chunk
        /// </summary>
        public ChunkData GetChunkByWorld(int worldX, int worldY, int floor)
        {
            int chunkX = worldX / MapDefine.ChunkSize;
            int chunkY = worldY / MapDefine.ChunkSize;
            return GetChunk(chunkX, chunkY, floor);
        }

        /// <summary>
        /// 检查Chunk坐标是否有效
        /// </summary>
        public bool IsValidChunkPos(int chunkX, int chunkY, int floor)
        {
            return chunkX >= 0 && chunkX < ChunkCountX &&
                   chunkY >= 0 && chunkY < ChunkCountY &&
                   floor >= 0 && floor < FloorCount;
        }

        /// <summary>
        /// 遍历所有Chunk
        /// </summary>
        public void ForEachChunk(Action<ChunkData> action)
        {
            for (int floor = 0; floor < FloorCount; floor++)
            {
                for (int cy = 0; cy < ChunkCountY; cy++)
                {
                    for (int cx = 0; cx < ChunkCountX; cx++)
                    {
                        action(_chunks[floor, cy, cx]);
                    }
                }
            }
        }

        /// <summary>
        /// 遍历指定楼层的Chunk
        /// </summary>
        public void ForEachChunkInFloor(int floor, Action<ChunkData> action)
        {
            if (floor < 0 || floor >= FloorCount)
                return;

            for (int cy = 0; cy < ChunkCountY; cy++)
            {
                for (int cx = 0; cx < ChunkCountX; cx++)
                {
                    action(_chunks[floor, cy, cx]);
                }
            }
        }

        #endregion

        #region 格子访问

        /// <summary>
        /// 获取格子
        /// </summary>
        public CellData GetCell(int x, int y, int floor)
        {
            if (!IsValidCellPos(x, y, floor))
                return null;

            var chunk = GetChunkByWorld(x, y, floor);
            return chunk?.GetCellByWorld(x, y);
        }

        /// <summary>
        /// 检查格子坐标是否有效
        /// </summary>
        public bool IsValidCellPos(int x, int y, int floor)
        {
            return x >= 0 && x < Width &&
                   y >= 0 && y < Height &&
                   floor >= 0 && floor < FloorCount;
        }

        /// <summary>
        /// 标记格子所在Chunk为脏
        /// </summary>
        public void MarkCellDirty(int x, int y, int floor)
        {
            var chunk = GetChunkByWorld(x, y, floor);
            chunk?.MarkDirty();
        }

        /// <summary>
        /// 世界坐标转Chunk坐标
        /// </summary>
        public Vector2Int WorldToChunkPos(int worldX, int worldY)
        {
            return new Vector2Int(
                worldX / MapDefine.ChunkSize,
                worldY / MapDefine.ChunkSize
            );
        }

        #endregion

        #region 房间数据

        /// <summary>
        /// 设置楼层的房间数据
        /// </summary>
        public void SetFloorRoomData(int floor, FloorRoomData roomData)
        {
            _floorRoomData ??= new Dictionary<int, FloorRoomData>();
            _floorRoomData[floor] = roomData;
        }

        /// <summary>
        /// 获取楼层的房间数据
        /// </summary>
        public FloorRoomData GetFloorRoomData(int floor)
        {
            if (_floorRoomData == null)
                return null;

            return _floorRoomData.TryGetValue(floor, out var data) ? data : null;
        }

        /// <summary>
        /// 获取指定房间
        /// </summary>
        public RoomData GetRoom(int roomId, int floor)
        {
            return GetFloorRoomData(floor)?.GetRoom(roomId);
        }

        /// <summary>
        /// 获取格子所属的房间ID
        /// </summary>
        public int GetRoomId(int x, int y, int floor)
        {
            var cell = GetCell(x, y, floor);
            return cell?.RoomId ?? -1;
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 获取所有脏Chunk
        /// </summary>
        public List<ChunkData> GetDirtyChunks()
        {
            var dirtyChunks = new List<ChunkData>();

            ForEachChunk(chunk =>
            {
                if (chunk.IsDirty)
                {
                    dirtyChunks.Add(chunk);
                }
            });

            return dirtyChunks;
        }

        /// <summary>
        /// 获取指定楼层的所有脏Chunk
        /// </summary>
        public List<ChunkData> GetDirtyChunksInFloor(int floor)
        {
            var dirtyChunks = new List<ChunkData>();

            ForEachChunkInFloor(floor, chunk =>
            {
                if (chunk.IsDirty)
                {
                    dirtyChunks.Add(chunk);
                }
            });

            return dirtyChunks;
        }

        /// <summary>
        /// 清除所有脏标记
        /// </summary>
        public void ClearAllDirty()
        {
            ForEachChunk(chunk => chunk.ClearDirty());
        }

        public override string ToString()
        {
            return $"Map({MapName}, {Width}x{Height}, {FloorCount} floors, {ChunkCountX}x{ChunkCountY} chunks)";
        }

        #endregion
    }
}
