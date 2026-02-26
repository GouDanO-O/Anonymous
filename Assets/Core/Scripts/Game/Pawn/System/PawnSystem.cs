using Core.Game.Map.Define;
using Core.Game.Map.Model;
using Core.Game.Navigation.System;
using Core.Game.Pawn.Data;
using Core.Game.Pawn.Define;
using Core.Game.Pawn.Model;
using Core.Game.Pawn.View;
using GDFrameworkCore;
using UnityEngine;

namespace Core.Game.Pawn.System
{
    /// <summary>
    /// Pawn主系统
    /// 管理Pawn生命周期，协调移动系统和视图
    /// </summary>
    public class PawnSystem : AbstractSystem
    {
        #region 子系统引用

        private PawnDataModel _pawnDataModel;
        private MapDataModel _mapDataModel;
        private PawnMovementSystem _movementSystem;
        private NavigationSystem _navigationSystem;

        #endregion

        #region 视图引用

        private PawnViewManager _viewManager;

        #endregion

        #region 属性

        /// <summary>
        /// Pawn总数
        /// </summary>
        public int PawnCount => _pawnDataModel.PawnCount;

        #endregion

        #region 初始化

        protected override void OnInit()
        {
            _pawnDataModel = this.GetModel<PawnDataModel>();
            _mapDataModel = this.GetModel<MapDataModel>();
            _movementSystem = this.GetSystem<PawnMovementSystem>();
            _navigationSystem = this.GetSystem<NavigationSystem>();

            // 订阅事件
            _pawnDataModel.OnPawnCreated += OnPawnCreated;
            _pawnDataModel.OnPawnDestroyed += OnPawnDestroyed;
        }

        protected override void OnDeinit()
        {
            if (_pawnDataModel != null)
            {
                _pawnDataModel.OnPawnCreated -= OnPawnCreated;
                _pawnDataModel.OnPawnDestroyed -= OnPawnDestroyed;
            }
        }

        #endregion

        #region 公共方法 - 视图

        /// <summary>
        /// 初始化Pawn视图管理器
        /// </summary>
        public void InitializeViewManager(PawnViewManager viewManager)
        {
            _viewManager = viewManager;

            // 为已存在的Pawn创建视图
            foreach (var pawn in _pawnDataModel.GetAllPawns())
            {
                _viewManager.CreatePawnView(pawn);
            }
        }

        #endregion

        #region 公共方法 - Pawn管理

        /// <summary>
        /// 创建活动Pawn（可移动）
        /// </summary>
        public PawnData CreateActivePawn(string defName, int cellX, int cellY, int floor,
            string displayName = null)
        {
            var pawn = _pawnDataModel.CreatePawn(defName, cellX, cellY, floor, false, displayName);

            // 设置Occupied标记
            SetCellOccupied(cellX, cellY, floor, true);

            return pawn;
        }

        /// <summary>
        /// 创建固定Pawn（不可移动，渲染优化）
        /// </summary>
        public PawnData CreateStaticPawn(string defName, int cellX, int cellY, int floor,
            string displayName = null)
        {
            var pawn = _pawnDataModel.CreatePawn(defName, cellX, cellY, floor, true, displayName);

            // 设置Occupied标记
            SetCellOccupied(cellX, cellY, floor, true);

            return pawn;
        }

        /// <summary>
        /// 销毁Pawn
        /// </summary>
        public void DestroyPawn(int pawnId)
        {
            var pawn = _pawnDataModel.GetPawn(pawnId);
            if (pawn == null) return;

            // 停止移动
            _movementSystem.StopMovement(pawnId);

            // 清除Occupied标记
            ClearCellOccupied(pawn.CellX, pawn.CellY, pawn.Floor, pawnId);

            _pawnDataModel.DestroyPawn(pawnId);
        }

        /// <summary>
        /// 请求Pawn移动到目标位置
        /// </summary>
        public void MovePawnTo(int pawnId, int targetX, int targetY, int targetFloor)
        {
            _movementSystem.RequestMoveTo(pawnId, targetX, targetY, targetFloor);
        }

        /// <summary>
        /// 停止Pawn移动
        /// </summary>
        public void StopPawnMovement(int pawnId)
        {
            _movementSystem.StopMovement(pawnId);
        }

        /// <summary>
        /// 获取Pawn数据
        /// </summary>
        public PawnData GetPawn(int pawnId)
        {
            return _pawnDataModel.GetPawn(pawnId);
        }

        #endregion

        #region 公共方法 - 更新

        /// <summary>
        /// 每帧更新（需外部调用）
        /// </summary>
        public void UpdatePawns(float deltaTime)
        {
            // 更新寻路系统
            _navigationSystem.UpdateNavigation();

            // 更新Pawn移动
            _movementSystem.UpdateMovement(deltaTime);
        }

        #endregion

        #region 事件处理

        private void OnPawnCreated(PawnData pawnData)
        {
            Debug.Log($"[PawnSystem] Created: {pawnData}");

            if (_viewManager != null && !pawnData.IsStatic)
            {
                _viewManager.CreatePawnView(pawnData);
            }
        }

        private void OnPawnDestroyed(int pawnId)
        {
            Debug.Log($"[PawnSystem] Destroyed: PawnId={pawnId}");

            _viewManager?.DestroyPawnView(pawnId);
        }

        #endregion

        #region 私有方法

        private void SetCellOccupied(int x, int y, int floor, bool occupied)
        {
            var mapData = _mapDataModel?.CurrentMap;
            if (mapData == null) return;

            var cell = mapData.GetCell(x, y, floor);
            cell?.SetFlag(ECellFlags.Occupied, occupied);
        }

        private void ClearCellOccupied(int x, int y, int floor, int excludePawnId)
        {
            // 只在该格没有其他Pawn时清除Occupied
            var pawnsAtCell = _pawnDataModel.GetPawnsAtCell(x, y, floor);
            bool hasOtherPawn = false;
            foreach (var p in pawnsAtCell)
            {
                if (p.PawnId != excludePawnId)
                {
                    hasOtherPawn = true;
                    break;
                }
            }

            if (!hasOtherPawn)
            {
                SetCellOccupied(x, y, floor, false);
            }
        }

        #endregion
    }
}
