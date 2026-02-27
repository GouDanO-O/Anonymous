using System.Collections.Generic;
using Core.Game.Item.Define;
using Core.Game.Item.Model;
using Core.Game.Map.Data;
using Core.Game.Map.Define;
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
        /// 构建墙壁层Mesh
        /// 墙壁渲染为细长的quad (北墙在格子顶边, 西墙在格子左边)
        /// </summary>
        public Mesh BuildWallMesh(ChunkData chunk, int mapWidth, int mapHeight)
        {
            ClearBuffers();
            float wallThickness = 0.2f;

            chunk.ForEachCell((cell, lx, ly) =>
            {
                // 北墙: 格子顶边的水平线段
                if (cell.WallNorth.HasWall)
                {
                    float x0 = lx;
                    float y0 = ly + 1f - wallThickness * 0.5f;
                    float x1 = lx + 1f;
                    float y1 = ly + 1f + wallThickness * 0.5f;
                    AddRect(x0, y0, x1, y1, GetWallColor(cell.WallNorth));
                }

                // 西墙: 格子左边的垂直线段
                if (cell.WallWest.HasWall)
                {
                    float x0 = lx - wallThickness * 0.5f;
                    float y0 = ly;
                    float x1 = lx + wallThickness * 0.5f;
                    float y1 = ly + 1f;
                    AddRect(x0, y0, x1, y1, GetWallColor(cell.WallWest));
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
        public Mesh BuildObjectMesh(ChunkData chunk, ItemDataModel itemModel, int floor)
        {
            ClearBuffers();

            if (itemModel == null) return null;

            // 记录已绘制的物品ID, 避免多格物品重复绘制
            var drawnItems = new HashSet<long>();

            chunk.ForEachCell((cell, lx, ly) =>
            {
                if (cell.ObjectIds == null) return;

                foreach (long objId in cell.ObjectIds)
                {
                    // 跳过非物品ID范围 (Pawn等)
                    if (objId < ItemConst.ItemIdStart) continue;
                    if (drawnItems.Contains(objId)) continue;

                    var item = itemModel.GetItem(objId);
                    if (item == null || item.Floor != floor) continue;

                    // 只在锚点格所在chunk绘制
                    if (item.AnchorX != cell.X || item.AnchorY != cell.Y) continue;

                    drawnItems.Add(objId);

                    var def = TempConfigProvider.GetItemDef(item.ItemDefId);
                    Color color = def.Color;
                    if (!def.BlocksMovement)
                        color.a = 0.5f; // 非阻挡物品半透明

                    // 绘制矩形 (相对于chunk局部坐标)
                    float rx = lx;
                    float ry = ly;

                    // 内缩一小圈, 和地板区分
                    float padding = 0.05f;
                    AddRect(rx + padding, ry + padding,
                        rx + item.Width - padding, ry + item.Height - padding, color);
                }
            });

            return CreateMesh("Object");
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

        private Color GetWallColor(WallData wall)
        {
            var wallDef = TempConfigProvider.GetWallDef(wall.WallDefId);
            if (wall.HasDoor) return wallDef.DoorColor;
            if (wall.HasWindow) return wallDef.WindowColor;
            return wallDef.Color;
        }

        #endregion
    }
}
