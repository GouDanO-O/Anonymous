using System.Collections;

namespace GDFrameworkExtend.ResKit
{
    public interface IResDatas
    {
        string[] GetAllDependenciesByUrl(string url);

        void LoadFromFile(string outRes);
        void Reset();
        IEnumerator LoadFromFileAsync(string outRes);
        AssetData GetAssetData(ResSearchKeys resSearchKeys);
        int AddAssetBundleName(string abName, string[] depends, out AssetDataGroup @group);
        string GetABHash(string assetName);
    }
}