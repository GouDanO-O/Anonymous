using System;

namespace GDFramework.SDK
{
    /// <summary>
    /// 显示广告
    /// </summary>
    public struct SOnShowADEvent
    {
        public EADType willShowAdType;

        public string showAdDes;

        public Action successCallback;

        public Action failCallback;
    }
}