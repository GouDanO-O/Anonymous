using System.Collections;
using UnityEngine;
using YooAsset;

namespace GDFramework.HybridCLRToolKit
{
    public class HybridCLRToolKit
    {
        [SerializeField]private string _hotfixdllnames = "";
        
        public IEnumerator GetHotfixDill(string packageName = "DefaultPackage")
        {
            var packages = YooAssets.GetPackage(packageName);
#if UNITY_EDITOR
            //加载热更新DLL逻辑
            var goa = packages.LoadAssetAsync<TextAsset>(_hotfixdllnames);
            yield return goa.Task;
            var bts = goa.AssetObject as TextAsset;
            byte[] dllBytes = bts.bytes;
            System.Reflection.Assembly.Load(dllBytes);
            //加载aot泛型与热更新dll成功     
            Debug.Log("热更新dll加载成功");
#endif
        }
    }
}