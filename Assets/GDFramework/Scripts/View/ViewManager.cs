
using Core.Game.View;
using GDFrameworkCore;
using GDFrameworkExtend.ActionKit;
using GDFrameworkExtend.UIKit;


namespace GDFramework.View
{
    public class ViewManager : AbstractSystem
    {
        protected override void OnInit()
        {
        }

        /// <summary>
        /// 进入菜单
        /// </summary>
        public void EnterMenu()
        {
            UIKit.OpenPanel<UI_GameMenuPanel>();
        }

        /// <summary>
        /// 进入游戏
        /// </summary>
        public void EnterGame()
        {
            
        }
    }
}