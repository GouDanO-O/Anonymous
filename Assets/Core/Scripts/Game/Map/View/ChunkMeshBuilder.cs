using System.Collections.Generic;
using Core.Game.Blueprint.Data;
using Core.Game.Blueprint.Define;
using Core.Game.Blueprint.Model;
using Core.Game.Config;
using Core.Game.Item.Define;
using Core.Game.Item.Model;
using Core.Game.Map.Data;
using Core.Game.Map.Define;
using Core.Game.Resource.Data;
using Core.Game.Resource.Define;
using Core.Game.Resource.Model;
using UnityEngine;

namespace Core.Game.Map.View
{
    /// <summary>
    /// Chunk Mesh构建器
    /// 为每个渲染层(地形/地板/墙/屋顶)生成Mesh
    /// 支持 Atlas UV 映射和 Autotile
    /// </summary>
    public class ChunkMeshBuilder
    {
        // Mesh构建缓存 (减少GC)
        private List<Vector3> _vertices = new List<Vector3>();
        private List<int> _triangles = new List<int>();
        private List<Vector2> _uvs = new List<Vector2>();
        private List<Color> _colors = new List<Color>();

        /// <summary>
        /// 构建地形层Mesh
        /// </summary>
        public Mesh BuildTerrainMesh(ChunkData chunk, int mapWidth, int mapHeight)
        {
            ClearBuffers();

            chunk.ForEachCell((cell, lx, ly) =>
            {
                if (cell.TerrainDefId == MapConst.InvalidDefId) return;
                var uv = TileAtlasManager.GetTerrainUV(cell.TerrainDefId);
                AddQuad(lx, ly, GetTerrainColor(cell.TerrainDefId), uv);
            });

            return CreateMesh("Terrain");
        }

        /// <summary>
        /// 构建地板层Mesh
        /// </summary>
        public Mesh BuildFloorMesh(ChunkData chunk, int mapWidth, int mapHeight)
        {
            ClearBuffers();

            chunk.ForEachCell((cell, lx, ly) =>
            {
                if (cell.FloorDefId == MapConst.InvalidDefId) return;
                var uv = TileAtlasManager.GetFloorUV(cell.FloorDefId);
                AddQuad(lx, ly, GetFloorColor(cell.FloorDefId), uv);
            });

            return CreateMesh("Floor");
        }

        /// <summary>
        /// 构建结构层Mesh (墙/门/窗 — 独占整格)
        /// Wall 类型支持 Autotile: 根据 4 邻接位掩码选择贴图变体
        /// </summary>
        public Mesh BuildWallMesh(ChunkData chunk, int mapWidth, int mapHeight,
            MapData mapData = null, int floor = 0)
        {
            ClearBuffers();

            chunk.ForEachCell((cell, lx, ly) =>
            {
                if (!cell.HasStructure) return;

                var def = ConfigManager.GetStructureDef(cell.StructureDefId);
                Color color = ConfigManager.ToUnityColor(def.Color);

                if ((EStructureType)def.StructureType == EStructureType.Door)
                {
                    // 门: 稍小的矩形以示区分
                    float inset = 0.15f;
                    var uv = TileAtlasManager.GetStructureUV(cell.StructureDefId, 0);
                    AddRect(lx + inset, ly + inset, lx + 1f - inset, ly + 1f - inset, color, uv);
                }
                else if ((EStructureType)def.StructureType == EStructureType.Window)
                {
                    // 窗: 略小矩形 + 半透明
                    float inset = 0.1f;
                    color.a = 0.7f;
                    var uv = TileAtlasManager.GetStructureUV(cell.StructureDefId, 0);
                    AddRect(lx + inset, ly + inset, lx + 1f - inset, ly + 1f - inset, color, uv);
                }
                else if ((EStructureType)def.StructureType == EStructureType.Pillar)
                {
                    // 柱子: 居中小方块
                    float inset = 0.3f;
                    var uv = TileAtlasManager.GetStructureUV(cell.StructureDefId, 0);
                    AddRect(lx + inset, ly + inset, lx + 1f - inset, ly + 1f - inset, color, uv);
                }
                else if ((EStructureType)def.StructureType == EStructureType.Stair)
                {
                    // 楼梯: 整格底色 + 居中箭头色块(模拟向上箭头)
                    var uv = TileAtlasManager.GetStructureUV(cell.StructureDefId, 0);
                    color.a = 0.6f;
                    AddQuad(lx, ly, color, uv);
                    // 箭头指示 (居中较亮色块, 使用 fallback UV)
                    Color arrowColor = new Color(
                        Mathf.Min(color.r + 0.3f, 1f),
                        Mathf.Min(color.g + 0.3f, 1f),
                        Mathf.Min(color.b + 0.3f, 1f), 0.9f);
                    AddRect(lx + 0.35f, ly + 0.15f, lx + 0.65f, ly + 0.85f, arrowColor,
                        TileAtlasManager.FallbackUV);
                }
                else
                {
                    // 墙: Autotile
                    int worldX = chunk.WorldStartX + lx;
                    int worldY = chunk.WorldStartY + ly;
                    int bitmask = ComputeAutotileBitmask(worldX, worldY, floor,
                        cell.StructureDefId, mapData);
                    var uv = TileAtlasManager.GetStructureUV(cell.StructureDefId, bitmask);
                    AddQuad(lx, ly, color, uv);
                }
            });

            return CreateMesh("Wall");
        }

