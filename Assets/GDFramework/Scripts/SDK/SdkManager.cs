using GDFramework.Models.Enums;
using GDFramework.Utility;
using GDFrameworkCore;


namespace GDFramework.SDK
{
    public class SdkManager : AbstractSystem
    {
        private EPlatform m_Platform = EPlatform.APK;

        private SdkUtility m_SdkUtility;

        protected override void OnInit()
        {
            m_SdkUtility = this.GetUtility<SdkUtility>();
            this.RegisterEvent<SOnShowADEvent>((data) => { m_SdkUtility.GetWillShowAdType(data); });
        }
    }
}