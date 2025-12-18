using Core.Game;
using GDFrameworkCore;
using UnityEngine;

namespace GDFramework.Utility
{
    public class GUIUtility : BasicToolMonoUtility,ICanGetUtility
    {
        private bool willShowCheatWindow = GameManager.Instance.WillShowCheatWindow;
        
        private CheatMonoUtility _cheatMonoUtility => this.GetUtility<CheatMonoUtility>();
        
        protected override void InitUtility()
        {
            
        }

        protected override void DrawGUI()
        {
            base.DrawGUI();
            DrawUtilityWindow();
        }

        private void DrawUtilityWindow()
        {
            var showCount = -1;
            
            if (willShowCheatWindow)
            {
                showCount++;
                if (GUI.Button(new Rect(20 + showCount * 150, 0, 120, 30),
                        _cheatMonoUtility.isShowing ? "关闭作弊系统" : "打开作弊系统"))
                {
                    _cheatMonoUtility.CheckButtonWillShow();
                }
            }
        }

        public IArchitecture GetArchitecture()
        {
            return Main.Interface;
        }
    }
}