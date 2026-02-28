using System.Collections.Generic;
using Core.Game.Blueprint.Data;
using Core.Game.Blueprint.Define;
using Core.Game.Blueprint.Model;
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
                AddQuad(lx, ly, GetTerrainColor(cell.TerrainDefId));
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
                AddQuad(lx, ly, GetFloorColor(cell.FloorDefId));
            });

            return CreateMesh("Floor");
        }

        /// <summary>
        /// 构建结构层Mesh (墙/门/窗 — 独占整格)
        /// </summary>
        public Mesh BuildWallMesh(ChunkData chunk, int mapWidth, int mapHeight)
        {
            ClearBuffers();

            chunk.ForEachCell((cell, lx, ly) =>
            {
                if (!cell.HasStructure) return;

                var def = TempConfigProvider.GetStructureDef(cell.StructureDefId);
                Color color = def.Color;

                if (def.StructureType == EStructureType.Door)
                {
                    // 门: 稍小的矩形以示区分
                    float inset = 0.15f;
                    AddRect(lx + inset, ly + inset, lx + 1f - inset, ly + 1f - inset, color);
                }
                else if (def.StructureType == EStructureType.Window)
                {
                    // 窗: 略小矩形 + 半透明
                    float inset = 0.1f;
                    color.a = 0.7f;
                    AddRect(lx + inset, ly + inset, lx + 1f - inset, ly + 1f - inset, color);
                }
                else
                {
                    // 墙: 完整1x1色块
                    AddQuad(lx, ly, color);
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

            chunk.ForEachCell((cell, lx, ly) =>
            {
                if (!cell.HasRoof) return;
                AddQuad(lx, ly, new Color(0.4f, 0.35f, 0.3f, 1f));
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

                        var def = TempConfigProvider.GetItemDef(item.ItemDefId);
                        Color color = def.Color;
                        if (!def.BlocksMovement)
                            color.a = 0.5f; // 非阻挡物品半透明

                        float rx = lx;
                        float ry = ly;
                        float padding = 0.05f;
                        AddRect(rx + padding, ry + padding,
                            rx + item.Width - padding, ry + item.Height - padding, color);
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

                // 建造类: 绿色半透明; 拆除/拆卸类: 红色半透明
                bool isDemolish = bp.Type == EBlueprintType.Demolish ||
                                  bp.Type == EBlueprintType.Disassemble;
                Color color = isDemolish
                    ? new Color(0.9f, 0.2f, 0.2f, 0.3f)
                    : new Color(0.2f, 0.9f, 0.3f, 0.3f);

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
                        var def = TempConfigProvider.GetItemDef(bp.DefId);
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
                    {
                        // 拆除/拆卸: 整格覆盖
                        AddQuad(localX, localY, color);
                        break;
                    }
                }
            }

            return CreateMesh("Blueprint");
        }

        #region 内部方法

        private void ClearBuffers()
        {
            _vertices.Clear();
            _triangles.Clear();
            _uvs.Clear();
            _colors.Clear();
        }

        /// <summary>
        /// 添加1x1的quad
        /// </summary>
        private void AddQuad(float x, float y, Color color)
        {
            AddRect(x, y, x + 1f, y + 1f, color);
        }

        /// <summary>
        /// 添加任意矩形quad
        /// </summary>
        private void AddRect(float x0, float y0, float x1, float y1, Color color)
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

            _uvs.Add(new Vector2(0, 0));
            _uvs.Add(new Vector2(1, 0));
            _uvs.Add(new Vector2(1, 1));
            _uvs.Add(new Vector2(0, 1));

            _colors.Add(color);
            _colors.Add(color);
            _colors.Add(color);
            _colors.Add(color);
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
            return TempConfigProvider.GetTerrainDef(terrainDefId).Color;
        }

        private Color GetFloorColor(int floorDefId)
        {
            return TempConfigProvider.GetFloorDef(floorDefId).Color;
        }

        #endregion
    }
}
