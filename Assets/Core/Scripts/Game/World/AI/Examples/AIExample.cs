/**
 * AIExample.cs
 * AI 系统示例
 * 
 * 演示：
 * - 生成僵尸和 NPC
 * - AI 行为展示
 * - 声音系统
 * - 玩家交互
 */

using UnityEngine;
using GDFramework.MapSystem.AI.Zombie;
using GDFramework.MapSystem.AI.NPC;
using GDFramework.MapSystem.Rendering;
using System.Collections.Generic;

namespace GDFramework.MapSystem.AI
{
    /// <summary>
    /// AI 示例场景
    /// </summary>
    public class AIExample : MonoBehaviour
    {
        #region 序列化字段
        
        [Header("Map Settings")]
        [SerializeField]
        private int _mapWidth = 4;
        
        [SerializeField]
        private int _mapHeight = 4;
        
        [Header("AI Settings")]
        [SerializeField]
        private int _initialZombieCount = 5;
        
        [SerializeField]
        private int _initialNPCCount = 2;
        
        [Header("Player")]
        [SerializeField]
        private float _playerMoveSpeed = 5f;
        
        [Header("Debug")]
        [SerializeField]
        private bool _showAIDebug = true;
        
        #endregion
        
        #region 字段
        
        private Map _map;
        private MapRenderer _renderer;
        private AIManager _aiManager;
        
        // 模拟玩家
        private Vector2 _playerPosition;
        private MapEntity _playerEntity;
        
        #endregion
        
        #region Unity 生命周期
        
        void Start()
        {
            Debug.Log("=== AI 系统示例 ===");
            
            InitializeSprites();
            CreateMap();
            InitializeRendering();
            InitializeAI();
            SpawnInitialAI();
            CreatePlayer();
            
            PrintInstructions();
        }
        
        void Update()
        {
            HandleInput();
            UpdatePlayer();
        }
        
        void OnDestroy()
        {
            _aiManager?.DespawnAll();
            _renderer?.Cleanup();
        }
        
        #endregion
        
        #region 初始化
        
        private void InitializeSprites()
        {
            var tileConfigs = new TileConfig[]
            {
                TileConfig.CreateTerrain(1, "草地", "terrain_grass"),
                TileConfig.CreateTerrain(2, "泥土", "terrain_dirt"),
                TileConfig.CreateFloor(10, "木地板", "floor_wood"),
                TileConfig.CreateWall(20, "木墙", "wall_wood", 100),
                TileConfig.CreateRoof(30, "屋顶", "roof_wood"),
            };
            
            var entityConfigs = new EntityConfig[]
            {
                EntityConfig.CreateDoor(1001, "木门", new[] { "door_closed", "door_open" }),
            };
            
            SpriteManager.Instance.Initialize(tileConfigs, entityConfigs);
        }
        
        private void CreateMap()
        {
            _map = new Map("ai_demo", "AI演示", _mapWidth, _mapHeight, MapType.Outdoor);
            
            // 填充地面
            _map.FillLayer(MapConstants.LAYER_GROUND, TileLayerData.Create(1));
            
            // 建造几个房子
            BuildHouse(10, 10, 8, 6);
            BuildHouse(25, 15, 6, 5);
            BuildHouse(15, 25, 7, 7);
            
            // 建造围墙
            BuildWall(5, 5, 40, false); // 水平墙
            BuildWall(5, 5, 35, true);  // 垂直墙
            BuildWall(5, 40, 40, false);
            BuildWall(45, 5, 35, true);
            
            Debug.Log($"地图创建完成: {_map}");
        }
        
        private void BuildHouse(int x, int y, int w, int h)
        {
            for (int dy = 0; dy < h; dy++)
            {
                for (int dx = 0; dx < w; dx++)
                {
                    var coord = new TileCoord(x + dx, y + dy);
                    if (!_map.IsTileCoordValid(coord)) continue;
                    
                    bool isEdge = dx == 0 || dx == w - 1 || dy == 0 || dy == h - 1;
                    bool isDoor = dy == 0 && dx == w / 2;
                    
                    if (isDoor)
                    {
                        _map.SetTile(coord, TileData.Empty
                            .WithFloor(10)
                            .WithRoof(30));
                        _map.Entities.CreateDoor(1001, coord, DoorType.Wooden);
                    }
                    else if (isEdge)
                    {
                        _map.SetTile(coord, TileData.Empty
                            .WithFloor(10)
                            .WithWall(20)
                            .WithRoof(30));
                    }
                    else
                    {
                        _map.SetTile(coord, TileData.Empty
                            .WithFloor(10)
                            .WithRoof(30));
                    }
                }
            }
        }
        
