using GDFrameworkCore;
using GDFrameworkExtend.FluentAPI;
using GDFrameworkExtend.ResKit;
using Luban;
using SimpleJSON;
using UnityEngine;

namespace GDFramework.LubanKit
{
    public class LubanKit : AbstractSystem
    {
        protected override void OnInit()
        {
            
        }

        public void InitData()
        {

        }
        
        /// <summary>
        /// 加载二进制格式的配置文件
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        private ByteBuf LoadByteBuf(string files)
        {
            byte[] jsons = null;
            var _mloader = ResLoader.Allocate();
            _mloader.Add2Load<TextAsset>($"yoo:{files}", (a, res) =>
            {
                if (a)
                {
                    jsons = res.Asset.As<TextAsset>().bytes;
                }
            });
            _mloader.LoadAsync();
            _mloader.Recycle2Cache();
            _mloader = null;
            return new ByteBuf(jsons);
        }
        
        /// <summary>
        /// 加载json格式的配置文件
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        private JSONNode LoadJson(string files)
        {
            string jsons = null;
            var _mloader = ResLoader.Allocate();
            _mloader.Add2Load<TextAsset>($"yoo:{files}", (a, res) =>
            {
                if (a)
                {
                    jsons = res.Asset.As<TextAsset>().text;
                }
            });
            _mloader.LoadAsync();
            _mloader.Recycle2Cache();
            _mloader = null;
            return JSON.Parse(jsons);

        }
    }
}