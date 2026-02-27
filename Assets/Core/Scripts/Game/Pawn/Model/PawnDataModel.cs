using System.Collections.Generic;
using Core.Game.Pawn.Data;
using Core.Game.Pawn.Define;
using GDFrameworkCore;
using UnityEngine;

namespace Core.Game.Pawn.Model
{
    public class PawnDataModel : AbstractModel
    {
        private readonly Dictionary<long, PawnData> _pawns = new();
        private long _nextPawnId = 1;

        // 缓存, 避免每帧GC
        private readonly List<Vector3Int> _positionCache = new();

        protected override void OnInit()
        {
        }

        /// <summary>
        /// 添加Pawn, 返回分配的ID
        /// </summary>
        public long AddPawn(PawnData data)
        {
            long id = _nextPawnId++;
            data.PawnId = id;
            _pawns[id] = data;
            return id;
        }

        public void RemovePawn(long pawnId)
        {
            _pawns.Remove(pawnId);
        }

        public PawnData GetPawn(long pawnId)
        {
            return _pawns.GetValueOrDefault(pawnId);
        }

        public IReadOnlyDictionary<long, PawnData> GetAllPawns()
        {
            return _pawns;
        }

        /// <summary>
        /// 获取指定楼层的所有Pawn
        /// </summary>
        public List<PawnData> GetPawnsOnFloor(int floor)
        {
            var result = new List<PawnData>();
            foreach (var pawn in _pawns.Values)
            {
                if (pawn.Floor == floor)
                    result.Add(pawn);
            }
            return result;
        }

        /// <summary>
        /// 获取所有Pawn的位置 (供MapOcclusionSystem使用)
        /// </summary>
        public List<Vector3Int> GetPawnPositions()
        {
            _positionCache.Clear();
            foreach (var pawn in _pawns.Values)
            {
                _positionCache.Add(new Vector3Int(pawn.X, pawn.Y, pawn.Floor));
            }
            return _positionCache;
        }

        public int PawnCount => _pawns.Count;
    }
}
