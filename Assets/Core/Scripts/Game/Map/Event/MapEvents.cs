using Core.Game.Map.Data;

namespace Core.Game.Map.Event
{
    /// <summary>
    /// 地图加载完成事件
    /// </summary>
    public struct SMapLoadedEvent
    {
        public readonly MapData MapData;

        public SMapLoadedEvent(MapData mapData)
        {
            MapData = mapData;
        }
    }

    /// <summary>
    /// 地图卸载事件
    /// </summary>
    public struct SMapUnloadedEvent
    {
    }

    /// <summary>
    /// 楼层切换事件
    /// </summary>
    public struct SFloorChangedEvent
    {
        public readonly int OldFloor;
        public readonly int NewFloor;

        public SFloorChangedEvent(int oldFloor, int newFloor)
        {
            OldFloor = oldFloor;
            NewFloor = newFloor;
        }
    }

    /// <summary>
    /// 楼层可见性变更事件 (渲染层监听)
    /// </summary>
    public struct SFloorVisibilityChangedEvent
    {
        public readonly int CurrentViewFloor;

        public SFloorVisibilityChangedEvent(int currentViewFloor)
        {
            CurrentViewFloor = currentViewFloor;
        }
    }

    /// <summary>
    /// Chunk脏标记事件
    /// </summary>
    public struct SChunkDirtyEvent
    {
        public readonly int ChunkX;
        public readonly int ChunkY;
        public readonly int Floor;

        public SChunkDirtyEvent(int chunkX, int chunkY, int floor)
        {
            ChunkX = chunkX;
            ChunkY = chunkY;
            Floor = floor;
        }
    }

    /// <summary>
    /// 单元格变更事件
    /// </summary>
    public struct SCellChangedEvent
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Floor;

        public SCellChangedEvent(int x, int y, int floor)
        {
            X = x;
            Y = y;
            Floor = floor;
        }
    }
}
