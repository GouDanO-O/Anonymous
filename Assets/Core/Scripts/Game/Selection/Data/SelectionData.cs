using Core.Game.Selection.Define;

namespace Core.Game.Selection.Data
{
    /// <summary>
    /// 当前选择状态
    /// </summary>
    public class SelectionData
    {
        public ESelectionType Type;
        public int CellX;
        public int CellY;
        public int Floor;

        /// <summary>
        /// 选中的Pawn或Item的ID (Type=Pawn/Item时有效)
        /// </summary>
        public long TargetId;

        public void Clear()
        {
            Type = ESelectionType.None;
            CellX = -1;
            CellY = -1;
            Floor = -1;
            TargetId = 0;
        }

        public void SetCell(int x, int y, int floor)
        {
            Type = ESelectionType.Cell;
            CellX = x;
            CellY = y;
            Floor = floor;
            TargetId = 0;
        }

        public void SetPawn(long pawnId, int x, int y, int floor)
        {
            Type = ESelectionType.Pawn;
            CellX = x;
            CellY = y;
            Floor = floor;
            TargetId = pawnId;
        }

        public void SetItem(long itemId, int x, int y, int floor)
        {
            Type = ESelectionType.Item;
            CellX = x;
            CellY = y;
            Floor = floor;
            TargetId = itemId;
        }
    }
}
