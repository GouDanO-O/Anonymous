using System.Collections.Generic;
using Core.Game.Map.Data;
using Core.Game.Map.Event;
using Core.Game.Pawn.Data;
using Core.Game.Pawn.Define;
using Core.Game.Pawn.Event;
using Core.Game.Pawn.Model;
using Core.Game.Pawn.System;
using GDFrameworkCore;
using UnityEngine;

namespace Core.Game.Pawn.View
{
    /// <summary>
    /// Pawn视图协调器: 管理所有Pawn的渲染GameObject
    /// </summary>
    public class PawnView : MonoBehaviour, IController
    {
        private PawnSystem _pawnSystem;
        private PawnDataModel _pawnModel;

        private readonly Dictionary<long, SpriteRenderer> _renderers = new();
        private Sprite _circleSprite;

        private int _currentFloor;

        private void Start()
        {
            _pawnSystem = this.GetSystem<PawnSystem>();
            _pawnModel = this.GetModel<PawnDataModel>();

            _circleSprite = CreateCircleSprite();

            this.RegisterEvent<SPawnCreatedEvent>(OnPawnCreated);
            this.RegisterEvent<SPawnDestroyedEvent>(OnPawnDestroyed);
            this.RegisterEvent<SFloorChangedEvent>(OnFloorChanged);

            // 追溯创建: Start()在下一帧才执行, 此时Pawn可能已经创建了
            foreach (var kvp in _pawnModel.GetAllPawns())
            {
                OnPawnCreated(new SPawnCreatedEvent
                {
                    PawnId = kvp.Key,
                    Data = kvp.Value
                });
            }
        }

        private void Update()
        {
            if (_pawnSystem == null) return;

            _pawnSystem.Tick(Time.deltaTime);
            UpdateRendererPositions();
        }

        #region 事件回调

        private void OnPawnCreated(SPawnCreatedEvent evt)
        {
            if (_renderers.ContainsKey(evt.PawnId)) return;

            var def = TempConfigProvider.GetPawnDef(evt.Data.PawnDefId);

            var go = new GameObject($"Pawn_{evt.Data.Name}");
            go.transform.SetParent(transform, false);

            var sr = go.AddComponent<SpriteRenderer>();
            // 尝试从 Resources 加载自定义 Pawn 贴图, 无则使用程序化圆形
            var customSprite = Resources.Load<Sprite>($"Tiles/Pawn/Pawn_{evt.Data.PawnDefId}");
            sr.sprite = customSprite != null ? customSprite : _circleSprite;
            sr.color = def.Color;
            sr.sortingOrder = PawnConst.PawnSortOrderBase;

            float scale = def.SpriteSize;
            go.transform.localScale = new Vector3(scale, scale, 1f);

            // 初始位置
            go.transform.position = new Vector3(
                evt.Data.X + 0.5f, evt.Data.Y + 0.5f, 0f);

            // 楼层可见性
            go.SetActive(evt.Data.Floor == _currentFloor);

            _renderers[evt.PawnId] = sr;
        }

        private void OnPawnDestroyed(SPawnDestroyedEvent evt)
        {
            if (_renderers.TryGetValue(evt.PawnId, out var sr))
            {
                Destroy(sr.gameObject);
                _renderers.Remove(evt.PawnId);
            }
        }

        private void OnFloorChanged(SFloorChangedEvent evt)
        {
            _currentFloor = evt.NewFloor;
            UpdateFloorVisibility();
        }

        #endregion

        #region 渲染更新

        /// <summary>
        /// 平滑插值更新所有Pawn的显示位置
        /// </summary>
        private void UpdateRendererPositions()
        {
            foreach (var kvp in _renderers)
            {
                var pawn = _pawnModel.GetPawn(kvp.Key);
                if (pawn == null) continue;

                var sr = kvp.Value;

                // 计算目标世界坐标 (格子中心 + 移动插值偏移)
                float targetX = pawn.X + 0.5f;
                float targetY = pawn.Y + 0.5f;

                // 移动中: 加上朝下一格的插值偏移
                if (pawn.IsMoving && pawn.NextPathStep.HasValue)
                {
                    var next = pawn.NextPathStep.Value;
                    // 楼梯过渡步骤不做XY插值
                    if (!next.IsStairTransition && next.Floor == pawn.Floor)
                    {
                        float offsetX = (next.X - pawn.X) * pawn.MoveProgress;
                        float offsetY = (next.Y - pawn.Y) * pawn.MoveProgress;
                        targetX += offsetX;
                        targetY += offsetY;
                    }
                }

                // 平滑插值到目标位置
                Vector3 targetPos = new Vector3(targetX, targetY, 0f);
                sr.transform.position = Vector3.Lerp(
                    sr.transform.position, targetPos, Time.deltaTime * 15f);
            }
        }

        /// <summary>
        /// 更新楼层可见性
        /// </summary>
        private void UpdateFloorVisibility()
        {
            foreach (var kvp in _renderers)
            {
                var pawn = _pawnModel.GetPawn(kvp.Key);
                if (pawn == null) continue;
                kvp.Value.gameObject.SetActive(pawn.Floor == _currentFloor);
            }
        }

        #endregion

        #region 工具

        /// <summary>
        /// 程序化生成圆形Sprite (无需外部资源)
        /// </summary>
        private Sprite CreateCircleSprite()
        {
            int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            float center = size * 0.5f;
            float radius = center - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (dist <= radius)
                        tex.SetPixel(x, y, Color.white);
                    else if (dist <= radius + 1f)
                    {
                        // 抗锯齿边缘
                        float alpha = 1f - (dist - radius);
                        tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
                    }
                    else
                        tex.SetPixel(x, y, Color.clear);
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f), size);
        }

        #endregion

        private void OnDestroy()
        {
            foreach (var sr in _renderers.Values)
            {
                if (sr != null) Destroy(sr.gameObject);
            }
            _renderers.Clear();
        }

        public IArchitecture GetArchitecture()
        {
            return GameMain.Interface;
        }
    }
}