        private void BuildWall(int startX, int startY, int length, bool vertical)
        {
            for (int i = 0; i < length; i++)
            {
                int x = vertical ? startX : startX + i;
                int y = vertical ? startY + i : startY;
                var coord = new TileCoord(x, y);
                
                if (!_map.IsTileCoordValid(coord)) continue;
                
                // 每隔一段留个缺口
                if (i % 10 == 5) continue;
                
                var tile = _map.GetTile(coord);
                if (!tile.HasWall)
                {
                    tile = tile.WithWall(20);
                    _map.SetTile(coord, tile);
                }
            }
        }
        
        private void InitializeRendering()
        {
            var rendererGo = new GameObject("MapRenderer");
            _renderer = rendererGo.AddComponent<MapRenderer>();
            _renderer.UseLighting = true;
            _renderer.Initialize(_map);
            
            // 设置相机
            var camera = Camera.main;
            if (camera != null)
            {
                camera.orthographic = true;
                camera.orthographicSize = 15f;
            }
        }
        
        private void InitializeAI()
        {
            _aiManager = AIManager.Instance;
            _aiManager.Initialize(_map);
            
            // 初始化寻路系统
            Pathfinding.PathfindingManager.Instance.Initialize(_map);
        }
        
        private void SpawnInitialAI()
        {
            // 生成僵尸
            for (int i = 0; i < _initialZombieCount; i++)
            {
                Vector2 pos = GetRandomSpawnPosition();
                var type = (ZombieType)Random.Range(0, 3); // Walker, Crawler, Runner
                _aiManager.SpawnZombie(pos, type);
            }
            
            // 生成 NPC
            for (int i = 0; i < _initialNPCCount; i++)
            {
                Vector2 pos = GetRandomSpawnPosition();
                var npc = _aiManager.SpawnNPC(pos, NPCType.Survivor);
                
                // 设置巡逻点
                if (npc != null)
                {
                    var patrolPoints = new List<TileCoord>
                    {
                        MapCoordUtility.WorldToTile(pos),
                        MapCoordUtility.WorldToTile(pos + new Vector2(5, 0)),
                        MapCoordUtility.WorldToTile(pos + new Vector2(5, 5)),
                        MapCoordUtility.WorldToTile(pos + new Vector2(0, 5))
                    };
                    npc.Blackboard.Set(AIBlackboard.KEY_PATROL_POINTS, patrolPoints);
                }
            }
            
            Debug.Log($"生成 AI: {_initialZombieCount} 僵尸, {_initialNPCCount} NPC");
        }
        
        private void CreatePlayer()
        {
            _playerPosition = new Vector2(20, 20);
            
            // 创建玩家实体
            _playerEntity = _map.Entities.CreateEntity(0, EntityType.Player, 
                MapCoordUtility.WorldToTile(_playerPosition));
            
            // 更新相机位置
            UpdateCameraPosition();
        }
        
        private Vector2 GetRandomSpawnPosition()
        {
            int maxTiles = _mapWidth * MapConstants.CHUNK_SIZE;
            
            for (int attempts = 0; attempts < 50; attempts++)
            {
                int x = Random.Range(10, maxTiles - 10);
                int y = Random.Range(10, maxTiles - 10);
                var coord = new TileCoord(x, y);
                
                if (Pathfinding.PathfindingManager.Instance.IsWalkable(coord))
                {
                    return MapCoordUtility.TileToWorldCenter(coord);
                }
            }
            
            return new Vector2(20, 20);
        }
        
        #endregion
        
        #region 输入处理
        
