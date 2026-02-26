using System.Collections.Generic;
using Core.Game.Pawn.Data;
using Core.Game.Pawn.Define;
using GDFrameworkCore;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Game.Pawn.Model
{
    /// <summary>
    /// Pawn数据模型
    /// 管理所有Pawn数据的增删查改
    /// </summary>
    public class PawnDataModel : AbstractModel
    {
        #region 数据

        /// <summary>
        /// 所有Pawn数据（PawnId -> PawnData）
        /// </summary>
        private readonly Dictionary<int, PawnData> _pawnMap = new Dictionary<int, PawnData>();

        /// <summary>
        /// 下一个可用的Pawn ID
        /// </summary>
        private int _nextPawnId = PawnDefine.PawnIdStart;

        #endregion

        #region 事件

        /// <summary>
        /// Pawn创建事件（pawnData）
        /// </summary>
        public UnityAction<PawnData> OnPawnCreated;

        /// <summary>
        /// Pawn销毁事件（pawnId）
        /// </summary>
        public UnityAction<int> OnPawnDestroyed;

        /// <summary>
        /// Pawn逻辑位置变更事件（pawnId, oldX, oldY, oldFloor）
        /// </summary>
        public UnityAction<int, int, int, int> OnPawnCellChanged;

        /// <summary>
        /// Pawn状态变更事件（pawnId, oldState, newState）
        /// </summary>
        public UnityAction<int, EPawnState, EPawnState> OnPawnStateChanged;

        /// <summary>
        /// Pawn朝向变更事件（pawnId, newFacing）
        /// </summary>
        public UnityAction<int, EFacingDirection> OnPawnFacingChanged;

        #endregion

        #region 属性

        /// <summary>
        /// Pawn总数
        /// </summary>
        public int PawnCount => _pawnMap.Count;

        #endregion

        #region 初始化

        protected override void OnInit()
        {
        }

        #endregion

        #region 创建与销毁

        /// <summary>
        /// 创建Pawn
        /// </summary>
        public PawnData CreatePawn(string defName, int cellX, int cellY, int floor,
            bool isStatic = false, string displayName = null)
        {
            int pawnId = _nextPawnId++;

            var pawnData = new PawnData(pawnId, defName, cellX, cellY, floor)
            {
                DisplayName = displayName ?? $"{defName}_{pawnId}",
                IsStatic = isStatic
            };

            _pawnMap[pawnId] = pawnData;

            OnPawnCreated?.Invoke(pawnData);

            return pawnData;
        }

        /// <summary>
        /// 销毁Pawn
        /// </summary>
        public bool DestroyPawn(int pawnId)
        {
            if (!_pawnMap.Remove(pawnId))
                return false;

            OnPawnDestroyed?.Invoke(pawnId);
            return true;
        }

        /// <summary>
        /// 销毁所有Pawn
        /// </summary>
        public void DestroyAllPawns()
        {
            var ids = new List<int>(_pawnMap.Keys);
            foreach (var id in ids)
            {
                DestroyPawn(id);
            }
        }

        #endregion

        #region 查询

        /// <summary>
        /// 获取Pawn数据
        /// </summary>
        public PawnData GetPawn(int pawnId)
        {
            return _pawnMap.TryGetValue(pawnId, out var pawn) ? pawn : null;
        }

        /// <summary>
        /// 检查Pawn是否存在
        /// </summary>
        public bool HasPawn(int pawnId)
        {
            return _pawnMap.ContainsKey(pawnId);
        }

        /// <summary>
        /// 获取所有Pawn
        /// </summary>
        public IEnumerable<PawnData> GetAllPawns()
        {
            return _pawnMap.Values;
        }

        /// <summary>
        /// 获取指定格子上的Pawn列表
        /// </summary>
        public List<PawnData> GetPawnsAtCell(int cellX, int cellY, int floor)
        {
            var result = new List<PawnData>();
            foreach (var pawn in _pawnMap.Values)
            {
                if (pawn.CellX == cellX && pawn.CellY == cellY && pawn.Floor == floor)
                    result.Add(pawn);
            }

            return result;
        }

        /// <summary>
        /// 获取所有活动Pawn（非静态）
        /// </summary>
        public List<PawnData> GetActivePawns()
        {
            var result = new List<PawnData>();
            foreach (var pawn in _pawnMap.Values)
            {
                if (!pawn.IsStatic && pawn.IsAlive)
                    result.Add(pawn);
            }

            return result;
        }

        /// <summary>
        /// 获取所有静态Pawn
        /// </summary>
        public List<PawnData> GetStaticPawns()
        {
            var result = new List<PawnData>();
            foreach (var pawn in _pawnMap.Values)
            {
                if (pawn.IsStatic)
                    result.Add(pawn);
            }

            return result;
        }

        #endregion

        #region 数据修改

        /// <summary>
        /// 更新Pawn逻辑位置
        /// </summary>
        public void SetPawnCellPosition(int pawnId, int newX, int newY, int newFloor)
        {
            var pawn = GetPawn(pawnId);
            if (pawn == null) return;

            int oldX = pawn.CellX;
            int oldY = pawn.CellY;
            int oldFloor = pawn.Floor;

            pawn.SetCellPosition(newX, newY, newFloor);

            OnPawnCellChanged?.Invoke(pawnId, oldX, oldY, oldFloor);
        }

        /// <summary>
        /// 更新Pawn状态
        /// </summary>
        public void SetPawnState(int pawnId, EPawnState newState)
        {
            var pawn = GetPawn(pawnId);
            if (pawn == null) return;

            var oldState = pawn.State;
            if (oldState == newState) return;

            pawn.State = newState;

            OnPawnStateChanged?.Invoke(pawnId, oldState, newState);
        }

        /// <summary>
        /// 更新Pawn朝向
        /// </summary>
        public void SetPawnFacing(int pawnId, EFacingDirection facing)
        {
            var pawn = GetPawn(pawnId);
            if (pawn == null) return;

            if (pawn.Facing == facing) return;

            pawn.Facing = facing;

            OnPawnFacingChanged?.Invoke(pawnId, facing);
        }

        #endregion
    }
}
