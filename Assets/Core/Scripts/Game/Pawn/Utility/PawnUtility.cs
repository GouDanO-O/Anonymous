using Core.Game.Pawn.Define;
using UnityEngine;

namespace Core.Game.Pawn.Utility
{
    /// <summary>
    /// Pawn工具类
    /// </summary>
    public static class PawnUtility
    {
        /// <summary>
        /// 根据移动方向计算8方向朝向
        /// </summary>
        public static EFacingDirection CalculateFacing(int fromX, int fromY, int toX, int toY)
        {
            int dx = toX - fromX;
            int dy = toY - fromY;

            // 归一化
            int dirX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
            int dirY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);

            // 映射到EFacingDirection
            if (dirX == 0 && dirY == 1) return EFacingDirection.North;
            if (dirX == 1 && dirY == 1) return EFacingDirection.NorthEast;
            if (dirX == 1 && dirY == 0) return EFacingDirection.East;
            if (dirX == 1 && dirY == -1) return EFacingDirection.SouthEast;
            if (dirX == 0 && dirY == -1) return EFacingDirection.South;
            if (dirX == -1 && dirY == -1) return EFacingDirection.SouthWest;
            if (dirX == -1 && dirY == 0) return EFacingDirection.West;
            if (dirX == -1 && dirY == 1) return EFacingDirection.NorthWest;

            return EFacingDirection.South; // 默认朝南
        }

        /// <summary>
        /// 根据浮点方向计算8方向朝向
        /// </summary>
        public static EFacingDirection CalculateFacing(float dx, float dy)
        {
            if (Mathf.Approximately(dx, 0f) && Mathf.Approximately(dy, 0f))
                return EFacingDirection.South;

            // 使用Atan2计算角度，映射到8个方向
            float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;

            // 将角度规范到 0~360
            if (angle < 0) angle += 360f;

            // 每个方向占45度，偏移22.5度对齐
            // 0=East, 逆时针递增
            if (angle < 22.5f || angle >= 337.5f) return EFacingDirection.East;
            if (angle < 67.5f) return EFacingDirection.NorthEast;
            if (angle < 112.5f) return EFacingDirection.North;
            if (angle < 157.5f) return EFacingDirection.NorthWest;
            if (angle < 202.5f) return EFacingDirection.West;
            if (angle < 247.5f) return EFacingDirection.SouthWest;
            if (angle < 292.5f) return EFacingDirection.South;
            return EFacingDirection.SouthEast;
        }

        /// <summary>
        /// 获取朝向对应的方向向量
        /// </summary>
        public static Vector2Int GetDirectionVector(EFacingDirection facing)
        {
            return facing switch
            {
                EFacingDirection.North => new Vector2Int(0, 1),
                EFacingDirection.NorthEast => new Vector2Int(1, 1),
                EFacingDirection.East => new Vector2Int(1, 0),
                EFacingDirection.SouthEast => new Vector2Int(1, -1),
                EFacingDirection.South => new Vector2Int(0, -1),
                EFacingDirection.SouthWest => new Vector2Int(-1, -1),
                EFacingDirection.West => new Vector2Int(-1, 0),
                EFacingDirection.NorthWest => new Vector2Int(-1, 1),
                _ => new Vector2Int(0, -1)
            };
        }
    }
}
