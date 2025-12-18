using GDFramework.Cheater;
using GDFramework.Models;
using GDFrameworkCore;
using UnityEngine;

namespace GDFramework.Utility
{
    public class CheatMonoUtility : BasicToolMonoUtility, ICanGetModel
    {
        private CheatDataModel _cheatDataModel;

        // GUI布局滚动条
        private Vector2 scrollPosition;

        protected override void InitUtility()
        {
            _cheatDataModel = GetArchitecture().GetModel<CheatDataModel>();
        }

        protected override void DrawGUI()
        {
            if (!isShowing)
                return;

            GUILayout.BeginArea(new Rect(10, 40, 300, 400), GUI.skin.box);
            GUILayout.Label("作弊系统", GUI.skin.label);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));

            var cheaters = _cheatDataModel.GetCheaterDatas();
            foreach (var cheat in cheaters)
            {
                GUILayout.BeginHorizontal();

                var thisCommand = cheat.Value;

                // 显示作弊功能的名称和描述
                GUILayout.Label(cheat.Key, GUILayout.Width(200));

                if (thisCommand is AddFreeVideoCheatCommand)
                {
                    var newCommand = (AddFreeVideoCheatCommand)thisCommand;
                    var isActivite = newCommand.IsActive ? "关闭" : "开启";
                    // 按钮执行作弊功能
                    if (GUILayout.Button(isActivite)) 
                        newCommand.ExecuteCommand();
                }
                else
                {
                    // 按钮执行作弊功能
                    if (GUILayout.Button("执行")) 
                        cheat.Value.ExecuteCommand();
                }


                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        public IArchitecture GetArchitecture()
        {
            return Main.Interface;
        }
    }
}