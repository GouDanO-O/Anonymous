using System;
using GDFramework.SDK;
using GDFramework.Utility;
using GDFrameworkExtend.LogKit;
using UnityEngine;

namespace GDFramework.SDK
{
    public class SdkUtility : BasicToolUtility
    {
        private Action succCallBack;

        private Action failCallBack;

        private bool isLogin;

        public bool isFreeVideoMod = false;

        public SdkUtility()
        {
            InitUtility();
        }
        
        protected override void InitUtility()
        {
            InitSDK();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void InitSDK()
        {
#if KuaiShou
            KS.Login((ret) =>
            {
                LogKit.Log("登录成功:" + ret.code);
                isLogin = true;
                InitRewardVideoAd();
            }, (code, msg) =>
            {
                LogKit.Log("登录失败:"+code+"---"+msg);
            });
#elif Vivo
#elif UNITY_ANDROID
            isLogin = true;
#else

#endif
        }

        public bool ChangeFreeVideoMod()
        {
            isFreeVideoMod = !isFreeVideoMod;
            LogKit.Log("切换免费广告:" + isFreeVideoMod);
            return isFreeVideoMod;
        }

        /// <summary>
        /// 展示广告
        /// </summary>
        /// <param name="adData"></param>
        public void GetWillShowAdType(SOnShowADEvent adData)
        {
            switch (adData.willShowAdType)
            {
                case EADType.Splash:
                    ShowSplash(adData.showAdDes);
                    break;
                case EADType.Native:
                    ShowNative(adData.showAdDes);
                    break;
                case EADType.BannerSmall:
                    ShowBanner(adData.showAdDes, 0);
                    break;
                case EADType.BannerBig:
                    ShowBanner(adData.showAdDes, 1);
                    break;
                case EADType.FloatIcon:
                    ShowFloatIcon(adData.showAdDes);
                    break;
                case EADType.InterstitialImg:
                    ShowInterstitial_Img(adData.showAdDes);
                    break;
                case EADType.InterstitialVideo:
                    ShowInterstitial_Video(adData.showAdDes, adData.successCallback, adData.failCallback);
                    break;
                case EADType.RewardVideo:
                    ShowRewardVideo(adData.showAdDes, adData.successCallback, adData.failCallback);
                    break;
            }
        }

#if KuaiShou
        static RewardVideoAd rewardVideoAd = null;
        
        const string AD_REWARD_VIDEO_AD_KEY = "2300014398_01";

        private void InitRewardVideoAd()
        {
            LogKit.Log("开始初始化激励视频");
            rewardVideoAd = KS.CreateRewardedVideoAd(AD_REWARD_VIDEO_AD_KEY);
            if (rewardVideoAd != null)
            {
                LogKit.Log("初始化激励视频--成功");
                rewardVideoAd.OnClose(new ADCloseResultCallBack((data) =>
                {
                    if (data.isEnded)
                    {
                        succCallBack?.Invoke();
                    }
                    else
                    {
                        failCallBack?.Invoke();
                    }
            
                    succCallBack = null;
                    failCallBack = null;
                    LogKit.Log("[激励广告] onClose : " + JsonUtility.ToJson(data));
                }));
                rewardVideoAd.OnError(new ADShowResultCallBack((data) =>
                {
                    if (data.code != 1)
                    {
                        failCallBack?.Invoke();
                    }

                    LogKit.Log("[激励广告] OnError : " + JsonUtility.ToJson(data));
                }));
            }
        }


#elif Vivo
#elif UNITY_ANDROID
#else

#endif

        /// <summary>
        /// 开屏广告
        /// </summary>
        /// <param name="text"></param>
        private void ShowSplash(string text = "")
        {
            if (!isLogin)
                return;
#if KuaiShou
#elif Vivo
#elif UNITY_ANDROID
            CallAd("插屏广告"+text,"ShowSplash");
#else
            CallAd("插屏广告" + text, "ShowSplash");
#endif
        }

        /// <summary>
        /// 悬浮ICON
        /// </summary>
        /// <param name="text"></param>
        private void ShowFloatIcon(string text = "")
        {
            if (!isLogin)
                return;
#if KuaiShou
#elif Vivo
#elif UNITY_ANDROID
            CallAd("悬浮ICON"+text,"ShowFloatIcon");
#else
            CallAd("悬浮ICON" + text, "ShowFloatIcon");
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        private void ShowNative(string text = "")
        {
            if (!isLogin)
                return;

#if KuaiShou
#elif Vivo
#elif UNITY_ANDROID
            CallAd("原生广告"+text,"ShowNative");
#else
            CallAd("原生广告" + text, "ShowNative");
#endif
        }

        /// <summary>
        /// Banner
        /// </summary>
        /// <param name="text"></param>
        /// <param name="bannerType">0=>小Banner,1=>大Banner</param>
        private void ShowBanner(string text = "", int bannerType = 0)
        {
            if (!isLogin)
                return;
#if KuaiShou
#elif Vivo
#elif UNITY_ANDROID
            if (bannerType == 0)
            {
                CallAd("Banner"+text,"ShowBannerSmall");
            }
            else if (bannerType == 1)
            {
                CallAd("Banner"+text,"ShowBannerBig");
            }
#else
            CallAd("Banner" + text, "ShowBanner");
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        private void ShowInterstitial_Img(string text = "")
        {
            if (!isLogin)
                return;

#if KuaiShou
#elif Vivo
#elif UNITY_ANDROID
            CallAd("插屏广告"+text,"ShowInterstitial_Img");
#else
            CallAd("插屏广告" + text, "ShowInterstitial_Img");
#endif
        }

        /// <summary>
        /// 插屏视频
        /// </summary>
        /// <param name="succ"></param>
        /// <param name="fail"></param>
        private void ShowInterstitial_Video(string text = "", Action succ = null, Action fail = null)
        {
            if (!isLogin)
                return;
#if KuaiShou
#elif Vivo
#elif UNITY_ANDROID
            CallAd("插屏视频"+text,"ShowInterstitial_Video");
            succCallBack = succ;
            failCallBack = fail;
#else
            CallAd("插屏视频" + text, "ShowInterstitial_Video");
            succCallBack = succ;
            failCallBack = fail;
#endif
        }

        /// <summary>
        /// 激励视频
        /// </summary>
        /// <param name="succ"></param>
        /// <param name="fail"></param>
        private void ShowRewardVideo(string text = "", Action succ = null, Action fail = null)
        {
            if (!isLogin)
                return;

#if KuaiShou
            if (rewardVideoAd != null)
            {
                succCallBack = succ;
                failCallBack = fail;
                LogKit.Log("[激励广告] 调用广告show方法");
                rewardVideoAd.Show();
            }
            else
            {
                LogKit.Log("[激励广告] : 没有创建广告实例！" );
            }
#elif Vivo
#elif UNITY_ANDROID
            CallAd("激励视频"+text,"ShowRewardAd");
            succCallBack = succ;
            failCallBack = fail;
#else
            CallAd("激励视频" + text, "ShowRewardAd");
            succCallBack = succ;
            failCallBack = fail;
#endif
        }

        /// <summary>
        /// 广告调用
        /// </summary>
        /// <param name="text"></param>
        /// <param name="methodName"></param>
        private void CallAd(string text, string methodName = "")
        {
            if (isFreeVideoMod)
            {
                ADCallBack("1");
                return;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            
            AndroidJavaClass Toast = new AndroidJavaClass("android.widget.Toast");
            AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
            
            // 在 UI 线程上运行代码
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                try
                {
                    // 尝试调用指定方法
                    activity.Call(methodName);
                }
                catch (AndroidJavaException)
                {
                    // 如果方法不存在，显示 Toast 提示
                    string errorMessage = methodName + "--方法不存在";
                    AndroidJavaObject javaString = new AndroidJavaObject("java.lang.String", errorMessage);
                    Toast.CallStatic<AndroidJavaObject>("makeText", context, javaString, Toast.GetStatic<int>("LENGTH_SHORT")).Call("show");
                }
            }));
#elif UNITY_ANDROID && UNITY_EDITOR
            ADCallBack("1");
#elif UNITY_OPENHARMONY
            OpenHarmonyJSClass openHarmonyJSClass = new OpenHarmonyJSClass("ClassObjectTest");
            openHarmonyJSClass.CallStatic(methodName);
#endif
        }

        /// <summary>
        /// 安卓回调
        /// </summary>
        /// <param name="message"></param>
        public void ADCallBack(string message)
        {
            if (message == "1")
            {
                LogKit.Log("领取奖励成功");
                succCallBack?.Invoke();
            }
            else
            {
                LogKit.Error("领取奖励错误:" + message);
                failCallBack?.Invoke();
            }
        }
    }
}