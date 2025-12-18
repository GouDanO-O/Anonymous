using GDFramework.Models;
using GDFramework.Models.Enums;
using GDFramework.Utility;
using GDFrameworkCore;
using GDFrameworkExtend.LogKit;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GDFramework.Multilingual
{
    public class MultilingualComponent : MonoBehaviour, ICanRegisterEvent, ICanGetModel
    {
        [ShowInInspector] private string key;

        [ShowInInspector] [LabelText("是否从AB包加载资源(文字除外)")]
        private bool isLoadFormAb;

        private Image _image;

        private RawImage _rawImage;

        private TextMeshProUGUI _textTmp;

        private Text _textNormal;


        private MultilingualDataModel _multilingualData_Model;

        private void Awake()
        {
        }

        private void Start()
        {
            GetMultilingualComponents();
            this.RegisterEvent<SOnChangeLanguageEvent>((data) => { ChangeLanguage(data.WillChangeLanguage); });
            GetMultilingual();
        }

        /// <summary>
        /// 初始化组件
        /// </summary>
        private void GetMultilingualComponents()
        {
            if (GetComponent<Image>())
                _image = GetComponent<Image>();
            else if (GetComponent<RawImage>()) _rawImage = GetComponent<RawImage>();

            if (GetComponent<TextMeshProUGUI>())
                _textTmp = GetComponent<TextMeshProUGUI>();
            else if (GetComponent<Text>()) _textNormal = GetComponent<Text>();

            _multilingualData_Model = this.GetModel<MultilingualDataModel>();
        }

        /// <summary>
        /// 获取多语言
        /// </summary>
        public void GetMultilingual()
        {
            if (key == "")
            {
                LogKit.Error($"{gameObject.name}:未配置key");
                return;
            }

            if (_multilingualData_Model == null)
            {
                LogKit.Error($"{gameObject.name}:尚未初始化");
                return;
            }

            if (_image)
            {
                if (isLoadFormAb)
                    _multilingualData_Model.GetMultilingual_Sprite_AB(key,
                        (callback) => _image.sprite = callback);
                else
                    _image.sprite = _multilingualData_Model.GetMultilingual_Sprite_Res(key);
            }
            else if (_rawImage)
            {
                if (isLoadFormAb)
                    _multilingualData_Model.GetMultilingual_Sprite_AB(key,
                        (callback) => _rawImage.texture = callback.texture);
                else
                    _rawImage.texture = _multilingualData_Model.GetMultilingual_Sprite_Res(key).texture;
            }

            if (_textNormal)
                _textNormal.text = _multilingualData_Model.GetMultilingual_Text(key);
            else if (_textTmp) _textTmp.text = _multilingualData_Model.GetMultilingual_Text(key);
        }

        /// <summary>
        /// 切换语言
        /// </summary>
        /// <param name="newLanguageType"></param>
        private void ChangeLanguage(ELanguageType newLanguageType)
        {
            GetMultilingual();
        }

        public IArchitecture GetArchitecture()
        {
            return Main.Interface;
        }
    }
}