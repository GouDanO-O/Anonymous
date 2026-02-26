using Core.Game.Map.Data;
using Core.Game.Map.Define;
using Core.Game.Map.Event;
using GDFrameworkCore;
using UnityEngine;

namespace Core.Game.Map.Model
{
    /// <summary>
    /// 地图数据模型
    /// </summary>
    public class MapDataModel : AbstractModel
    {
        public MapData CurrentMap { get; private set; }
        public int CurrentFloor { get; private set; }
        public bool IsMapLoaded => CurrentMap != null;

        /// <summary>
        /// 寻路网格缓存 [floor] -> byte[][]
        /// </summary>
        private byte[][][] _pathGridCache;
        private bool[] _pathGridDirty;

        protected override void OnInit()
        {
            CurrentFloor = 0;
        }

        public void LoadMap(MapData mapData)
        {
            CurrentMap = mapData;
            CurrentFloor = 0;

            _pathGridCache = new byte[mapData.FloorCount][][];
            _pathGridDirty = new bool[mapData.FloorCount];
            for (int i = 0; i < mapData.FloorCount; i++)
                _pathGridDirty[i] = true;

            this.SendEvent(new SMapLoadedEvent(mapData));
        }

        public void UnloadMap()
        {
            CurrentMap = null;
            CurrentFloor = 0;
            _pathGridCache = null;
            _pathGridDirty = null;
            this.SendEvent<SMapUnloadedEvent>();
        }

        public void SetCurrentFloor(int floor)
        {
            if (CurrentMap == null) return;

            int oldFloor = CurrentFloor;
            CurrentFloor = Mathf.Clamp(floor, 0, CurrentMap.FloorCount - 1);

            if (oldFloor != CurrentFloor)
                this.SendEvent(new SFloorChangedEvent(oldFloor, CurrentFloor));
        }

        public CellData GetCell(int x, int y, int floor = -1)
        {
            if (CurrentMap == null) return null;
            if (floor < 0) floor = CurrentFloor;
            return CurrentMap.GetCell(x, y, floor);
        }

        public ChunkData GetChunk(int chunkX, int chunkY, int floor = -1)
        {
            if (CurrentMap == null) return null;
            if (floor < 0) floor = CurrentFloor;
            return CurrentMap.GetChunk(chunkX, chunkY, floor);
        }

        /// <summary>
        /// 标记指定楼层的寻路网格为脏
        /// </summary>
        public void InvalidatePathGrid(int floor)
        {
            if (_pathGridDirty != null && floor >= 0 && floor < _pathGridDirty.Length)
                _pathGridDirty[floor] = true;
        }

        /// <summary>
        /// 获取指定楼层的寻路网格缓存
        /// </summary>
        public byte[][] GetPathGridCache(int floor)
        {
            if (_pathGridCache == null || floor < 0 || floor >= _pathGridCache.Length)
                return null;
            return _pathGridCache[floor];
        }

        /// <summary>
        /// 设置指定楼层的寻路网格缓存
        /// </summary>
        public void SetPathGridCache(int floor, byte[][] grid)
        {
            if (_pathGridCache == null || floor < 0 || floor >= _pathGridCache.Length) return;
            _pathGridCache[floor] = grid;
            _pathGridDirty[floor] = false;
        }

        /// <summary>
        /// 指定楼层的寻路网格是否需要重建
        /// </summary>
        public bool IsPathGridDirty(int floor)
        {
            if (_pathGridDirty == null || floor < 0 || floor >= _pathGridDirty.Length)
                return true;
            return _pathGridDirty[floor];
        }
    }
}
