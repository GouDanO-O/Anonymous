using System.Collections.Generic;
using Core.Game.Map.Data;
using Core.Game.Map.Define;
using Core.Game.Map.Utility;
using UnityEngine;

namespace Core.Game.Map.View
{
    /// <summary>
    /// Chunk Mesh构建器
    /// 将Chunk数据转换为可渲染的Mesh
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
        private static readonly Color ColorSnow = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color ColorSwamp = new Color(0.3f, 0.4f, 0.2f, 1f);

        // 地板颜色
        private static readonly Color ColorWoodFloor = new Color(0.7f, 0.5f, 0.3f, 1f);
        private static readonly Color ColorTileFloor = new Color(0.8f, 0.8f, 0.8f, 1f);
        private static readonly Color ColorConcreteFloor = new Color(0.6f, 0.6f, 0.6f, 1f);
        private static readonly Color ColorCarpetFloor = new Color(0.5f, 0.2f, 0.2f, 1f);
        private static readonly Color ColorMetalFloor = new Color(0.7f, 0.7f, 0.75f, 1f);
        private static readonly Color ColorStoneSlabFloor = new Color(0.55f, 0.55f, 0.5f, 1f);

        // 墙体颜色
        private static readonly Color ColorWallWood = new Color(0.6f, 0.45f, 0.3f, 1f);
        private static readonly Color ColorWallStone = new Color(0.5f, 0.5f, 0.5f, 1f);
        private static readonly Color ColorWallBrick = new Color(0.7f, 0.35f, 0.25f, 1f);
        private static readonly Color ColorWallMetal = new Color(0.6f, 0.6f, 0.65f, 1f);
        private static readonly Color ColorWallGlass = new Color(0.7f, 0.85f, 0.9f, 0.6f);
        private static readonly Color ColorWallDoor = new Color(0.5f, 0.35f, 0.2f, 1f);

        // 屋顶颜色
        private static readonly Color ColorRoof = new Color(0.4f, 0.3f, 0.25f, 1f);

        #endregion

        #region 公共方法

        /// <summary>
        /// 构建地面层Mesh
        /// </summary>
        public Mesh BuildGroundMesh(ChunkData chunk)
        {
            ClearBuffers();

            chunk.ForEachCell((cell, lx, ly) =>
            {
                if (cell.GroundType == EGroundType.None)
                    return;

                Color color = GetGroundColor(cell.GroundType);
                AddTileQuad(cell.X, cell.Y, chunk.Floor, color, MapDefine.DepthOffsetGround);
            });

            return CreateMesh("Ground");
        }

        /// <summary>
        /// 构建地板层Mesh
        /// </summary>
        public Mesh BuildFloorMesh(ChunkData chunk)
        {
            ClearBuffers();

            chunk.ForEachCell((cell, lx, ly) =>
            {
                if (cell.FloorType == EFloorType.None)
                    return;

                Color color = GetFloorColor(cell.FloorType);
                // 地板略微抬高，避免Z-fighting
                AddTileQuad(cell.X, cell.Y, chunk.Floor, color, MapDefine.DepthOffsetFloor, 0.001f);
            });

            return CreateMesh("Floor");
        }

        /// <summary>
        /// 构建墙体层Mesh
        /// </summary>
        public Mesh BuildWallMesh(ChunkData chunk)
        {
            ClearBuffers();

            chunk.ForEachCell((cell, lx, ly) =>
            {
                // 北墙
                if (cell.HasWallNorth)
                {
                    Color color = GetWallColor(cell.WallNorth);
                    AddNorthWallQuad(cell.X, cell.Y, chunk.Floor, color);
                }

                // 西墙
                if (cell.HasWallWest)
                {
                    Color color = GetWallColor(cell.WallWest);
                    AddWestWallQuad(cell.X, cell.Y, chunk.Floor, color);
                }
            });

            return CreateMesh("Wall");
        }

        /// <summary>
        /// 构建屋顶层Mesh
        /// </summary>
        public Mesh BuildRoofMesh(ChunkData chunk)
        {
            ClearBuffers();

            chunk.ForEachCell((cell, lx, ly) =>
            {
                // 只渲染有屋顶标记的格子
                if (!cell.HasFlag(ECellFlags.HasRoof))
                    return;

                // 屋顶位于墙顶部
                AddRoofQuad(cell.X, cell.Y, chunk.Floor, ColorRoof);
            });

            return CreateMesh("Roof");
        }

        #endregion

        #region 私有方法 - 缓存管理

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

        #endregion

        #region 私有方法 - 几何构建

        /// <summary>
        /// 添加一个菱形Tile的四边形
        /// </summary>
        private void AddTileQuad(int cellX, int cellY, int floor, Color color, float depthOffset, float heightOffset = 0f)
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

            // 计算深度（Z坐标）
            float depth = IsometricUtility.CalculateDepth(cellX, cellY, floor, depthOffset);
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
            float depth = IsometricUtility.CalculateDepth(cellX, cellY, floor, MapDefine.DepthOffsetWall);
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
            float depth = IsometricUtility.CalculateDepth(cellX, cellY, floor, MapDefine.DepthOffsetWall + 0.01f);
            bottomLeft.z = depth;
            bottomRight.z = depth;
            topLeft.z = depth;
            topRight.z = depth;

            AddQuad(bottomLeft, bottomRight, topRight, topLeft, color);
        }

        /// <summary>
        /// 添加屋顶四边形
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
            float depth = IsometricUtility.CalculateDepth(cellX, cellY, floor, MapDefine.DepthOffsetRoof);
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

        #endregion

        #region 私有方法 - 颜色获取

        private Color GetGroundColor(EGroundType groundType)
        {
            return groundType switch
            {
                EGroundType.Grass => ColorGrass,
                EGroundType.Dirt => ColorDirt,
                EGroundType.Sand => ColorSand,
                EGroundType.Stone => ColorStone,
                EGroundType.Water => ColorWater,
                EGroundType.Snow => ColorSnow,
                EGroundType.Swamp => ColorSwamp,
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
                EFloorType.Metal => ColorMetalFloor,
                EFloorType.StoneSlab => ColorStoneSlabFloor,
                _ => Color.magenta
            };
        }

        private Color GetWallColor(WallData wall)
        {
            if (wall.HasDoor)
                return ColorWallDoor;

            return wall.WallType switch
            {
                EWallType.Wood => ColorWallWood,
                EWallType.Stone => ColorWallStone,
                EWallType.Brick => ColorWallBrick,
                EWallType.Metal => ColorWallMetal,
                EWallType.Glass => ColorWallGlass,
                _ => Color.magenta
            };
        }

        #endregion
    }
}