        private void HandleInput()
        {
            // 生成更多僵尸
            if (UnityEngine.Input.GetKeyDown(KeyCode.Z))
            {
                Vector2 pos = GetRandomSpawnPosition();
                _aiManager.SpawnZombie(pos);
                Debug.Log($"生成僵尸于 {pos}");
            }
            
            // 生成 NPC
            if (UnityEngine.Input.GetKeyDown(KeyCode.N))
            {
                Vector2 pos = GetRandomSpawnPosition();
                _aiManager.SpawnNPC(pos);
                Debug.Log($"生成 NPC 于 {pos}");
            }
            
            // 发出声音（吸引僵尸）
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                SoundSystem.EmitSound(_playerPosition, 0.8f, SoundType.Alert, this);
                Debug.Log($"发出声音于 {_playerPosition}");
            }
            
            // 开枪（大声音）
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
                SoundSystem.EmitGunshot(mousePos);
                Debug.Log($"开枪于 {mousePos}");
            }
            
            // 清除所有 AI
            if (UnityEngine.Input.GetKeyDown(KeyCode.C))
            {
                _aiManager.DespawnAll();
                Debug.Log("清除所有 AI");
            }
            
            // 切换调试显示
            if (UnityEngine.Input.GetKeyDown(KeyCode.F1))
            {
                _showAIDebug = !_showAIDebug;
            }
        }
        
        private void UpdatePlayer()
        {
            // 玩家移动
            Vector2 moveDir = Vector2.zero;
            if (UnityEngine.Input.GetKey(KeyCode.W) || UnityEngine.Input.GetKey(KeyCode.UpArrow)) moveDir.y += 1;
            if (UnityEngine.Input.GetKey(KeyCode.S) || UnityEngine.Input.GetKey(KeyCode.DownArrow)) moveDir.y -= 1;
            if (UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow)) moveDir.x -= 1;
            if (UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow)) moveDir.x += 1;
            
            if (moveDir != Vector2.zero)
            {
                moveDir.Normalize();
                _playerPosition += moveDir * _playerMoveSpeed * Time.deltaTime;
                
                // 更新实体位置
                _playerEntity?.SetWorldPosition(_playerPosition);
                
                // 更新相机
                UpdateCameraPosition();
                
                // 发出脚步声
                bool running = UnityEngine.Input.GetKey(KeyCode.LeftShift);
                if (Time.frameCount % (running ? 10 : 20) == 0)
                {
                    SoundSystem.EmitFootstep(_playerPosition, running);
                }
            }
        }
        
        private void UpdateCameraPosition()
        {
            var camera = Camera.main;
            if (camera != null)
            {
                camera.transform.position = new Vector3(_playerPosition.x, _playerPosition.y, -10);
            }
        }
        
        #endregion
        
        #region 调试
        
        void OnGUI()
        {
            if (!_showAIDebug) return;
            
            GUILayout.BeginArea(new Rect(Screen.width - 220, 10, 210, 400));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("=== AI 示例 ===");
            GUILayout.Label($"玩家位置: {_playerPosition:F1}");
            GUILayout.Label($"活跃 AI: {_aiManager.ActiveAgentCount}");
            
            // 显示附近的僵尸
            int nearbyZombies = _aiManager.GetZombieCountInRange(_playerPosition, 10f);
            GUILayout.Label($"附近僵尸 (10格): {nearbyZombies}");
            
            GUILayout.Space(10);
            GUILayout.Label("--- 控制 ---");
            GUILayout.Label("WASD: 移动玩家");
            GUILayout.Label("Shift: 奔跑");
            GUILayout.Label("Space: 发出声音");
            GUILayout.Label("左键: 开枪");
            GUILayout.Label("Z: 生成僵尸");
            GUILayout.Label("N: 生成 NPC");
            GUILayout.Label("C: 清除所有 AI");
            GUILayout.Label("F1: 切换调试");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            
            // 绘制玩家
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere((Vector3)_playerPosition, 0.5f);
            
            // 绘制玩家视野范围
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawWireSphere((Vector3)_playerPosition, 10f);
        }
        
        #endregion
        
        #region 辅助
        
        private void PrintInstructions()
        {
            Debug.Log(@"
=== AI 系统示例 控制说明 ===

【玩家移动】
WASD / 方向键: 移动
Shift: 奔跑（更大脚步声）

【交互】
Space: 发出声音（吸引僵尸）
左键点击: 开枪（大声音）

【生成】
Z: 生成僵尸
N: 生成 NPC
C: 清除所有 AI

【调试】
F1: 切换调试信息

================================
");
        }
        
        #endregion
    }
}
