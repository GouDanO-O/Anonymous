using Core.Game.Map.Data;
using Core.Game.Map.Model;
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

        protected override void OnInit()
        {
            _mapDataModel = this.GetModel<MapDataModel>();
            _mapGenerateSystem = this.GetSystem<MapGenerateSystem>();

            // 订阅事件
            _mapDataModel.OnMapLoaded += OnMapLoaded;
            _mapDataModel.OnMapUnloaded += OnMapUnloaded;
            _mapDataModel.OnFloorChanged += OnFloorChanged;
            _mapDataModel.OnChunkChanged += OnChunkChanged;
        }

        protected override void OnDeinit()
        {
            if (_mapDataModel != null)
            {
                _mapDataModel.OnMapLoaded -= OnMapLoaded;
                _mapDataModel.OnMapUnloaded -= OnMapUnloaded;
                _mapDataModel.OnFloorChanged -= OnFloorChanged;
                _mapDataModel.OnChunkChanged -= OnChunkChanged;
            }
        }

        #region 公共方法

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

        /// <summary>
        /// 获取格子
        /// </summary>
        public CellData GetCell(int x, int y, int floor = -1)
        {
            return _mapDataModel.GetCell(x, y, floor);
        }

        /// <summary>
        /// 屏幕坐标转格子坐标
        /// </summary>
        public Vector2Int ScreenToCell(Vector3 screenPos)
        {
            return Utility.IsometricUtility.ScreenToCellInt(screenPos, CurrentFloor);
        }

        /// <summary>
        /// 格子坐标转屏幕坐标
        /// </summary>
        public Vector3 CellToScreen(int x, int y)
        {
            return Utility.IsometricUtility.CellToScreen(x, y, CurrentFloor);
        }

        #endregion

        #region 事件处理

        private void OnMapLoaded(MapData mapData)
        {
            Debug.Log($"[MapSystem] Map loaded: {mapData.MapName} ({mapData.Width}x{mapData.Height}, {mapData.FloorCount} floors)");

            if (_mapView != null)
            {
                _mapView.Initialize(mapData);
            }
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
        }

        private void OnChunkChanged(int chunkX, int chunkY, int floor)
        {
            if (_mapView != null)
            {
                _mapView.RefreshChunk(chunkX, chunkY, floor);
            }
        }

        #endregion
    }
}
