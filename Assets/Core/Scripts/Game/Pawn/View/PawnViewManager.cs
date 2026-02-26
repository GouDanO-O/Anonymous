using System.Collections.Generic;
using Core.Game.Pawn.Data;
using Core.Game.Pawn.Define;
using Core.Game.Pawn.Model;
using Core.Game.Pawn.System;
using GDFrameworkCore;
using UnityEngine;

namespace Core.Game.Pawn.View
{
    /// <summary>
    /// Pawn视图管理器
    /// 管理所有PawnView的创建、销毁和更新
    /// </summary>
    public class PawnViewManager : MonoBehaviour
    {
        #region 数据

        /// <summary>
        /// PawnId -> PawnView 映射
        /// </summary>
        private readonly Dictionary<int, PawnView> _pawnViews = new Dictionary<int, PawnView>();

        /// <summary>
        /// 视图容器
        /// </summary>
        private Transform _viewContainer;

        #endregion

        #region 系统引用

        private PawnDataModel _pawnDataModel;
        private PawnMovementSystem _movementSystem;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            _viewContainer = new GameObject("PawnViewContainer").transform;
            _viewContainer.SetParent(transform);
        }

        private void Start()
        {
            _pawnDataModel = GameMain.Interface.GetModel<PawnDataModel>();
            _movementSystem = GameMain.Interface.GetSystem<PawnMovementSystem>();

            // 订阅事件
            _pawnDataModel.OnPawnFacingChanged += OnPawnFacingChanged;
            _movementSystem.OnPawnVisualMoved += OnPawnVisualMoved;
        }

        private void OnDestroy()
        {
            if (_pawnDataModel != null)
            {
                _pawnDataModel.OnPawnFacingChanged -= OnPawnFacingChanged;
            }

            if (_movementSystem != null)
            {
                _movementSystem.OnPawnVisualMoved -= OnPawnVisualMoved;
            }

            ClearAllViews();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 创建Pawn视图
        /// </summary>
        public PawnView CreatePawnView(PawnData pawnData)
        {
            if (pawnData == null || _pawnViews.ContainsKey(pawnData.PawnId))
                return null;

            // 静态Pawn不创建独立View（后续合入Chunk Mesh）
            if (pawnData.IsStatic)
                return null;

            var viewObj = new GameObject($"PawnView_{pawnData.PawnId}");
            viewObj.transform.SetParent(_viewContainer);

            var view = viewObj.AddComponent<PawnView>();
            view.Initialize(pawnData);

            _pawnViews[pawnData.PawnId] = view;

            return view;
        }

        /// <summary>
        /// 销毁Pawn视图
        /// </summary>
        public void DestroyPawnView(int pawnId)
        {
            if (!_pawnViews.TryGetValue(pawnId, out var view))
                return;

            view.Cleanup();
            Destroy(view.gameObject);
            _pawnViews.Remove(pawnId);
        }

        /// <summary>
        /// 获取PawnView
        /// </summary>
        public PawnView GetPawnView(int pawnId)
        {
            return _pawnViews.TryGetValue(pawnId, out var view) ? view : null;
        }

        /// <summary>
        /// 设置楼层可见性（切换楼层时隐藏/显示Pawn）
        /// </summary>
        public void UpdateFloorVisibility(int currentFloor)
        {
            foreach (var kvp in _pawnViews)
            {
                var pawn = _pawnDataModel.GetPawn(kvp.Key);
                if (pawn != null)
                {
                    kvp.Value.SetVisible(pawn.Floor <= currentFloor);
                }
            }
        }

        /// <summary>
        /// 清除所有视图
        /// </summary>
        public void ClearAllViews()
        {
            foreach (var kvp in _pawnViews)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.Cleanup();
                    Destroy(kvp.Value.gameObject);
                }
            }

            _pawnViews.Clear();
        }

        #endregion

        #region 事件处理

        private void OnPawnVisualMoved(int pawnId, float visualX, float visualY)
        {
            if (_pawnViews.TryGetValue(pawnId, out var view))
            {
                view.UpdateVisualPosition(visualX, visualY);
            }
        }

        private void OnPawnFacingChanged(int pawnId, EFacingDirection facing)
        {
            if (_pawnViews.TryGetValue(pawnId, out var view))
            {
                view.UpdateFacing(facing);
            }
        }

        #endregion
    }
}
