using Core.Game.Map.Define;
using Core.Game.Map.Utility;
using Core.Game.Pawn.Data;
using Core.Game.Pawn.Define;
using UnityEngine;

namespace Core.Game.Pawn.View
{
    /// <summary>
    /// 单个Pawn的视觉渲染组件
    /// 使用SpriteRenderer进行占位渲染（后续替换为分层动画）
    /// </summary>
    public class PawnView : MonoBehaviour
    {
        #region 引用

        private SpriteRenderer _spriteRenderer;
        private PawnData _pawnData;

        #endregion

        #region 属性

        /// <summary>
        /// 关联的PawnId
        /// </summary>
        public int PawnId => _pawnData?.PawnId ?? PawnDefine.InvalidPawnId;

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化视图
        /// </summary>
        public void Initialize(PawnData pawnData)
        {
            _pawnData = pawnData;

            // 创建SpriteRenderer
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            // 创建占位Sprite（正方形）
            _spriteRenderer.sprite = CreatePlaceholderSprite();
            _spriteRenderer.color = GetPawnColor(pawnData.DefName);
            _spriteRenderer.sortingLayerName = "Default";

            // 设置初始位置
            UpdateVisualPosition(pawnData.VisualX, pawnData.VisualY);

            gameObject.name = $"Pawn_{pawnData.PawnId}_{pawnData.DefName}";
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 更新视觉位置（浮点格子坐标 -> 世界坐标）
        /// </summary>
        public void UpdateVisualPosition(float cellX, float cellY)
        {
            if (_pawnData == null) return;

            // 使用浮点版本的坐标转换
            Vector3 worldPos = IsometricUtility.CellToScreen(cellX + 0.5f, cellY + 0.5f, _pawnData.Floor);

            // 设置深度
            float depth = IsometricUtility.CalculateDepth(
                Mathf.FloorToInt(cellX), Mathf.FloorToInt(cellY),
                _pawnData.Floor, MapDefine.DepthOffsetPawn);

            transform.position = new Vector3(worldPos.x, worldPos.y, depth);

            // 更新sortingOrder
            _spriteRenderer.sortingOrder = IsometricUtility.CalculateSortingOrder(
                Mathf.FloorToInt(cellX), Mathf.FloorToInt(cellY),
                _pawnData.Floor, 4000); // Pawn层基础值
        }

        /// <summary>
        /// 更新朝向显示
        /// </summary>
        public void UpdateFacing(EFacingDirection facing)
        {
            // 占位实现：通过翻转SpriteRenderer来表示朝向
            // 后续替换为方向动画
            if (_spriteRenderer == null) return;

            bool flipX = facing == EFacingDirection.West ||
                         facing == EFacingDirection.NorthWest ||
                         facing == EFacingDirection.SouthWest;

            _spriteRenderer.flipX = flipX;
        }

        /// <summary>
        /// 设置可见性
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        /// <summary>
        /// 清理
        /// </summary>
        public void Cleanup()
        {
            _pawnData = null;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 创建占位Sprite
        /// </summary>
        private Sprite CreatePlaceholderSprite()
        {
            int size = 16;
            var texture = new Texture2D(size, size);
            texture.filterMode = FilterMode.Point;

            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0f), // pivot在底部中心
                100f);
        }

        /// <summary>
        /// 根据DefName返回占位颜色
        /// </summary>
        private Color GetPawnColor(string defName)
        {
            if (string.IsNullOrEmpty(defName))
                return Color.white;

            if (defName.Contains("Colonist")) return new Color(0.2f, 0.6f, 1f);
            if (defName.Contains("Raider")) return new Color(1f, 0.3f, 0.2f);
            if (defName.Contains("Trader")) return new Color(0.2f, 0.8f, 0.4f);
            if (defName.Contains("Rabbit")) return new Color(0.9f, 0.85f, 0.7f);
            if (defName.Contains("Wolf")) return new Color(0.5f, 0.5f, 0.5f);

            return Color.white;
        }

        #endregion
    }
}
