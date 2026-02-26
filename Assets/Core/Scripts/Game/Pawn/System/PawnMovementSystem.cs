using System.Collections.Generic;
using Core.Game.Map.Define;
using Core.Game.Map.Model;
using Core.Game.Navigation.Data;
using Core.Game.Navigation.Define;
using Core.Game.Navigation.System;
using Core.Game.Pawn.Data;
using Core.Game.Pawn.Define;
using Core.Game.Pawn.Model;
using Core.Game.Pawn.Utility;
using GDFrameworkCore;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Game.Pawn.System
{
    /// <summary>
    /// Pawn移动系统
    /// 逻辑：格子到格子的离散移动
    /// 视觉：浮点精度的平滑插值
    /// </summary>
    public class PawnMovementSystem : AbstractSystem
    {
        #region 子系统引用

        private PawnDataModel _pawnDataModel;
        private MapDataModel _mapDataModel;
        private NavigationSystem _navigationSystem;

        #endregion

        #region 事件

        /// <summary>
        /// Pawn到达目标事件（pawnId）
        /// </summary>
        public UnityAction<int> OnPawnArrived;

        /// <summary>
        /// Pawn移动失败事件（pawnId）
        /// </summary>
        public UnityAction<int> OnPawnMoveFailed;

        /// <summary>
        /// Pawn视觉位置更新事件（pawnId, worldX, worldY）
        /// </summary>
        public UnityAction<int, float, float> OnPawnVisualMoved;

        #endregion

        #region 初始化

        protected override void OnInit()
        {
            _pawnDataModel = this.GetModel<PawnDataModel>();
            _mapDataModel = this.GetModel<MapDataModel>();
            _navigationSystem = this.GetSystem<NavigationSystem>();
        }

        protected override void OnDeinit()
        {
        }

        #endregion

        #region 公共方法 - 移动请求

        /// <summary>
        /// 请求Pawn移动到目标位置
        /// </summary>
        public void RequestMoveTo(int pawnId, int targetX, int targetY, int targetFloor)
        {
            var pawn = _pawnDataModel.GetPawn(pawnId);
            if (pawn == null || !pawn.CanMove)
                return;

            // 已在目标位置
            if (pawn.CellX == targetX && pawn.CellY == targetY && pawn.Floor == targetFloor)
                return;

            // 取消之前的寻路请求
            _navigationSystem.CancelRequests(pawnId);

            // 标记等待寻路
            pawn.PathState.IsWaitingForPath = true;
            pawn.PathState.TargetPosition = new Vector3Int(targetX, targetY, targetFloor);

            // 提交寻路请求
            _navigationSystem.RequestPath(
                pawn.CellX, pawn.CellY, pawn.Floor,
                targetX, targetY, targetFloor,
                result => OnPathResult(pawnId, result),
                pawnId);
        }

        /// <summary>
        /// 停止Pawn移动
        /// </summary>
        public void StopMovement(int pawnId)
        {
            var pawn = _pawnDataModel.GetPawn(pawnId);
            if (pawn == null) return;

            _navigationSystem.CancelRequests(pawnId);
            pawn.PathState.ClearPath();

            if (pawn.State == EPawnState.Moving)
            {
                _pawnDataModel.SetPawnState(pawnId, EPawnState.Idle);
                pawn.SnapVisualToCell();
            }
        }

        #endregion

        #region 公共方法 - 更新

        /// <summary>
        /// 每帧更新所有活动Pawn的移动（需要在PawnSystem.Update中调用）
        /// </summary>
        public void UpdateMovement(float deltaTime)
        {
            foreach (var pawn in _pawnDataModel.GetAllPawns())
            {
                if (pawn.IsStatic || !pawn.IsAlive)
                    continue;

                if (pawn.State == EPawnState.Moving && pawn.PathState.HasPath)
                {
                    UpdatePawnMovement(pawn, deltaTime);
                }
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 寻路结果回调
        /// </summary>
        private void OnPathResult(int pawnId, PathResult result)
        {
            var pawn = _pawnDataModel.GetPawn(pawnId);
            if (pawn == null) return;

            pawn.PathState.IsWaitingForPath = false;

            if (result.IsSuccess && result.PathLength > 1)
            {
                // 设置路径，从索引0开始（索引0是当前位置）
                pawn.PathState.SetPath(result.Path, pawn.PathState.TargetPosition);
                _pawnDataModel.SetPawnState(pawnId, EPawnState.Moving);
            }
            else
            {
                pawn.PathState.ClearPath();
                OnPawnMoveFailed?.Invoke(pawnId);
            }
        }

        /// <summary>
        /// 更新单个Pawn的移动
        /// </summary>
        private void UpdatePawnMovement(PawnData pawn, float deltaTime)
        {
            var pathState = pawn.PathState;

            // 路径已走完
            if (pathState.IsPathComplete)
            {
                CompletePath(pawn);
                return;
            }

            // 获取当前位置和下一个目标
            var nextWaypoint = pathState.NextWaypoint;
            int nextX = nextWaypoint.x;
            int nextY = nextWaypoint.y;
            int nextFloor = nextWaypoint.z;

            // 计算移动速度（受地形影响）
            float effectiveSpeed = CalculateEffectiveSpeed(pawn, nextX, nextY, nextFloor);

            // 更新进度
            pathState.MoveProgress += effectiveSpeed * deltaTime;

            // 更新视觉位置（线性插值）
            var currentWaypoint = pathState.CurrentWaypoint;
            float visualX = Mathf.Lerp(currentWaypoint.x, nextX, pathState.MoveProgress);
            float visualY = Mathf.Lerp(currentWaypoint.y, nextY, pathState.MoveProgress);
            pawn.SetVisualPosition(visualX, visualY);

            // 更新朝向
            var newFacing = PawnUtility.CalculateFacing(
                currentWaypoint.x, currentWaypoint.y, nextX, nextY);
            _pawnDataModel.SetPawnFacing(pawn.PawnId, newFacing);

            // 通知视觉位置更新
            OnPawnVisualMoved?.Invoke(pawn.PawnId, visualX, visualY);

            // 逻辑位置切换：进度超过阈值时切换到下一格
            if (pathState.MoveProgress >= PawnDefine.LogicSwitchProgress)
            {
                // 更新逻辑位置到下一格
                UpdateLogicPosition(pawn, nextX, nextY, nextFloor);
            }

            // 到达下一个路径点
            if (pathState.MoveProgress >= 1f)
            {
                pathState.AdvanceToNext();
                pawn.SetVisualPosition(nextX, nextY);

                // 检查是否走完整条路径
                if (pathState.IsPathComplete)
                {
                    CompletePath(pawn);
                }
            }
        }

        /// <summary>
        /// 计算有效移动速度（格/秒）
        /// </summary>
        private float CalculateEffectiveSpeed(PawnData pawn, int targetX, int targetY, int targetFloor)
        {
            var targetCell = _mapDataModel.GetCell(targetX, targetY, targetFloor);
            if (targetCell == null)
                return pawn.MoveSpeed;

            // MoveCost为地形代价系数，默认1
            float terrainMultiplier = targetCell.MoveCost > 0 ? 1f / targetCell.MoveCost : 1f;

            return pawn.MoveSpeed * terrainMultiplier;
        }

        /// <summary>
        /// 更新逻辑位置（处理Occupied标记）
        /// </summary>
        private void UpdateLogicPosition(PawnData pawn, int newX, int newY, int newFloor)
        {
            if (pawn.CellX == newX && pawn.CellY == newY && pawn.Floor == newFloor)
                return;

            // 清除旧位置的Occupied标记
            var mapData = _mapDataModel.CurrentMap;
            if (mapData != null)
            {
                var oldCell = mapData.GetCell(pawn.CellX, pawn.CellY, pawn.Floor);
                if (oldCell != null)
                {
                    // 只在该格没有其他Pawn时清除Occupied
                    var pawnsAtOld = _pawnDataModel.GetPawnsAtCell(pawn.CellX, pawn.CellY, pawn.Floor);
                    if (pawnsAtOld.Count <= 1)
                    {
                        oldCell.RemoveFlag(ECellFlags.Occupied);
                    }
                }

                // 设置新位置的Occupied标记
                var newCell = mapData.GetCell(newX, newY, newFloor);
                newCell?.AddFlag(ECellFlags.Occupied);
            }

            _pawnDataModel.SetPawnCellPosition(pawn.PawnId, newX, newY, newFloor);
        }

        /// <summary>
        /// 完成路径移动
        /// </summary>
        private void CompletePath(PawnData pawn)
        {
            pawn.SnapVisualToCell();
            pawn.PathState.ClearPath();
            _pawnDataModel.SetPawnState(pawn.PawnId, EPawnState.Idle);
            OnPawnArrived?.Invoke(pawn.PawnId);
        }

        #endregion
    }
}
