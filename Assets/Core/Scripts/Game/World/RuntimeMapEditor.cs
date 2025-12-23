/**
 * RuntimeMapEditor.cs
 * 运行时地图编辑器
 * 
 * 提供在运行时编辑地图的功能：
 * - 放置/删除瓦片
 * - 放置/删除实体
 * - 笔刷工具
 * - 撤销/重做
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using GDFramework.MapSystem.Rendering;

namespace GDFramework.MapSystem
{
    /// <summary>
    /// 编辑模式
    /// </summary>
    public enum EditorMode
    {
        None,
        TilePaint,      // 绘制瓦片
        TileErase,      // 擦除瓦片
        EntityPlace,    // 放置实体
        EntitySelect,   // 选择实体
        EntityMove,     // 移动实体
        EntityDelete,   // 删除实体
        Fill,           // 填充区域
        Pick            // 吸取瓦片
    }
    
    /// <summary>
    /// 笔刷形状
    /// </summary>
    public enum BrushShape
    {
        Single,     // 单格
        Square,     // 方形
        Circle      // 圆形
    }
    
    /// <summary>
    /// 编辑操作（用于撤销/重做）
    /// </summary>
    public abstract class EditorOperation
    {
        public abstract void Undo(Map map, EntityManager entities);
        public abstract void Redo(Map map, EntityManager entities);
    }
    
    /// <summary>
    /// Tile 修改操作
    /// </summary>
    public class TileEditOperation : EditorOperation
    {
        public int Layer;
        public List<TileCoord> Coords;
        public List<TileLayerData> OldData;
        public List<TileLayerData> NewData;
        
        public TileEditOperation()
        {
            Coords = new List<TileCoord>();
            OldData = new List<TileLayerData>();
            NewData = new List<TileLayerData>();
        }
        
        public override void Undo(Map map, EntityManager entities)
        {
            for (int i = 0; i < Coords.Count; i++)
            {
                map.SetTileLayer(Coords[i], Layer, OldData[i]);
            }
        }
        
        public override void Redo(Map map, EntityManager entities)
        {
            for (int i = 0; i < Coords.Count; i++)
            {
                map.SetTileLayer(Coords[i], Layer, NewData[i]);
            }
        }
    }
    
    /// <summary>
    /// Entity 添加操作
    /// </summary>
    public class EntityAddOperation : EditorOperation
    {
        public int EntityId;
        public int ConfigId;
        public EntityType EntityType;
        public TileCoord Position;
        
        public override void Undo(Map map, EntityManager entities)
        {
            entities.RemoveEntity(EntityId);
        }
        
        public override void Redo(Map map, EntityManager entities)
        {
            entities.CreateEntityWithId(EntityId, ConfigId, EntityType, Position);
        }
    }
    
    /// <summary>
    /// Entity 删除操作
    /// </summary>
    public class EntityDeleteOperation : EditorOperation
    {
        public Saving.EntitySaveData SavedEntity;
        
        public override void Undo(Map map, EntityManager entities)
        {
            var entity = entities.CreateEntityWithId(
                SavedEntity.entityId,
                SavedEntity.configId,
                (EntityType)SavedEntity.entityType,
                new TileCoord(SavedEntity.tileX, SavedEntity.tileY)
            );
            SavedEntity.ApplyTo(entity);
        }
        
        public override void Redo(Map map, EntityManager entities)
        {
            entities.RemoveEntity(SavedEntity.entityId);
        }
    }
    
    /// <summary>
    /// 运行时地图编辑器
    /// </summary>
    public class RuntimeMapEditor : MonoBehaviour
    {
        #region 序列化字段
        
        [Header("References")]
        [SerializeField]
        private Camera _camera;
        
        [SerializeField]
        private MapRenderer _mapRenderer;
        
        [Header("Settings")]
        [SerializeField]
        private int _maxUndoSteps = 50;
        
        [Header("Current Tool")]
        [SerializeField]
        private EditorMode _currentMode = EditorMode.None;
        
        [SerializeField]
        private int _currentLayer = MapConstants.LAYER_GROUND;
        
        [SerializeField]
        private ushort _currentTileId = 1;
        
        [SerializeField]
        private int _currentEntityConfigId = 0;
        
        [SerializeField]
        private EntityType _currentEntityType = EntityType.Furniture;
        
        [Header("Brush")]
        [SerializeField]
        private BrushShape _brushShape = BrushShape.Single;
        
        [SerializeField]
        private int _brushSize = 1;
        
        #endregion
        
        #region 字段
        
        /// <summary>
        /// 当前编辑的地图
        /// </summary>
        private Map _map;
        
        /// <summary>
        /// 撤销栈
        /// </summary>
        private Stack<EditorOperation> _undoStack;
        
        /// <summary>
        /// 重做栈
        /// </summary>
        private Stack<EditorOperation> _redoStack;
        
        /// <summary>
        /// 当前正在进行的操作
        /// </summary>
        private TileEditOperation _currentTileOperation;
        
        /// <summary>
        /// 是否正在拖动
        /// </summary>
        private bool _isDragging;
        
        /// <summary>
        /// 上一次绘制的位置
        /// </summary>
        private TileCoord? _lastPaintPosition;
        
        /// <summary>
        /// 选中的实体
        /// </summary>
        private MapEntity _selectedEntity;
        
        /// <summary>
        /// 是否启用编辑器
        /// </summary>
        private bool _isEnabled;
        
        #endregion
        
        #region 属性
        
        public Map Map => _map;
        public EditorMode CurrentMode => _currentMode;
        public int CurrentLayer => _currentLayer;
        public ushort CurrentTileId => _currentTileId;
        public MapEntity SelectedEntity => _selectedEntity;
        public bool IsEnabled => _isEnabled;
        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
        
        #endregion
        
        #region 事件
        
        public event Action<TileCoord> OnTilePainted;
        public event Action<MapEntity> OnEntityPlaced;
        public event Action<MapEntity> OnEntitySelected;
        public event Action<MapEntity> OnEntityDeleted;
        
        #endregion
        
        #region Unity 生命周期
        
        void Awake()
        {
            _undoStack = new Stack<EditorOperation>();
            _redoStack = new Stack<EditorOperation>();
            
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }
        
        void Update()
        {
            if (!_isEnabled || _map == null) return;
            
            HandleInput();
        }
        
        #endregion
        
        #region 初始化
        
        /// <summary>
        /// 初始化编辑器
        /// </summary>
        public void Initialize(Map map, MapRenderer renderer)
        {
            _map = map;
            _mapRenderer = renderer;
            _isEnabled = true;
            
            // 清空历史
            _undoStack.Clear();
            _redoStack.Clear();
            
            Debug.Log("[RuntimeMapEditor] 初始化完成");
        }
        
        /// <summary>
        /// 启用/禁用编辑器
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;
            
            if (!enabled)
            {
                EndCurrentOperation();
            }
        }
        
        #endregion
        
        #region 工具设置
        
        /// <summary>
        /// 设置编辑模式
        /// </summary>
        public void SetMode(EditorMode mode)
        {
            if (_currentMode != mode)
            {
                EndCurrentOperation();
                _currentMode = mode;
                _selectedEntity = null;
            }
        }
        
        /// <summary>
        /// 设置当前编辑层
        /// </summary>
        public void SetLayer(int layer)
        {
            _currentLayer = Mathf.Clamp(layer, 0, MapConstants.TILE_LAYER_COUNT - 1);
        }
        
        /// <summary>
        /// 设置当前瓦片ID
        /// </summary>
        public void SetTileId(ushort tileId)
        {
            _currentTileId = tileId;
        }
        
        /// <summary>
        /// 设置当前实体配置
        /// </summary>
        public void SetEntityConfig(int configId, EntityType type)
        {
            _currentEntityConfigId = configId;
            _currentEntityType = type;
        }
        
        /// <summary>
        /// 设置笔刷
        /// </summary>
        public void SetBrush(BrushShape shape, int size)
        {
            _brushShape = shape;
            _brushSize = Mathf.Max(1, size);
        }
        
        #endregion
        
        #region 输入处理
        
        private void HandleInput()
        {
            // 获取鼠标位置对应的 Tile 坐标
            Vector2 mouseWorld = _camera.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
            TileCoord tileCoord = MapCoordUtility.WorldToTile(mouseWorld);
            
            bool validCoord = _map.IsTileCoordValid(tileCoord);
            
            // 鼠标按下
            if (UnityEngine.Input.GetMouseButtonDown(0) && validCoord)
            {
                OnMouseDown(tileCoord);
            }
            
            // 鼠标拖动
            if (UnityEngine.Input.GetMouseButton(0) && _isDragging && validCoord)
            {
                OnMouseDrag(tileCoord);
            }
            
            // 鼠标释放
            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                OnMouseUp(tileCoord);
            }
            
            // 快捷键
            HandleShortcuts();
        }
        
        private void OnMouseDown(TileCoord coord)
        {
            _isDragging = true;
            _lastPaintPosition = null;
            
            switch (_currentMode)
            {
                case EditorMode.TilePaint:
                    BeginTileOperation();
                    PaintTile(coord);
                    break;
                    
                case EditorMode.TileErase:
                    BeginTileOperation();
                    EraseTile(coord);
                    break;
                    
                case EditorMode.EntityPlace:
                    PlaceEntity(coord);
                    break;
                    
                case EditorMode.EntitySelect:
                    SelectEntityAt(coord);
                    break;
                    
                case EditorMode.EntityDelete:
                    DeleteEntityAt(coord);
                    break;
                    
                case EditorMode.Pick:
                    PickTile(coord);
                    break;
                    
                case EditorMode.Fill:
                    FillArea(coord);
                    break;
            }
        }
        
        private void OnMouseDrag(TileCoord coord)
        {
            if (_lastPaintPosition.HasValue && _lastPaintPosition.Value.Equals(coord))
            {
                return; // 同一位置不重复处理
            }
            
            switch (_currentMode)
            {
                case EditorMode.TilePaint:
                    PaintTile(coord);
                    break;
                    
                case EditorMode.TileErase:
                    EraseTile(coord);
                    break;
            }
            
            _lastPaintPosition = coord;
        }
        
        private void OnMouseUp(TileCoord coord)
        {
            _isDragging = false;
            _lastPaintPosition = null;
            
            EndCurrentOperation();
        }
        
        private void HandleShortcuts()
        {
            // Ctrl+Z - 撤销
            if (UnityEngine.Input.GetKey(KeyCode.LeftControl) && UnityEngine.Input.GetKeyDown(KeyCode.Z))
            {
                Undo();
            }
            
            // Ctrl+Y - 重做
            if (UnityEngine.Input.GetKey(KeyCode.LeftControl) && UnityEngine.Input.GetKeyDown(KeyCode.Y))
            {
                Redo();
            }
            
            // 数字键切换层
            for (int i = 0; i < MapConstants.TILE_LAYER_COUNT; i++)
            {
                if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SetLayer(i);
                }
            }
            
            // [ ] 调整笔刷大小
            if (UnityEngine.Input.GetKeyDown(KeyCode.LeftBracket))
            {
                _brushSize = Mathf.Max(1, _brushSize - 1);
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.RightBracket))
            {
                _brushSize = Mathf.Min(10, _brushSize + 1);
            }
        }
        
        #endregion
        
        #region Tile 编辑
        
        private void BeginTileOperation()
        {
            if (_currentTileOperation == null)
            {
                _currentTileOperation = new TileEditOperation
                {
                    Layer = _currentLayer
                };
            }
        }
        
        private void PaintTile(TileCoord center)
        {
            var coords = GetBrushCoords(center);
            var newData = new TileLayerData(_currentTileId);
            
            foreach (var coord in coords)
            {
                if (!_map.IsTileCoordValid(coord)) continue;
                
                var oldData = _map.GetTile(coord).GetLayer(_currentLayer);
                
                _map.SetTileLayer(coord, _currentLayer, newData);
                
                // 记录操作
                if (_currentTileOperation != null)
                {
                    _currentTileOperation.Coords.Add(coord);
                    _currentTileOperation.OldData.Add(oldData);
                    _currentTileOperation.NewData.Add(newData);
                }
                
                // 更新渲染
                _mapRenderer?.UpdateTile(coord);
                
                OnTilePainted?.Invoke(coord);
            }
        }
        
        private void EraseTile(TileCoord center)
        {
            var coords = GetBrushCoords(center);
            var newData = TileLayerData.Empty;
            
            foreach (var coord in coords)
            {
                if (!_map.IsTileCoordValid(coord)) continue;
                
                var oldData = _map.GetTile(coord).GetLayer(_currentLayer);
                
                _map.SetTileLayer(coord, _currentLayer, newData);
                
                // 记录操作
                if (_currentTileOperation != null)
                {
                    _currentTileOperation.Coords.Add(coord);
                    _currentTileOperation.OldData.Add(oldData);
                    _currentTileOperation.NewData.Add(newData);
                }
                
                // 更新渲染
                _mapRenderer?.UpdateTile(coord);
            }
        }
        
        private void PickTile(TileCoord coord)
        {
            var tile = _map.GetTile(coord);
            var layerData = tile.GetLayer(_currentLayer);
            
            if (!layerData.IsEmpty)
            {
                _currentTileId = layerData.tileId;
                Debug.Log($"[RuntimeMapEditor] 吸取瓦片: Layer={_currentLayer}, TileId={_currentTileId}");
            }
        }
        
        private void FillArea(TileCoord startCoord)
        {
            var tile = _map.GetTile(startCoord);
            var targetData = tile.GetLayer(_currentLayer);
            var newData = new TileLayerData(_currentTileId);
            
            // 如果目标已经是要填充的瓦片，跳过
            if (targetData.tileId == _currentTileId) return;
            
            BeginTileOperation();
            
            // 使用 BFS 填充
            var visited = new HashSet<TileCoord>();
            var queue = new Queue<TileCoord>();
            queue.Enqueue(startCoord);
            
            int maxFillCount = 10000; // 防止填充过大区域
            int fillCount = 0;
            
            while (queue.Count > 0 && fillCount < maxFillCount)
            {
                var coord = queue.Dequeue();
                
                if (visited.Contains(coord)) continue;
                if (!_map.IsTileCoordValid(coord)) continue;
                
                var currentData = _map.GetTile(coord).GetLayer(_currentLayer);
                if (!currentData.Equals(targetData)) continue;
                
                visited.Add(coord);
                fillCount++;
                
                // 填充当前格子
                _map.SetTileLayer(coord, _currentLayer, newData);
                
                if (_currentTileOperation != null)
                {
                    _currentTileOperation.Coords.Add(coord);
                    _currentTileOperation.OldData.Add(currentData);
                    _currentTileOperation.NewData.Add(newData);
                }
                
                _mapRenderer?.UpdateTile(coord);
                
                // 添加相邻格子
                queue.Enqueue(new TileCoord(coord.x + 1, coord.y));
                queue.Enqueue(new TileCoord(coord.x - 1, coord.y));
                queue.Enqueue(new TileCoord(coord.x, coord.y + 1));
                queue.Enqueue(new TileCoord(coord.x, coord.y - 1));
            }
            
            EndCurrentOperation();
            
            Debug.Log($"[RuntimeMapEditor] 填充完成: {fillCount} 个格子");
        }
        
        private void EndCurrentOperation()
        {
            if (_currentTileOperation != null && _currentTileOperation.Coords.Count > 0)
            {
                PushOperation(_currentTileOperation);
            }
            _currentTileOperation = null;
        }
        
        #endregion
        
        #region Entity 编辑
        
        private void PlaceEntity(TileCoord coord)
        {
            if (_currentEntityConfigId <= 0) return;
            
            var entity = _map.Entities.CreateEntity(
                _currentEntityConfigId,
                _currentEntityType,
                coord
            );
            
            // 记录操作
            var operation = new EntityAddOperation
            {
                EntityId = entity.EntityId,
                ConfigId = _currentEntityConfigId,
                EntityType = _currentEntityType,
                Position = coord
            };
            PushOperation(operation);
            
            // 刷新渲染
            _mapRenderer?.EntityRenderer?.CreateViewImmediate(entity);
            
            OnEntityPlaced?.Invoke(entity);
            
            Debug.Log($"[RuntimeMapEditor] 放置实体: {entity}");
        }
        
        private void SelectEntityAt(TileCoord coord)
        {
            var entities = _map.Entities.GetEntitiesAt(coord);
            
            _selectedEntity = entities.Count > 0 ? entities[0] : null;
            
            OnEntitySelected?.Invoke(_selectedEntity);
            
            if (_selectedEntity != null)
            {
                Debug.Log($"[RuntimeMapEditor] 选中实体: {_selectedEntity}");
            }
        }
        
        private void DeleteEntityAt(TileCoord coord)
        {
            var entities = _map.Entities.GetEntitiesAt(coord);
            
            if (entities.Count == 0) return;
            
            var entity = entities[0];
            
            // 记录操作
            var operation = new EntityDeleteOperation
            {
                SavedEntity = new Saving.EntitySaveData(entity)
            };
            PushOperation(operation);
            
            // 删除渲染
            _mapRenderer?.EntityRenderer?.DestroyViewImmediate(entity.EntityId);
            
            // 删除实体
            _map.Entities.RemoveEntity(entity.EntityId);
            
            OnEntityDeleted?.Invoke(entity);
            
            Debug.Log($"[RuntimeMapEditor] 删除实体: {entity}");
        }
        
        #endregion
        
        #region 撤销/重做
        
        private void PushOperation(EditorOperation operation)
        {
            _undoStack.Push(operation);
            _redoStack.Clear();
            
            // 限制撤销栈大小
            while (_undoStack.Count > _maxUndoSteps)
            {
                // 移除最老的操作（需要转换为列表操作，这里简化处理）
                var tempStack = new Stack<EditorOperation>();
                while (_undoStack.Count > 0)
                {
                    tempStack.Push(_undoStack.Pop());
                }
                tempStack.Pop(); // 移除最老的
                while (tempStack.Count > 0)
                {
                    _undoStack.Push(tempStack.Pop());
                }
            }
        }
        
        /// <summary>
        /// 撤销
        /// </summary>
        public void Undo()
        {
            if (_undoStack.Count == 0) return;
            
            var operation = _undoStack.Pop();
            operation.Undo(_map, _map.Entities);
            _redoStack.Push(operation);
            
            // 刷新渲染
            _mapRenderer?.ForceRefreshAll();
            
            Debug.Log("[RuntimeMapEditor] 撤销操作");
        }
        
        /// <summary>
        /// 重做
        /// </summary>
        public void Redo()
        {
            if (_redoStack.Count == 0) return;
            
            var operation = _redoStack.Pop();
            operation.Redo(_map, _map.Entities);
            _undoStack.Push(operation);
            
            // 刷新渲染
            _mapRenderer?.ForceRefreshAll();
            
            Debug.Log("[RuntimeMapEditor] 重做操作");
        }
        
        #endregion
        
        #region 笔刷
        
        /// <summary>
        /// 获取笔刷覆盖的坐标
        /// </summary>
        private List<TileCoord> GetBrushCoords(TileCoord center)
        {
            var coords = new List<TileCoord>();
            
            if (_brushSize == 1 || _brushShape == BrushShape.Single)
            {
                coords.Add(center);
                return coords;
            }
            
            int radius = _brushSize / 2;
            
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    bool include = false;
                    
                    switch (_brushShape)
                    {
                        case BrushShape.Square:
                            include = true;
                            break;
                            
                        case BrushShape.Circle:
                            include = dx * dx + dy * dy <= radius * radius;
                            break;
                    }
                    
                    if (include)
                    {
                        coords.Add(new TileCoord(center.x + dx, center.y + dy));
                    }
                }
            }
            
            return coords;
        }
        
        #endregion
        
        #region 调试绘制
        
        void OnDrawGizmos()
        {
            if (!_isEnabled || _map == null || _camera == null) return;
            
            // 绘制光标
            Vector2 mouseWorld = _camera.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
            TileCoord tileCoord = MapCoordUtility.WorldToTile(mouseWorld);
            
            if (_map.IsTileCoordValid(tileCoord))
            {
                var brushCoords = GetBrushCoords(tileCoord);
                
                Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
                
                foreach (var coord in brushCoords)
                {
                    Vector2 worldPos = MapCoordUtility.TileToWorld(coord);
                    Gizmos.DrawWireCube(
                        new Vector3(worldPos.x, worldPos.y, 0),
                        new Vector3(MapConstants.TILE_SIZE, MapConstants.TILE_SIZE, 0)
                    );
                }
            }
            
            // 绘制选中的实体
            if (_selectedEntity != null)
            {
                Gizmos.color = Color.cyan;
                Vector2 entityPos = _selectedEntity.WorldPosition;
                Gizmos.DrawWireCube(
                    new Vector3(entityPos.x, entityPos.y, 0),
                    new Vector3(MapConstants.TILE_SIZE * 1.2f, MapConstants.TILE_SIZE * 1.2f, 0)
                );
            }
        }
        
        #endregion
    }
}
