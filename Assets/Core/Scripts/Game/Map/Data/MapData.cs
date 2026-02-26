using System;
using Core.Game.Map.Define;
using UnityEngine;

namespace Core.Game.Map.Data
{
    /// <summary>
    /// 地图顶层数据容器
    /// 管理3D chunk数组: [floor, chunkY, chunkX]
    /// </summary>
    [Serializable]
    public class MapData
    {
        public string MapName;
        public int Width;
        public int Height;
        public int FloorCount;
        public int Seed;

        public int ChunkCountX { get; private set; }
        public int ChunkCountY { get; private set; }

        /// <summary>
        /// 3D chunk数组: [floor, chunkY, chunkX]
        /// </summary>
        private ChunkData[,,] _chunks;

        /// <summary>
        /// 每层楼的房间数据
        /// </summary>
        private FloorRoomData[] _floorRoomData;

        public MapData(string name, int width, int height, int floorCount, int seed)
        {
            MapName = name;
            Width = width;
            Height = height;
            FloorCount = Mathf.Clamp(floorCount, 1, MapConst.MaxFloorCount);
            Seed = seed;

            ChunkCountX = Mathf.CeilToInt((float)width / MapConst.ChunkSize);
            ChunkCountY = Mathf.CeilToInt((float)height / MapConst.ChunkSize);

            InitializeChunks();
            InitializeRoomData();
        }

        private void InitializeChunks()
        {
            _chunks = new ChunkData[FloorCount, ChunkCountY, ChunkCountX];

            for (int f = 0; f < FloorCount; f++)
            {
                for (int cy = 0; cy < ChunkCountY; cy++)
                {
                    for (int cx = 0; cx < ChunkCountX; cx++)
                    {
                        var chunk = new ChunkData(cx, cy, f);
                        chunk.InitializeCells(Width, Height);
                        _chunks[f, cy, cx] = chunk;
                    }
                }
            }
        }

        private void InitializeRoomData()
        {
            _floorRoomData = new FloorRoomData[FloorCount];
            for (int f = 0; f < FloorCount; f++)
            {
                _floorRoomData[f] = new FloorRoomData();
            }
        }

        /// <summary>
        /// 获取指定chunk
        /// </summary>
        public ChunkData GetChunk(int chunkX, int chunkY, int floor)
        {
            if (!IsValidChunkPos(chunkX, chunkY, floor))
                return null;
            return _chunks[floor, chunkY, chunkX];
        }

        /// <summary>
        /// 通过世界坐标获取chunk
        /// </summary>
        public ChunkData GetChunkByWorld(int worldX, int worldY, int floor)
        {
            int cx = worldX / MapConst.ChunkSize;
            int cy = worldY / MapConst.ChunkSize;
            return GetChunk(cx, cy, floor);
        }

        /// <summary>
        /// 获取指定坐标的Cell
        /// </summary>
        public CellData GetCell(int x, int y, int floor)
        {
            var chunk = GetChunkByWorld(x, y, floor);
            return chunk?.GetCellByWorld(x, y);
        }

        /// <summary>
        /// 检查Cell坐标是否有效
        /// </summary>
        public bool IsValidCellPos(int x, int y, int floor)
        {
            return x >= 0 && x < Width &&
                   y >= 0 && y < Height &&
                   floor >= 0 && floor < FloorCount;
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
        /// 标记Cell所在的Chunk为脏
        /// </summary>
        public void MarkCellDirty(int x, int y, int floor)
        {
            var chunk = GetChunkByWorld(x, y, floor);
            chunk?.MarkDirty();
        }

        /// <summary>
        /// 获取指定楼层的房间数据管理器
        /// </summary>
        public FloorRoomData GetFloorRoomData(int floor)
        {
            if (floor < 0 || floor >= FloorCount)
                return null;
            return _floorRoomData[floor];
        }
    }
}