        /// <summary>
        /// 构建屋顶层Mesh
        /// </summary>
        public Mesh BuildRoofMesh(ChunkData chunk, int mapWidth, int mapHeight)
        {
            ClearBuffers();

            var roofUV = TileAtlasManager.GetRoofUV();

            chunk.ForEachCell((cell, lx, ly) =>
            {
                if (!cell.HasRoof) return;
                AddQuad(lx, ly, new Color(0.4f, 0.35f, 0.3f, 1f), roofUV);
            });

            return CreateMesh("Roof");
        }

        /// <summary>
        /// 构建物品/家具层Mesh
        /// 只在锚点格绘制完整矩形, 避免重复
        /// </summary>
        public Mesh BuildObjectMesh(ChunkData chunk, ItemDataModel itemModel,
            MaterialDataModel materialModel, int floor)
        {
            ClearBuffers();

            // 记录已绘制的物品ID, 避免多格物品重复绘制
            var drawnItems = new HashSet<long>();

            chunk.ForEachCell((cell, lx, ly) =>
            {
                if (cell.ObjectIds == null) return;

                foreach (long objId in cell.ObjectIds)
                {
                    if (drawnItems.Contains(objId)) continue;

                    // 材料堆 (ID >= 2_000_000)
                    if (objId >= MaterialDataModel.MaterialStackIdStart && materialModel != null)
                    {
                        var stack = materialModel.GetStack(objId);
                        if (stack == null || stack.Floor != floor) continue;
                        if (stack.X != cell.X || stack.Y != cell.Y) continue;

                        drawnItems.Add(objId);

                        Color color = GetMaterialColor(stack.Type);
                        float padding = 0.2f;
                        AddRect(lx + padding, ly + padding,
                            lx + 1f - padding, ly + 1f - padding, color);
                        continue;
                    }

                    // 物品/家具 (ID >= 1_000_000)
                    if (objId >= ItemConst.ItemIdStart && itemModel != null)
                    {
                        var item = itemModel.GetItem(objId);
                        if (item == null || item.Floor != floor) continue;

                        // 只在锚点格所在chunk绘制
                        if (item.AnchorX != cell.X || item.AnchorY != cell.Y) continue;

                        drawnItems.Add(objId);

                        var def = ConfigManager.GetItemDef(item.ItemDefId);
                        Color color = ConfigManager.ToUnityColor(def.Color);
                        if (!def.BlocksMovement)
                            color.a = 0.5f; // 非阻挡物品半透明

                        var uv = TileAtlasManager.GetItemUV(item.ItemDefId);
                        float rx = lx;
                        float ry = ly;
                        float padding = 0.05f;
                        AddRect(rx + padding, ry + padding,
                            rx + item.Width - padding, ry + item.Height - padding, color, uv);
                    }
                }
            });

            return CreateMesh("Object");
        }

