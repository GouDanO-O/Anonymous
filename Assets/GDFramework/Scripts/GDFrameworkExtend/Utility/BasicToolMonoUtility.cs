using GDFrameworkCore;
using UnityEngine;

namespace GDFramework.Utility
{
    public abstract class BasicToolMonoUtility : MonoBehaviour, IUtility
    {
        [HideInInspector] public bool isShowing;

        // Start is called before the first frame update
        private void Awake()
        {
            InitUtility();
        }

        private void OnDestroy()
        {
            DeInitUtility();
        }

        protected abstract void InitUtility();

        private void OnGUI()
        {
            DrawGUI();
        }

        protected virtual void DrawGUI()
        {
        }

        public virtual void CheckButtonWillShow()
        {
            isShowing = !isShowing;
        }

        protected virtual void DeInitUtility()
        {
        }
    }
}