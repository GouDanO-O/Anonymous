using System.Collections.Generic;
using Core.Game.Map.Data;
using Core.Game.Map.Define;
using Core.Game.Map.Utility;
using UnityEngine;

namespace Core.Game.Map.View
{
    /// <summary>
    /// Chunk Mesh 构建器
    /// 将 Chunk 数据转换为可渲染的 Mesh
    /// </summary>
    public class ChunkMeshBuilder
    {
        #region 缓存

        private List<Vector3> _vertices = new List<Vector3>();
        private List<int> _triangles = new List<int>();
        private List<Vector2> _uvs = new List<Vector2>();
        private List<Color> _colors = new List<Color>();

        #endregion

        #region 颜色定义（占位用）

        // 地面颜色
        private static readonly Color ColorGrass = new Color(0.3f, 0.7f, 0.3f, 1f);
        private static readonly Color ColorDirt = new Color(0.6f, 0.4f, 0.2f, 1f);
        private static readonly Color ColorSand = new Color(0.9f, 0.8f, 0.5f, 1f);
        private static readonly Color ColorStone = new Color(0.5f, 0.5f, 0.5f, 1f);
        private static readonly Color ColorWater = new Color(0.2f, 0.4f, 0.8f, 0.8f);

        // 地板颜色
        private static readonly Color ColorWoodFloor = new Color(0.7f, 0.5f, 0.3f, 1f);
        private static readonly Color ColorTileFloor = new Color(0.8f, 0.8f, 0.8f, 1f);
        private static readonly Color ColorConcreteFloor = new Color(0.6f, 0.6f, 0.6f, 1f);
        private static readonly Color ColorCarpetFloor = new Color(0.5f, 0.2f, 0.2f, 1f);

        // 墙体颜色
        private static readonly Color ColorWallNorth = new Color(0.2f, 0.4f, 0.8f, 1f); // 蓝色
        private static readonly Color ColorWallWest = new Color(0.8f, 0.2f, 0.2f, 1f); // 红色
        private static readonly Color ColorWallDoor = new Color(0.6f, 0.4f, 0.2f, 1f); // 棕色（门）

        // 屋顶颜色
        private static readonly Color ColorRoof = new Color(0.4f, 0.3f, 0.25f, 1f); // 深棕色
        private static readonly Color ColorRoofEdge = new Color(0.3f, 0.2f, 0.15f, 1f); // 更深的边缘色

        #endregion

        /// <summary>
        /// 构建地面层 Mesh
        /// </summary>
        public Mesh BuildGroundMesh(ChunkData chunk)
        {
            ClearBuffers();

            for (int ly = 0; ly < MapDefine.ChunkHeight; ly++)
            {
                for (int lx = 0; lx < MapDefine.ChunkWidth; lx++)
                {
                    var cell = chunk.GetCell(lx, ly);
                    if (cell == null || cell.GroundType == EGroundType.None)
                        continue;

                    Color color = GetGroundColor(cell.GroundType);
                    AddTileQuad(cell.X, cell.Y, chunk.Floor, color);
                }
            }

            return CreateMesh("Ground");
        }

        /// <summary>
        /// 构建地板层 Mesh
        /// </summary>
        public Mesh BuildFloorMesh(ChunkData chunk)
        {
            ClearBuffers();

            for (int ly = 0; ly < MapDefine.ChunkHeight; ly++)
            {
                for (int lx = 0; lx < MapDefine.ChunkWidth; lx++)
                {
                    var cell = chunk.GetCell(lx, ly);
                    if (cell == null || cell.FloorType == EFloorType.None)
                        continue;

                    Color color = GetFloorColor(cell.FloorType);
                    // 地板略微抬高，避免 Z-fighting
                    AddTileQuad(cell.X, cell.Y, chunk.Floor, color, 0.001f);
                }
            }

            return CreateMesh("Floor");
        }

        /// <summary>
        /// 构建墙体层 Mesh
        /// </summary>
        public Mesh BuildWallMesh(ChunkData chunk)
        {
            ClearBuffers();

            for (int ly = 0; ly < MapDefine.ChunkHeight; ly++)
            {
                for (int lx = 0; lx < MapDefine.ChunkWidth; lx++)
                {
                    var cell = chunk.GetCell(lx, ly);
                    if (cell == null)
                        continue;

                    // 北墙
                    if (cell.HasWallNorth)
                    {
                        Color color = cell.WallNorth.HasDoor ? ColorWallDoor : ColorWallNorth;
                        AddNorthWallQuad(cell.X, cell.Y, chunk.Floor, color);
                    }

                    // 西墙
                    if (cell.HasWallWest)
                    {
                        Color color = cell.WallWest.HasDoor ? ColorWallDoor : ColorWallWest;
                        AddWestWallQuad(cell.X, cell.Y, chunk.Floor, color);
                    }
                }
            }

            return CreateMesh("Wall");
        }