        private Color GetMaterialColor(EMaterialType type)
        {
            switch (type)
            {
                case EMaterialType.Wood: return new Color(0.60f, 0.45f, 0.25f);
                case EMaterialType.Stone: return new Color(0.55f, 0.55f, 0.55f);
                case EMaterialType.Steel: return new Color(0.50f, 0.55f, 0.60f);
                default: return Color.gray;
            }
        }

        /// <summary>
        /// 构建蓝图层Mesh (半透明覆盖, 显示待建造/拆除指令)
        /// 蓝图层不使用贴图, 保持纯顶点色
        /// </summary>
        public Mesh BuildBlueprintMesh(ChunkData chunk, BlueprintDataModel blueprintModel, int floor)
        {
            ClearBuffers();

            if (blueprintModel == null) return null;

            var blueprints = blueprintModel.GetBlueprintsOnFloor(floor);

            foreach (var bp in blueprints)
            {
                if (bp.State == EBlueprintState.Complete || bp.State == EBlueprintState.Cancelled)
                    continue;

                // 检查蓝图是否在当前chunk范围内
                int localX = bp.X - chunk.WorldStartX;
                int localY = bp.Y - chunk.WorldStartY;
                if (localX < 0 || localX >= MapConst.ChunkSize ||
                    localY < 0 || localY >= MapConst.ChunkSize)
                    continue;

                // 根据蓝图类型决定颜色
                Color color;
                switch (bp.Type)
                {
                    case EBlueprintType.Demolish:
                    case EBlueprintType.Disassemble:
                    case EBlueprintType.DemolishFloor:
                    case EBlueprintType.DemolishRoof:
                        color = new Color(0.9f, 0.2f, 0.2f, 0.3f);
                        break;
                    case EBlueprintType.BuildFloor:
                        color = new Color(0.2f, 0.5f, 0.9f, 0.3f);
                        break;
                    case EBlueprintType.BuildRoof:
                        color = new Color(0.5f, 0.45f, 0.4f, 0.3f);
                        break;
                    case EBlueprintType.BuildFoundation:
                        color = new Color(0.6f, 0.45f, 0.3f, 0.3f);
                        break;
                    default:
                        color = new Color(0.2f, 0.9f, 0.3f, 0.3f);
                        break;
                }

                // 进行中的蓝图稍亮
                if (bp.State == EBlueprintState.InProgress)
                    color.a = 0.5f;

                switch (bp.Type)
                {
                    case EBlueprintType.BuildStructure:
                    {
                        // 结构蓝图: 整格覆盖
                        AddQuad(localX, localY, color);
                        break;
                    }

                    case EBlueprintType.BuildFurniture:
                    case EBlueprintType.Reinstall:
                    {
                        var def = ConfigManager.GetItemDef(bp.DefId);
                        int w = def.Width, h = def.Height;
                        if (bp.Rotation == 90 || bp.Rotation == 270)
                        {
                            w = def.Height;
                            h = def.Width;
                        }
                        float padding = 0.1f;
                        AddRect(localX + padding, localY + padding,
                            localX + w - padding, localY + h - padding, color);
                        break;
                    }

                    case EBlueprintType.Demolish:
                    case EBlueprintType.Disassemble:
                    case EBlueprintType.DemolishFloor:
                    case EBlueprintType.DemolishRoof:
                    case EBlueprintType.BuildFloor:
                    case EBlueprintType.BuildRoof:
                    case EBlueprintType.BuildFoundation:
                    {
                        // 整格覆盖
                        AddQuad(localX, localY, color);
                        break;
                    }
                }
            }

            return CreateMesh("Blueprint");
        }

        #region Autotile

        /// <summary>
        /// 计算 4-bit 邻接位掩码 (仅用于 Wall 类型)
        /// N=1, E=2, S=4, W=8
        /// 同类型 Wall 的邻居置位
        /// </summary>
        private int ComputeAutotileBitmask(int worldX, int worldY, int floor,
            int structureDefId, MapData mapData)
        {
            if (mapData == null) return 0;

            int mask = 0;
            // N: y+1
            if (HasSameWallType(worldX, worldY + 1, floor, structureDefId, mapData)) mask |= 1;
            // E: x+1
            if (HasSameWallType(worldX + 1, worldY, floor, structureDefId, mapData)) mask |= 2;
            // S: y-1
            if (HasSameWallType(worldX, worldY - 1, floor, structureDefId, mapData)) mask |= 4;
            // W: x-1
            if (HasSameWallType(worldX - 1, worldY, floor, structureDefId, mapData)) mask |= 8;
            return mask;
        }

