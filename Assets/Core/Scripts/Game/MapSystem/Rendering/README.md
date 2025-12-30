# TycoonGame 地图系统 - Rendering渲染模块

## 概述

本模块提供地图系统的可视化渲染功能，包括Tile渲染、实体渲染、调试显示和相机控制。

## 文件结构

```
MapSystem/
└── Rendering/
    ├── MapRenderer.cs       # 地图渲染管理器
    ├── TileRenderer.cs      # Tile渲染器
    ├── EntityRenderer.cs    # 实体渲染器
    ├── DebugRenderer.cs     # 调试渲染器
    ├── SpriteManager.cs     # 精灵资源管理
    ├── CameraController.cs  # 相机控制器
    ├── MapTestScene.cs      # 测试场景
    └── README.md
```

## 快速开始

### 方法1：使用测试场景

最简单的方式是使用 `MapTestScene`：

```csharp
// 1. 创建空GameObject
// 2. 挂载MapTestScene脚本
// 3. 运行即可看到完整的测试场景
```

### 方法2：手动设置

```csharp
// 1. 创建Site
var config = new SiteConfig { SizeX = 64, SizeZ = 64 };
var site = new Site(config);
site.Initialize();

// 2. 填充Tile数据
var floor = site.GroundFloor;
floor.TerrainGrid.SetDefId(new CellCoord(0, 0), "Terrain_Grass");
// ... 更多Tile

// 3. 创建相机
var cameraGO = new GameObject("Camera");
var camera = cameraGO.AddComponent<Camera>();
camera.orthographic = true;
camera.transform.position = new Vector3(32, 50, 32);
camera.transform.rotation = Quaternion.Euler(90, 0, 0);

// 4. 添加相机控制器
var cameraController = cameraGO.AddComponent<CameraController>();
cameraController.SetSite(site);

// 5. 创建MapRenderer
var rendererGO = new GameObject("MapRenderer");
var mapRenderer = rendererGO.AddComponent<MapRenderer>();
mapRenderer.SetSite(site);
```

## 核心组件

### MapRenderer

地图渲染的入口和协调者。

```csharp
// 设置要渲染的Site
mapRenderer.SetSite(site);

// 楼层切换
mapRenderer.SetCurrentFloor(1);
mapRenderer.GoUpFloor();
mapRenderer.GoDownFloor();

// 刷新渲染
mapRenderer.RefreshAll();
mapRenderer.RefreshCell(cell);
mapRenderer.RefreshRect(rect);

// 调试显示
mapRenderer.ShowRegionOverlay = true;
mapRenderer.ShowRoomOverlay = true;

// 坐标转换
CellCoord cell = mapRenderer.ScreenToCell(Input.mousePosition);
Vector3 world = mapRenderer.CellToWorld(cell);
```

### TileRenderer

渲染六层Tile（Terrain、Foundation、Floor、Cover、Wall、Roof）。

- 自动视口裁剪，只渲染可见区域
- 对象池优化，减少GC
- 支持自定义颜色（调试模式）
- 支持精灵纹理

### EntityRenderer

渲染所有实体（Building、Item、Pawn、Plant等）。

```csharp
// 自动根据EntityLister渲染所有实体
// 支持状态图标：建造中、损坏、无电等
// 支持生成/销毁特效
```

### DebugRenderer

调试可视化工具。

```csharp
// Region叠加显示
debugRenderer.Refresh(showRegions: true, showRooms: false);

// Room叠加显示
debugRenderer.Refresh(showRegions: false, showRooms: true);

// 路径显示
debugRenderer.ShowPath(pathResult.Path);
debugRenderer.ClearPath();

// 标记显示
debugRenderer.ShowMarker(cell, Color.red, duration: 2f);
```

### SpriteManager

精灵资源管理和动态生成。

```csharp
// 获取精灵
Sprite sprite = SpriteManager.Instance.GetSprite("Sprites/Terrain/Grass");

// 异步加载
SpriteManager.Instance.GetSpriteAsync("path", (sprite) => {
    // 使用sprite
});

// 动态生成精灵
Sprite solid = SpriteManager.Instance.CreateSolidSprite("name", Color.green);
Sprite circle = SpriteManager.Instance.CreateCircleSprite("name", Color.blue);
Sprite pattern = SpriteManager.Instance.CreatePatternSprite("name", Color.white, Color.gray, PatternType.Checker);

// 为Def生成精灵
Sprite tileSprite = SpriteManager.Instance.GenerateTileSprite(tileDef);
Sprite entitySprite = SpriteManager.Instance.GenerateEntitySprite(entityDef);
```

### CameraController

俯视角相机控制。

```csharp
// 设置Site
cameraController.SetSite(site);

// 控制
cameraController.MoveTo(cell);
cameraController.SetZoom(15f);
cameraController.FocusOnRect(rect);

// 获取鼠标位置
Vector3 worldPos = cameraController.GetMouseWorldPosition();
CellCoord cell = cameraController.GetMouseCell();

// 特效
cameraController.Shake(intensity: 0.5f, duration: 0.3f);
```

## 键盘/鼠标控制

| 操作 | 按键 |
|------|------|
| 移动 | WASD / 方向键 |
| 快速移动 | Shift + 移动键 |
| 缩放 | 鼠标滚轮 |
| 拖拽平移 | 鼠标中键拖拽 |
| 上楼 | Page Up |
| 下楼 | Page Down |
| Region叠加 | F1 |
| Room叠加 | F2 |
| 刷新渲染 | F5 |

## 颜色配置

### Tile颜色

渲染器会根据DefId自动分配颜色：

| DefId包含 | 颜色 |
|-----------|------|
| Grass | 绿色 |
| Dirt | 棕色 |
| Sand | 黄色 |
| Rock | 灰色 |
| Water | 蓝色 |
| Wood | 棕色 |
| Stone | 灰色 |
| Steel | 银灰色 |

### Entity颜色

| 类别 | 颜色 |
|------|------|
| Building | 灰色系 |
| Item | 根据材料 |
| Pawn | 肤色 |
| Plant | 绿色系 |

## 性能优化

### 视口裁剪

只渲染相机可见区域内的Tile和Entity，大幅减少渲染开销。

### 对象池

TileObject和EntityView使用对象池管理，避免频繁创建/销毁。

### 分层渲染

Tile按层级排序，使用SortingOrder确保正确的渲染顺序。

### 建议

1. 对于大地图，增加 `viewPadding` 预加载更多Tile
2. 使用精灵图集减少Draw Call
3. 对于大量实体，考虑GPU Instancing

## 扩展

### 自定义精灵

```csharp
// 在TileDef中设置SpritePath
var terrainDef = new TerrainDef("MyTerrain")
{
    SpritePath = "Sprites/Terrain/MyTerrain"
};

// 或者重写GetSprite方法
public override Sprite GetSprite()
{
    return SpriteManager.Instance.GetSprite(SpritePath);
}
```

### 自定义渲染器

```csharp
public class MyCustomRenderer : MonoBehaviour
{
    private MapRenderer _mapRenderer;

    public void Initialize(MapRenderer mapRenderer)
    {
        _mapRenderer = mapRenderer;
    }

    public void OnFloorChanged(int floorIndex)
    {
        // 处理楼层变化
    }

    public void Refresh()
    {
        // 刷新渲染
    }
}
```

## 依赖

- Unity 2020.3+
- MapSystem核心模块（Coords、Defs、Site、Entity、Region）

---

*TycoonGame MapSystem v1.0*
