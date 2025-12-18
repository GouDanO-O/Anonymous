using GDFrameworkCore;
using UnityEngine.InputSystem;

namespace GDFramework.Models
{
    public class GameDataModel : AbstractModel
    {
        public InputActionAsset InputActionAsset { get; set; } 
        
        public bool isPausing { get; protected set; }

        public bool isCheatMode { get; protected set; }

        /// <summary>
        /// 改变暂停状态
        /// </summary>
        /// <param name="willPaused"></param>
        public void ChangGamePasuing(bool willPaused)
        {
            isPausing = willPaused;
        }

        /// <summary>
        /// 改变作弊模式
        /// </summary>
        /// <param name="isCheatMode"></param>
        public void ChangeCheatMode(bool isCheatMode)
        {
            this.isCheatMode = isCheatMode;
        }

        protected override void OnInit()
        {
            isPausing = false;
        }
    }
}