        /// <summary>
        /// 检查邻接格是否有相同类型的 Wall
        /// 允许不同材质的墙互相连接 (只要都是 Wall 类型)
        /// </summary>
        private bool HasSameWallType(int worldX, int worldY, int floor,
            int structureDefId, MapData mapData)
        {
            if (!mapData.IsValidCellPos(worldX, worldY, floor)) return false;

            var cell = mapData.GetCell(worldX, worldY, floor);
            if (cell == null || !cell.HasStructure) return false;

            var neighborDef = ConfigManager.GetStructureDef(cell.StructureDefId);
            return (EStructureType)neighborDef.StructureType == EStructureType.Wall;
        }

        #endregion

        #region 内部方法

        private void ClearBuffers()
        {
            _vertices.Clear();
            _triangles.Clear();
            _uvs.Clear();
            _colors.Clear();
        }

        /// <summary>
        /// 添加1x1的quad (使用 Atlas UV)
        /// </summary>
        private void AddQuad(float x, float y, Color color, Rect uvRect)
        {
            AddRect(x, y, x + 1f, y + 1f, color, uvRect);
        }

        /// <summary>
        /// 添加1x1的quad (使用 FallbackUV, 蓝图层等纯色场景)
        /// </summary>
        private void AddQuad(float x, float y, Color color)
        {
            AddRect(x, y, x + 1f, y + 1f, color, TileAtlasManager.FallbackUV);
        }

        /// <summary>
        /// 添加任意矩形quad (使用 Atlas UV)
        /// </summary>
        private void AddRect(float x0, float y0, float x1, float y1, Color color, Rect uvRect)
        {
            int idx = _vertices.Count;

            _vertices.Add(new Vector3(x0, y0, 0));
            _vertices.Add(new Vector3(x1, y0, 0));
            _vertices.Add(new Vector3(x1, y1, 0));
            _vertices.Add(new Vector3(x0, y1, 0));

            _triangles.Add(idx);
            _triangles.Add(idx + 2);
            _triangles.Add(idx + 1);
            _triangles.Add(idx);
            _triangles.Add(idx + 3);
            _triangles.Add(idx + 2);

            _uvs.Add(new Vector2(uvRect.xMin, uvRect.yMin));
            _uvs.Add(new Vector2(uvRect.xMax, uvRect.yMin));
            _uvs.Add(new Vector2(uvRect.xMax, uvRect.yMax));
            _uvs.Add(new Vector2(uvRect.xMin, uvRect.yMax));

            _colors.Add(color);
            _colors.Add(color);
            _colors.Add(color);
            _colors.Add(color);
        }

        /// <summary>
        /// 添加任意矩形quad (使用 FallbackUV)
        /// </summary>
        private void AddRect(float x0, float y0, float x1, float y1, Color color)
        {
            AddRect(x0, y0, x1, y1, color, TileAtlasManager.FallbackUV);
        }

        private Mesh CreateMesh(string name)
        {
            if (_vertices.Count == 0) return null;

            var mesh = new Mesh
            {
                name = name
            };
            mesh.SetVertices(_vertices);
            mesh.SetTriangles(_triangles, 0);
            mesh.SetUVs(0, _uvs);
            mesh.SetColors(_colors);
            mesh.RecalculateBounds();
            return mesh;
        }

        private Color GetTerrainColor(int terrainDefId)
        {
            return ConfigManager.ToUnityColor(ConfigManager.GetTerrainDef(terrainDefId).Color);
        }

        private Color GetFloorColor(int floorDefId)
        {
            return ConfigManager.ToUnityColor(ConfigManager.GetFloorDef(floorDefId).Color);
        }

        #endregion
    }
}
