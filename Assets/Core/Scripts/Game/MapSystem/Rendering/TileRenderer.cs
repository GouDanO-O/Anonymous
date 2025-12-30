/*******************************************************************************
 * 文件名:    TileRenderer.cs
 * 描述:      Tile渲染器，负责渲染六层Tile
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   TileRenderer 使用SpriteRenderer或Tilemap渲染Tile：
 *   - 支持六层Tile的分层渲染
 *   - 支持视口裁剪优化
 *   - 支持Tile动画
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TycoonGame.MapSystem.Rendering
{
    /// <summary>
    /// Tile渲染器
    /// </summary>
    public class TileRenderer : MonoBehaviour
    {
        #region 常量

        /// <summary>
        /// 渲染块大小
        /// </summary>
        private const int ChunkSize = 16;

        #endregion

        #region 字段

        /// <summary>
        /// 父渲染器
        /// </summary>
        private MapRenderer _mapRenderer;

        /// <summary>
        /// 当前Site
        /// </summary>
        private Site _site;

        /// <summary>
        /// 当前楼层索引
        /// </summary>
        private int _currentFloorIndex;

        /// <summary>
        /// 层级容器
        /// </summary>
        private Transform[] _layerContainers;

        /// <summary>
        /// Tile对象池
        /// </summary>
        private TileObjectPool _tilePool;

        /// <summary>
        /// 活动的TileObject
        /// </summary>
        private Dictionary<CellCoord, TileObject[]> _activeTiles;

        /// <summary>
        /// 当前可见区域
        /// </summary>
        private CellRect _visibleRect;

        /// <summary>
        /// 默认Tile材质
        /// </summary>
        private Material _defaultMaterial;

        /// <summary>
        /// 颜色缓存（用于调试显示）
        /// </summary>
        private Dictionary<string, Color> _defIdColors;

        #endregion

        #region 属性

        /// <summary>
        /// 当前楼层
        /// </summary>
        public Floor CurrentFloor => _site?.GetFloor(_currentFloorIndex);

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(MapRenderer mapRenderer)
        {
            _mapRenderer = mapRenderer;
            _activeTiles = new Dictionary<CellCoord, TileObject[]>();
            _defIdColors = new Dictionary<string, Color>();

            // 创建层级容器
            CreateLayerContainers();

            // 创建对象池
            _tilePool = new TileObjectPool(transform);

            // 创建默认材质
            CreateDefaultMaterial();
        }

        /// <summary>
        /// 创建层级容器
        /// </summary>
        private void CreateLayerContainers()
        {
            int layerCount = TileLayerExtensions.LayerCount;
            _layerContainers = new Transform[layerCount];

            for (int i = 0; i < layerCount; i++)
            {
                var layer = (TileLayer)i;
                var container = new GameObject($"Layer_{layer}");
                container.transform.SetParent(transform);
                container.transform.localPosition = new Vector3(0, i * 0.01f, 0); // 微小Y偏移避免Z-fighting
                _layerContainers[i] = container.transform;
            }
        }

        /// <summary>
        /// 创建默认材质
        /// </summary>
        private void CreateDefaultMaterial()
        {
            // 使用Sprites/Default着色器
            var shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                _defaultMaterial = new Material(shader);
            }
        }

        /// <summary>
        /// 设置Site
        /// </summary>
        public void SetSite(Site site)
        {
            _site = site;
            Clear();
        }

        #endregion

        #region 楼层切换

        /// <summary>
        /// 楼层变更回调
        /// </summary>
        public void OnFloorChanged(int floorIndex)
        {
            _currentFloorIndex = floorIndex;
            Clear();
        }

        #endregion

        #region 可见区域

        /// <summary>
        /// 可见区域变更回调
        /// </summary>
        public void OnVisibleRectChanged(CellRect visibleRect)
        {
            _visibleRect = visibleRect;
            UpdateVisibleTiles();
        }

        /// <summary>
        /// 更新可见Tile
        /// </summary>
        private void UpdateVisibleTiles()
        {
            if (_site == null)
            {
                Debug.LogWarning("[TileRenderer] UpdateVisibleTiles: site is null");
                return;
            }

            var floor = CurrentFloor;
            if (floor == null)
            {
                Debug.LogWarning($"[TileRenderer] UpdateVisibleTiles: floor is null (index={_currentFloorIndex})");
                return;
            }

            // 收集需要移除的Tile
            var toRemove = new List<CellCoord>();
            foreach (var kvp in _activeTiles)
            {
                if (!_visibleRect.Contains(kvp.Key))
                {
                    toRemove.Add(kvp.Key);
                }
            }

            // 移除不可见的Tile
            foreach (var cell in toRemove)
            {
                RemoveTileAt(cell);
            }

            // 添加新可见的Tile
            int addedCount = 0;
            foreach (var cell in _visibleRect.GetCells())
            {
                if (!_activeTiles.ContainsKey(cell))
                {
                    CreateTileAt(cell);
                    addedCount++;
                }
            }
            
            if (addedCount > 0)
            {
                Debug.Log($"[TileRenderer] UpdateVisibleTiles: added {addedCount} tiles, rect={_visibleRect}, total={_activeTiles.Count}");
            }
        }

        #endregion

        #region Tile创建/移除

        /// <summary>
        /// 在指定位置创建Tile对象
        /// </summary>
        private void CreateTileAt(CellCoord cell)
        {
            var floor = CurrentFloor;
            if (floor == null)
                return;

            float cellSize = _site.CellSize;
            Vector3 worldPos = new Vector3(
                (cell.x + 0.5f) * cellSize,
                0,
                (cell.z + 0.5f) * cellSize
            );

            var tileObjects = new TileObject[TileLayerExtensions.LayerCount];

            for (int layerIndex = 0; layerIndex < TileLayerExtensions.LayerCount; layerIndex++)
            {
                var layer = (TileLayer)layerIndex;
                var grid = floor.GetTileGrid(layer);
                var defId = grid.GetDefId(cell);

                if (string.IsNullOrEmpty(defId))
                    continue;

                var tileObj = _tilePool.Get();
                tileObj.transform.SetParent(_layerContainers[layerIndex]);
                tileObj.transform.position = worldPos + new Vector3(0, layerIndex * 0.01f, 0);
                tileObj.transform.localScale = new Vector3(cellSize, cellSize, 1);

                // 设置渲染
                SetupTileRenderer(tileObj, layer, defId, cell);

                tileObjects[layerIndex] = tileObj;
            }

            _activeTiles[cell] = tileObjects;
        }

        /// <summary>
        /// 设置Tile渲染器
        /// </summary>
        private void SetupTileRenderer(TileObject tileObj, TileLayer layer, string defId, CellCoord cell)
        {
            var spriteRenderer = tileObj.SpriteRenderer;
            spriteRenderer.sortingOrder = (int)layer;

            // 获取Def
            var tileDef = DefDatabase.GetDef<TileDef>(defId);

            if (tileDef != null)
            {
                // 尝试加载精灵
                var sprite = tileDef.GetSprite();
                if (sprite != null)
                {
                    spriteRenderer.sprite = sprite;
                    spriteRenderer.color = tileDef.TileColor;
                }
                else
                {
                    // 使用颜色方块表示
                    spriteRenderer.sprite = GetDefaultSprite();
                    spriteRenderer.color = GetColorForDef(defId, layer);
                }
            }
            else
            {
                // 未找到Def，使用调试颜色
                spriteRenderer.sprite = GetDefaultSprite();
                spriteRenderer.color = GetColorForDef(defId, layer);
            }
        }

        /// <summary>
        /// 获取DefId对应的颜色（调试用）
        /// </summary>
        private Color GetColorForDef(string defId, TileLayer layer)
        {
            if (_defIdColors.TryGetValue(defId, out var color))
                return color;

            // 根据层级和DefId生成颜色
            switch (layer)
            {
                case TileLayer.Terrain:
                    if (defId.Contains("Grass")) color = new Color(0.3f, 0.7f, 0.3f);
                    else if (defId.Contains("Dirt")) color = new Color(0.6f, 0.4f, 0.2f);
                    else if (defId.Contains("Sand")) color = new Color(0.9f, 0.8f, 0.5f);
                    else if (defId.Contains("Rock")) color = new Color(0.5f, 0.5f, 0.5f);
                    else if (defId.Contains("Water")) color = new Color(0.2f, 0.4f, 0.8f);
                    else if (defId.Contains("Lava")) color = new Color(1f, 0.3f, 0f);
                    else color = new Color(0.5f, 0.5f, 0.3f);
                    break;

                case TileLayer.Foundation:
                    if (defId.Contains("None")) color = Color.clear;
                    else color = new Color(0.4f, 0.4f, 0.4f, 0.5f);
                    break;

                case TileLayer.Floor:
                    if (defId.Contains("None")) color = Color.clear;
                    else if (defId.Contains("Wood")) color = new Color(0.6f, 0.4f, 0.2f);
                    else if (defId.Contains("Stone")) color = new Color(0.7f, 0.7f, 0.7f);
                    else if (defId.Contains("Steel")) color = new Color(0.6f, 0.6f, 0.7f);
                    else if (defId.Contains("Carpet")) color = new Color(0.8f, 0.2f, 0.2f);
                    else color = new Color(0.6f, 0.6f, 0.6f);
                    break;

                case TileLayer.Cover:
                    if (defId.Contains("Blood")) color = new Color(0.6f, 0f, 0f, 0.7f);
                    else if (defId.Contains("Dirt")) color = new Color(0.4f, 0.3f, 0.2f, 0.5f);
                    else if (defId.Contains("Snow")) color = new Color(1f, 1f, 1f, 0.8f);
                    else color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    break;

                case TileLayer.Wall:
                    if (defId.Contains("None")) color = Color.clear;
                    else if (defId.Contains("Wood")) color = new Color(0.5f, 0.3f, 0.1f);
                    else if (defId.Contains("Stone")) color = new Color(0.6f, 0.6f, 0.6f);
                    else if (defId.Contains("Steel")) color = new Color(0.5f, 0.5f, 0.6f);
                    else if (defId.Contains("Glass")) color = new Color(0.7f, 0.9f, 1f, 0.5f);
                    else if (defId.Contains("Door")) color = new Color(0.4f, 0.25f, 0.1f);
                    else color = new Color(0.5f, 0.5f, 0.5f);
                    break;

                case TileLayer.Roof:
                    if (defId.Contains("None")) color = Color.clear;
                    else color = new Color(0.3f, 0.3f, 0.3f, 0.7f);
                    break;

                default:
                    color = Color.magenta;
                    break;
            }

            _defIdColors[defId] = color;
            return color;
        }

        /// <summary>
        /// 移除指定位置的Tile
        /// </summary>
        private void RemoveTileAt(CellCoord cell)
        {
            if (_activeTiles.TryGetValue(cell, out var tileObjects))
            {
                foreach (var tileObj in tileObjects)
                {
                    if (tileObj != null)
                    {
                        _tilePool.Return(tileObj);
                    }
                }
                _activeTiles.Remove(cell);
            }
        }

        #endregion

        #region 刷新

        /// <summary>
        /// 刷新所有可见Tile
        /// </summary>
        public void Refresh()
        {
            Clear();
            UpdateVisibleTiles();
        }

        /// <summary>
        /// 刷新指定格子
        /// </summary>
        public void RefreshCell(CellCoord cell)
        {
            if (_visibleRect.Contains(cell))
            {
                RemoveTileAt(cell);
                CreateTileAt(cell);
            }
        }

        /// <summary>
        /// 刷新指定区域
        /// </summary>
        public void RefreshRect(CellRect rect)
        {
            foreach (var cell in rect.GetCells())
            {
                if (_visibleRect.Contains(cell))
                {
                    RefreshCell(cell);
                }
            }
        }

        /// <summary>
        /// 清除所有渲染
        /// </summary>
        public void Clear()
        {
            foreach (var kvp in _activeTiles)
            {
                foreach (var tileObj in kvp.Value)
                {
                    if (tileObj != null)
                    {
                        _tilePool.Return(tileObj);
                    }
                }
            }
            _activeTiles.Clear();
        }

        #endregion

        #region 辅助

        /// <summary>
        /// 默认精灵缓存
        /// </summary>
        private Sprite _defaultSprite;

        /// <summary>
        /// 获取默认精灵（白色方块）
        /// </summary>
        private Sprite GetDefaultSprite()
        {
            if (_defaultSprite == null)
            {
                // 创建1x1白色纹理
                var texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();

                _defaultSprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, 1, 1),
                    new Vector2(0.5f, 0.5f),
                    1
                );
            }
            return _defaultSprite;
        }

        #endregion
    }

    /// <summary>
    /// Tile对象
    /// </summary>
    public class TileObject : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;

        public SpriteRenderer SpriteRenderer
        {
            get
            {
                if (_spriteRenderer == null)
                {
                    _spriteRenderer = GetComponent<SpriteRenderer>();
                    if (_spriteRenderer == null)
                    {
                        _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                    }
                }
                return _spriteRenderer;
            }
        }

        public void Reset()
        {
            SpriteRenderer.sprite = null;
            SpriteRenderer.color = Color.white;
        }
    }

    /// <summary>
    /// Tile对象池
    /// </summary>
    public class TileObjectPool
    {
        private Transform _parent;
        private Queue<TileObject> _pool;
        private int _created;

        public TileObjectPool(Transform parent)
        {
            _parent = parent;
            _pool = new Queue<TileObject>();
        }

        public TileObject Get()
        {
            TileObject tileObj;
            if (_pool.Count > 0)
            {
                tileObj = _pool.Dequeue();
                tileObj.gameObject.SetActive(true);
            }
            else
            {
                var go = new GameObject($"Tile_{_created++}");
                go.transform.SetParent(_parent);
                // SpriteRenderer默认朝向+Z，旋转-90度使其朝向+Y（面向上方，被俯视相机看到）
                go.transform.rotation = Quaternion.Euler(-90, 0, 0);
                tileObj = go.AddComponent<TileObject>();
            }
            return tileObj;
        }

        public void Return(TileObject tileObj)
        {
            tileObj.Reset();
            tileObj.gameObject.SetActive(false);
            _pool.Enqueue(tileObj);
        }
    }
}
