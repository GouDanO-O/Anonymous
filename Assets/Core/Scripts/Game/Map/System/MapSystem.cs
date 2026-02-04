using Core.Game.Map.Data;
using Core.Game.Map.Model;
using Core.Game.Map.Utility;
using Core.Game.Map.View;
using GDFrameworkCore;
using UnityEngine;

namespace Core.Game.Map.System
{
    /// <summary>
    /// 地图核心系统
    /// 协调地图数据、生成、渲染等子系统
    /// </summary>
    public class MapSystem : AbstractSystem
    {
        #region 子系统引用

        private MapDataModel _mapDataModel;
        private MapGenerateSystem _mapGenerateSystem;
        private RoomSystem _roomSystem;
        private MapOcclusionSystem _occlusionSystem;
        private ChunkCullingSystem _cullingSystem;

        #endregion

        #region 视图引用

        private MapView _mapView;

        #endregion

        #region 属性

        /// <summary>
        /// 当前地图数据
        /// </summary>
        public MapData CurrentMap => _mapDataModel?.CurrentMap;

        /// <summary>
        /// 当前楼层
        /// </summary>
        public int CurrentFloor => _mapDataModel?.CurrentFloor ?? 0;

        /// <summary>
        /// 是否已加载地图
        /// </summary>
        public bool IsMapLoaded => _mapDataModel?.IsMapLoaded ?? false;

        #endregion

        #region 初始化

        protected override void OnInit()
        {
            _mapDataModel = this.GetModel<MapDataModel>();
            _mapGenerateSystem = this.GetSystem<MapGenerateSystem>();
            _roomSystem = this.GetSystem<RoomSystem>();
            _occlusionSystem = this.GetSystem<MapOcclusionSystem>();
            _cullingSystem = this.GetSystem<ChunkCullingSystem>();

            // 订阅事件
            _mapDataModel.OnMapLoaded += OnMapLoaded;
            _mapDataModel.OnMapUnloaded += OnMapUnloaded;
            _mapDataModel.OnFloorChanged += OnFloorChanged;
            _mapDataModel.OnChunkDirty += OnChunkDirty;
        }

        protected override void OnDeinit()
        {
            if (_mapDataModel != null)
            {
                _mapDataModel.OnMapLoaded -= OnMapLoaded;
                _mapDataModel.OnMapUnloaded -= OnMapUnloaded;
                _mapDataModel.OnFloorChanged -= OnFloorChanged;
                _mapDataModel.OnChunkDirty -= OnChunkDirty;
            }
        }

        #endregion

        #region 公共方法 - 初始化

        /// <summary>
        /// 初始化地图视图
        /// </summary>
        public void InitializeMapView(MapView mapView)
        {
            _mapView = mapView;

            if (IsMapLoaded)
            {
                _mapView.Initialize(CurrentMap);
            }
        }

        /// <summary>
        /// 设置主摄像机
        /// </summary>
        public void SetMainCamera(Camera camera)
        {
            _cullingSystem?.SetMainCamera(camera);
        }

        #endregion

        #region 公共方法 - 地图操作

        /// <summary>
        /// 生成并加载测试地图
        /// </summary>
        public void LoadTestMap()
        {
            _mapGenerateSystem.GenerateTestMap();
        }

        /// <summary>
        /// 生成并加载地图
        /// </summary>
        public void LoadMap(string mapName, int width, int height, int floorCount, int seed)
        {
            _mapGenerateSystem.GenerateAndLoadMap(mapName, width, height, floorCount, seed);
        }

        /// <summary>
        /// 卸载当前地图
        /// </summary>
        public void UnloadMap()
        {
            _mapDataModel.UnloadMap();
        }

        #endregion

        #region 公共方法 - 楼层控制

        /// <summary>
        /// 切换楼层
        /// </summary>
        public void SetFloor(int floor)
        {
            _mapDataModel.SetCurrentFloor(floor);
        }

        /// <summary>
        /// 上一层
        /// </summary>
        public void FloorUp()
        {
            _mapDataModel.FloorUp();
        }

        /// <summary>
        /// 下一层
        /// </summary>
        public void FloorDown()
        {
            _mapDataModel.FloorDown();
        }

        #endregion

        #region 公共方法 - 数据访问

        /// <summary>
        /// 获取格子
        /// </summary>
        public CellData GetCell(int x, int y, int floor = -1)
        {
            return _mapDataModel.GetCell(x, y, floor);
        }

        /// <summary>
        /// 获取Chunk
        /// </summary>
        public ChunkData GetChunk(int chunkX, int chunkY, int floor = -1)
        {
            return _mapDataModel.GetChunk(chunkX, chunkY, floor);
        }

        /// <summary>
        /// 获取房间ID
        /// </summary>
        public int GetRoomId(int x, int y, int floor = -1)
        {
            return _roomSystem.GetRoomId(x, y, floor);
        }

        /// <summary>
        /// 检查是否室内
        /// </summary>
        public bool IsCellIndoor(int x, int y, int floor = -1)
        {
            return _roomSystem.IsCellIndoor(x, y, floor);
        }

        #endregion

        #region 公共方法 - 坐标转换

        /// <summary>
        /// 屏幕坐标转格子坐标
        /// </summary>
        public Vector2Int ScreenToCell(Vector3 screenPos)
        {
            return IsometricUtility.ScreenToCellInt(screenPos, CurrentFloor);
        }

        /// <summary>
        /// 格子坐标转屏幕坐标
        /// </summary>
        public Vector3 CellToScreen(int x, int y)
        {
            return IsometricUtility.CellToScreen(x, y, CurrentFloor);
        }

        /// <summary>
        /// 格子中心坐标转屏幕坐标
        /// </summary>
        public Vector3 CellCenterToScreen(int x, int y)
        {
            return IsometricUtility.CellCenterToScreen(x, y, CurrentFloor);
        }

        #endregion

        #region 公共方法 - 更新

        /// <summary>
        /// 更新（需要每帧调用）
        /// </summary>
        public void UpdateMap(float deltaTime)
        {
            if (!IsMapLoaded)
                return;

            // 更新视野裁剪
            _cullingSystem?.UpdateVisibility();

            // 更新遮挡过渡
            _occlusionSystem?.UpdateTransitions(deltaTime);
        }

        #endregion

        #region 事件处理

        private void OnMapLoaded(MapData mapData)
        {
            Debug.Log($"[MapSystem] Map loaded: {mapData}");

            if (_mapView != null)
            {
                _mapView.Initialize(mapData);
            }

            // 强制刷新视野裁剪
            _cullingSystem?.ForceRefresh();
        }

        private void OnMapUnloaded()
        {
            Debug.Log("[MapSystem] Map unloaded");

            if (_mapView != null)
            {
                _mapView.Clear();
            }
        }

        private void OnFloorChanged(int oldFloor, int newFloor)
        {
            Debug.Log($"[MapSystem] Floor changed: {oldFloor} -> {newFloor}");

            if (_mapView != null)
            {
                _mapView.OnFloorChanged(oldFloor, newFloor);
            }

            // 强制刷新视野裁剪
            _cullingSystem?.ForceRefresh();
        }

        private void OnChunkDirty(int chunkX, int chunkY, int floor)
        {
            if (_mapView != null)
            {
                _mapView.RefreshChunk(chunkX, chunkY, floor);
            }
        }

        #endregion
    }
}
