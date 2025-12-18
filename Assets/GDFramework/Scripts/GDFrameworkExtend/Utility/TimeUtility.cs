using GDFramework;
using GDFrameworkCore;
using UnityEngine;

namespace GDFramework.Utility
{
    public class TimeUtility : BasicToolUtility
    {
        public TimeUtility()
        {
            InitUtility();
        }
        
        protected override void InitUtility()
        {
            
        }
        
        /// <summary>
        /// 将秒转换成时分秒
        /// </summary>
        /// <param name="totalSeconds"></param>
        /// <returns></returns>
        public string ConvertToTimeFormat(float totalSeconds = 0)
        {
            var hours = Mathf.FloorToInt(totalSeconds / 3600);
            var minutes = Mathf.FloorToInt(totalSeconds % 3600 / 60);
            var seconds = Mathf.FloorToInt(totalSeconds % 60);

            return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }
    }
}