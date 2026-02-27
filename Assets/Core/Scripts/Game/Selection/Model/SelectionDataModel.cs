using Core.Game.Selection.Data;
using Core.Game.Selection.Define;
using GDFrameworkCore;

namespace Core.Game.Selection.Model
{
    /// <summary>
    /// 选择状态数据模型
    /// </summary>
    public class SelectionDataModel : AbstractModel
    {
        private SelectionData _current = new();

        public SelectionData Current => _current;
        public ESelectionType CurrentType => _current.Type;
        public bool HasSelection => _current.Type != ESelectionType.None;

        protected override void OnInit()
        {
            _current.Clear();
        }

        public void SetSelection(SelectionData data)
        {
            _current.Type = data.Type;
            _current.CellX = data.CellX;
            _current.CellY = data.CellY;
            _current.Floor = data.Floor;
            _current.TargetId = data.TargetId;
        }

        public void ClearSelection()
        {
            _current.Clear();
        }
    }
}
