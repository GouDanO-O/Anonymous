using System;
using System.Collections.Generic;
using UnityEngine;

namespace GDFrameworkExtend.AStar
{
    public class Astar
    {
        public static Astar instance = new();

        private int mycount = 0;

        private BinaryHeap openList = new();

        private Dictionary<int, AsGrid> gridMap = new();

        private byte[][] mapdt;

        private int COL;

        private int ROW;

        private int EndX;

        private int EndY;

        private int limitR;

        private void open(int x, int y, int cost, AsGrid parent, bool create)
        {
            AsGrid grid;

            if (create)
            {
                grid = new AsGrid();
                grid.x = x;
                grid.y = y;
                grid.cost = cost;
                grid.parent = parent;
                grid.closed = false;
                grid.score = 10000;
                gridMap[(y << 16) + x] = grid;
                var dx = Mathf.Abs(x - EndX);
                var dy = Mathf.Abs(y - EndY);
                if (dx > 0 && dy > 0)
                    grid.last = 14;
                else
                    grid.last = 10;

                grid.score = grid.cost + grid.last;
                openList.push(grid);
            }
            else
            {
                grid = gridMap[(y << 16) + x];
                grid.parent = parent;
                ;
                grid.cost = cost;
                openList.updata(grid, grid.cost + grid.last);
            }

            mycount++;
        }

        private void check(int new_x, int new_y, AsGrid grid, int cost)
        {
            if (Mathf.Abs(new_x - EndX) > limitR || Mathf.Abs(new_y - EndY) > limitR)
                return;
            if (isBlock(new_x, new_y)) return;

            AsGrid newGrid = null;


            if (!gridMap.TryGetValue((new_y << 16) + new_x, out newGrid))
                open(new_x, new_y, grid.cost + cost, grid, true);
            else if (!newGrid.closed && newGrid.cost > grid.cost + cost)
                open(new_x, new_y, grid.cost + cost, grid, false);
        }

        private bool isBlock(int x, int y)
        {
            try
            {
                return y >= ROW || x >= COL || x < 0 || y < 0 || (mapdt[y][x] & 0x1) != 0;
            }
            catch (Exception e)
            {
                Debug.LogError(x + "," + y);
                return true;
            }
        }

        private List<AstarPosVo> getRst(AsGrid grid)
        {
            var return_dt = new List<AstarPosVo>();
            do
            {
                var pos = new AstarPosVo();
                pos.x = grid.x;
                pos.y = grid.y;
                grid = grid.parent;
                return_dt.Insert(0, pos);
            } while (grid != null);

            return return_dt.Count > 2 ? return_dt : null;
        }

        public List<AstarPosVo> find(byte[][] mapdata, int row, int col,
            int start_x, int start_y, int end_x, int end_y, int limitR)
        {
            mapdt = mapdata;
            this.limitR = limitR;

            if (start_x == end_x && start_y == end_y)
                return null;
            ROW = row;
            COL = col;
            EndX = end_x;
            EndY = end_y;
            if (isBlock(start_x, start_y)) return null;

            if (isBlock(end_x, end_y)) return null;

            List<AstarPosVo> rst = null;

            open(start_x, start_y, 0, null, true);

            while (true)
            {
                var len = openList.length;
                if (len == 0) break;

                var c_grid = openList.popMix();

                if (c_grid.x == end_x && c_grid.y == end_y)
                {
                    rst = getRst(c_grid);
                    break;
                }

                c_grid.closed = true;
                var x = c_grid.x;
                var y = c_grid.y;

                check(x, y - 1, c_grid, 10);
                check(x - 1, y, c_grid, 10);
                check(x + 1, y, c_grid, 10);
                check(x, y + 1, c_grid, 10);
                check(x - 1, y - 1, c_grid, 14);
                check(x + 1, y - 1, c_grid, 14);
                check(x - 1, y + 1, c_grid, 14);
                check(x + 1, y + 1, c_grid, 14);
            }

            gridMap.Clear();
            openList.clear();
            return rst;
        }
    }
}