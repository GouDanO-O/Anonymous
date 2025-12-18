using Core.Game;
using GDFramework.Input;
using GDFramework.LubanKit;
using GDFramework.Models;
using GDFramework.Multilingual;
using GDFramework.Resource;
using GDFramework.Scene;
using GDFramework.SDK;
using GDFramework.Utility;
using GDFramework.View;
using GDFramework.YooAssetKit;
using GDFrameworkExtend.StorageKit;

namespace GDFrameworkCore
{
    public class Main : Architecture<GameMain>
    {
        /// <summary>
        /// 初始化
        /// </summary>
        protected override void Init()
        {
            Register_Utility();
            Register_Model();
            Register_System();

            LoadFrameSettingUtility();
        }
    
        /// <summary>
        /// 注册System
        /// </summary>
        protected virtual void Register_System()
        {
            this.RegisterSystem(new ResourcesManager());
            this.RegisterSystem(new SceneLoaderKit());
            this.RegisterSystem(new MultilingualManager());
            this.RegisterSystem(new ViewManager());
            this.RegisterSystem(new SdkManager());
            this.RegisterSystem(new YooAssetManager());
            this.RegisterSystem(new LubanKit());
            this.RegisterSystem(new StorageKit());
            this.RegisterSystem(new NewInputManager());
        }
    
        /// <summary>
        /// 注册Model
        /// </summary>
        protected virtual void Register_Model()
        {
            this.RegisterModel(new MultilingualDataModel());
            this.RegisterModel(new CheatDataModel());
            this.RegisterModel(new GameDataModel());
        }
    
        /// <summary>
        /// 注册Utility
        /// </summary>
        protected virtual void Register_Utility()
        {
            this.RegisterUtility(new ResourcesUtility());
            this.RegisterUtility(new MultilingualUtility());
            this.RegisterUtility(new SdkUtility());
        }

        /// <summary>
        /// 根据框架设置来添加模块
        /// </summary>
        protected virtual void LoadFrameSettingUtility()
        {
            FrameManager frameManager = FrameManager.Instance;
            if (frameManager.WillShowCheatWindow)
            {
                this.RegisterUtility(frameManager.gameObject.AddComponent<CheatMonoUtility>());;
            }
            
            this.RegisterUtility(frameManager.gameObject.AddComponent<CoroutineMonoUtility>());;
            
            this.RegisterUtility(frameManager.gameObject.AddComponent<GUIUtility>());;
        }
    }
}
