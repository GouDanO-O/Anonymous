using System;
using System.Collections.Generic;
using GDFramework.Models.Enums;
using GDFramework.Utility;
using GDFrameworkCore;
using GDFrameworkExtend.LogKit;
using UnityEngine;

namespace GDFramework.Multilingual
{
    public class MultilingualDataModel : AbstractModel
    {
        public TextAsset LanguageTextAsset;

        public ELanguageType WillChangeLanguageType = ELanguageType.SimplifiedChinese;

        public Dictionary<string, Dictionary<ELanguageType, string>> TranslationDict = new();

        private ResourcesUtility _resoucesUtility;

        protected override void OnInit()
        {
            _resoucesUtility = this.GetUtility<ResourcesUtility>();
        }

        public void SetTranslation(string key, Dictionary<ELanguageType, string> languageData)
        {
            TranslationDict[key] = languageData;
        }

        /// <summary>
        /// 设置多语言文本json
        /// </summary>
        /// <param name="textAsset"></param>
        public void SetTextAsset(TextAsset textAsset)
        {
            LanguageTextAsset = textAsset;
            this.GetUtility<MultilingualUtility>().InitLanguage(this);
        }

        /// <summary>
        /// 获取当前多语言文本
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetMultilingual_Text(string key)
        {
            if (TranslationDict.Count == 0)
                return "";

            if (TranslationDict.ContainsKey(key) && TranslationDict[key].ContainsKey(WillChangeLanguageType))
                return TranslationDict[key][WillChangeLanguageType];

            return "";
        }

        /// <summary>
        /// 获取当前多语言图片--从Resources目录读取
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Sprite GetMultilingual_Sprite_Res(string key)
        {
            var folderName = WillChangeLanguageType.ToString();
            var path = $"Multilingual/{folderName}/{key}";
            var sprite = Resources.Load<Sprite>(path);
            if (sprite == null) 
                LogKit.Error($"未在路径中找到Sprite: {path}");
            return sprite;
        }

        /// <summary>
        /// 获取当前多语言图片--AB
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public void GetMultilingual_Sprite_AB(string key, Action<Sprite> callback)
        {
            var folderName = WillChangeLanguageType.ToString();
            var path = $"Multilingual/{folderName}/{key}";
            _resoucesUtility.LoadSpritesAsync(path, callback);
        }
    }
}