        /// <summary>
        /// 构建屋顶层 Mesh
        /// </summary>
        public Mesh BuildRoofMesh(ChunkData chunk)
        {
            ClearBuffers();

            for (int ly = 0; ly < MapDefine.ChunkHeight; ly++)
            {
                for (int lx = 0; lx < MapDefine.ChunkWidth; lx++)
                {
                    var cell = chunk.GetCell(lx, ly);
                    if (cell == null)
                        continue;

                    // 只渲染有屋顶标记的格子
                    if (!cell.HasFlag(ECellFlags.HasRoof))
                        continue;

                    // 屋顶位于墙顶部
                    AddRoofQuad(cell.X, cell.Y, chunk.Floor, ColorRoof);
                }
            }

            return CreateMesh("Roof");
        }

        #region 私有方法

        private void ClearBuffers()
        {
            _vertices.Clear();
            _triangles.Clear();
            _uvs.Clear();
            _colors.Clear();
        }

        private Mesh CreateMesh(string name)
        {
            if (_vertices.Count == 0)
                return null;

            var mesh = new Mesh();
            mesh.name = name;
            mesh.SetVertices(_vertices);
            mesh.SetTriangles(_triangles, 0);
            mesh.SetUVs(0, _uvs);
            mesh.SetColors(_colors);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        /// <summary>
        /// 添加一个菱形 Tile 的四边形
        /// </summary>
        private void AddTileQuad(int cellX, int cellY, int floor, Color color, float heightOffset = 0f)
        {
            Vector3 center = IsometricUtility.CellCenterToScreen(cellX, cellY, floor);
            center.y += heightOffset;

            float halfW = MapDefine.TileWorldWidth * 0.5f;
            float halfH = MapDefine.TileWorldHeight * 0.5f;

            // 菱形四个顶点：左、上、右、下
            Vector3 left = center + new Vector3(-halfW, 0, 0);
            Vector3 top = center + new Vector3(0, halfH, 0);
            Vector3 right = center + new Vector3(halfW, 0, 0);
            Vector3 bottom = center + new Vector3(0, -halfH, 0);

            // 计算深度（Z 坐标）
            float depth = IsometricUtility.CalculateDepth(cellX, cellY, floor, 0);
            left.z = depth;
            top.z = depth;
            right.z = depth;
            bottom.z = depth;

            int startIndex = _vertices.Count;

            _vertices.Add(left);
            _vertices.Add(top);
            _vertices.Add(right);
            _vertices.Add(bottom);

            // 两个三角形组成菱形
            _triangles.Add(startIndex);
            _triangles.Add(startIndex + 1);
            _triangles.Add(startIndex + 2);

            _triangles.Add(startIndex);
            _triangles.Add(startIndex + 2);
            _triangles.Add(startIndex + 3);

            // UV（简单映射）
            _uvs.Add(new Vector2(0, 0.5f));
            _uvs.Add(new Vector2(0.5f, 1));
            _uvs.Add(new Vector2(1, 0.5f));
            _uvs.Add(new Vector2(0.5f, 0));

            // 颜色
            _colors.Add(color);
            _colors.Add(color);
            _colors.Add(color);
            _colors.Add(color);
        }

        /// <summary>
        /// 添加北墙四边形
        /// </summary>
        private void AddNorthWallQuad(int cellX, int cellY, int floor, Color color)
        {
            Vector3 cellPos = IsometricUtility.CellToScreen(cellX, cellY, floor);
            float halfW = MapDefine.TileWorldWidth * 0.5f;
            float halfH = MapDefine.TileWorldHeight * 0.5f;
            float wallH = MapDefine.WallWorldHeight;

            // 北墙连接格子的"左顶点"和"上顶点"
            Vector3 bottomLeft = cellPos + new Vector3(-halfW, 0, 0);
            Vector3 bottomRight = cellPos + new Vector3(0, halfH, 0);
            Vector3 topLeft = bottomLeft + new Vector3(0, wallH, 0);
            Vector3 topRight = bottomRight + new Vector3(0, wallH, 0);

            // 深度（墙比地面深度大一点）
            float depth = IsometricUtility.CalculateDepth(cellX, cellY, floor, 2);
            bottomLeft.z = depth;
            bottomRight.z = depth;
            topLeft.z = depth;
            topRight.z = depth;

            AddQuad(bottomLeft, bottomRight, topRight, topLeft, color);
        }

        /// <summary>
        /// 添加西墙四边形
        /// </summary>
        private void AddWestWallQuad(int cellX, int cellY, int floor, Color color)
        {
            Vector3 cellPos = IsometricUtility.CellToScreen(cellX, cellY, floor);
            float halfW = MapDefine.TileWorldWidth * 0.5f;
            float halfH = MapDefine.TileWorldHeight * 0.5f;
            float wallH = MapDefine.WallWorldHeight;

            // 西墙连接格子的"下顶点"和"左顶点"
            Vector3 bottomLeft = cellPos + new Vector3(0, -halfH, 0);
            Vector3 bottomRight = cellPos + new Vector3(-halfW, 0, 0);
            Vector3 topLeft = bottomLeft + new Vector3(0, wallH, 0);
            Vector3 topRight = bottomRight + new Vector3(0, wallH, 0);

            // 深度
            float depth = IsometricUtility.CalculateDepth(cellX, cellY, floor, 2);
            bottomLeft.z = depth;
            bottomRight.z = depth;
            topLeft.z = depth;
            topRight.z = depth;

            AddQuad(bottomLeft, bottomRight, topRight, topLeft, color);
        }

        /// <summary>
        /// 添加屋顶四边形
        /// 屋顶位于墙体顶部，是一个菱形
        /// </summary>
        private void AddRoofQuad(int cellX, int cellY, int floor, Color color)
        {
            Vector3 cellPos = IsometricUtility.CellToScreen(cellX, cellY, floor);
            float halfW = MapDefine.TileWorldWidth * 0.5f;
            float halfH = MapDefine.TileWorldHeight * 0.5f;
            float wallH = MapDefine.WallWorldHeight;

            // 屋顶在墙体顶部，形成菱形
            Vector3 center = cellPos + new Vector3(0, wallH, 0);

            Vector3 left = center + new Vector3(-halfW, 0, 0);
            Vector3 top = center + new Vector3(0, halfH, 0);
            Vector3 right = center + new Vector3(halfW, 0, 0);
            Vector3 bottom = center + new Vector3(0, -halfH, 0);

            // 深度（屋顶在最上层）
            float depth = IsometricUtility.CalculateDepth(cellX, cellY, floor, 4);
            left.z = depth;
            top.z = depth;
            right.z = depth;
            bottom.z = depth;

            int startIndex = _vertices.Count;

            _vertices.Add(left);
            _vertices.Add(top);
            _vertices.Add(right);
            _vertices.Add(bottom);

            // 两个三角形组成菱形
            _triangles.Add(startIndex);
            _triangles.Add(startIndex + 1);
            _triangles.Add(startIndex + 2);

            _triangles.Add(startIndex);
            _triangles.Add(startIndex + 2);
            _triangles.Add(startIndex + 3);

            // UV
            _uvs.Add(new Vector2(0, 0.5f));
            _uvs.Add(new Vector2(0.5f, 1));
            _uvs.Add(new Vector2(1, 0.5f));
            _uvs.Add(new Vector2(0.5f, 0));

            // 颜色
            _colors.Add(color);
            _colors.Add(color);
            _colors.Add(color);
            _colors.Add(color);
        }

        /// <summary>
        /// 添加通用四边形
        /// </summary>
        private void AddQuad(Vector3 bl, Vector3 br, Vector3 tr, Vector3 tl, Color color)
        {
            int startIndex = _vertices.Count;

            _vertices.Add(bl);
            _vertices.Add(br);
            _vertices.Add(tr);
            _vertices.Add(tl);

            _triangles.Add(startIndex);
            _triangles.Add(startIndex + 2);
            _triangles.Add(startIndex + 1);

            _triangles.Add(startIndex);
            _triangles.Add(startIndex + 3);
            _triangles.Add(startIndex + 2);

            _uvs.Add(new Vector2(0, 0));
            _uvs.Add(new Vector2(1, 0));
            _uvs.Add(new Vector2(1, 1));
            _uvs.Add(new Vector2(0, 1));

            _colors.Add(color);
            _colors.Add(color);
            _colors.Add(color);
            _colors.Add(color);
        }

        private Color GetGroundColor(EGroundType groundType)
        {
            return groundType switch
            {
                EGroundType.Grass => ColorGrass,
                EGroundType.Dirt => ColorDirt,
                EGroundType.Sand => ColorSand,
                EGroundType.Stone => ColorStone,
                EGroundType.Water => ColorWater,
                _ => Color.magenta
            };
        }

        private Color GetFloorColor(EFloorType floorType)
        {
            return floorType switch
            {
                EFloorType.Wood => ColorWoodFloor,
                EFloorType.Tile => ColorTileFloor,
                EFloorType.Concrete => ColorConcreteFloor,
                EFloorType.Carpet => ColorCarpetFloor,
                _ => Color.magenta
            };
        }

        #endregion
    }
}
