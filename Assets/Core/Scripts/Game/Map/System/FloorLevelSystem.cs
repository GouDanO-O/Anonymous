using Core.Game.Map.Event;
using Core.Game.Map.Model;
using GDFrameworkCore;

namespace Core.Game.Map.System
{
    /// <summary>
    /// 楼层切换逻辑系统
    /// 楼层N显示规则: 0~N-1层全部可见, N层可见但隐藏屋顶, N+1~max层隐藏
    /// </summary>
    public class FloorLevelSystem : AbstractSystem
    {
        private MapDataModel _mapDataModel;

        protected override void OnInit()
        {
            _mapDataModel = this.GetModel<MapDataModel>();
            this.RegisterEvent<SFloorChangedEvent>(OnFloorChanged);
        }

        private void OnFloorChanged(SFloorChangedEvent evt)
        {
            this.SendEvent(new SFloorVisibilityChangedEvent(evt.NewFloor));
        }

        /// <summary>
        /// 指定楼层是否应该可见
        /// </summary>
        public bool IsFloorVisible(int floor)
        {
            return floor <= _mapDataModel.CurrentFloor;
        }

        /// <summary>
        /// 指定楼层的屋顶是否应该显示
        /// 只有低于当前查看楼层的屋顶才显示
        /// </summary>
        public bool ShouldShowRoof(int floor)
        {
            return floor < _mapDataModel.CurrentFloor;
        }

        /// <summary>
        /// 指定楼层是否是当前查看楼层
        /// </summary>
        public bool IsCurrentFloor(int floor)
        {
            return floor == _mapDataModel.CurrentFloor;
        }
    }
}
