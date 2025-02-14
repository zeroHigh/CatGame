using UnityEngine;

namespace Game
{
    public class AssetBundleLoader : BaseLoader
    {
        public AssetBundleLoader(string root)
        {
            AssetBundleUtil.InitAssetBundle(root);
        }

        protected override T OnLoadResByFileName<T>(string path)
        {
            return AssetBundleUtil.LoadResByFile<T>(path);
        }

        protected override T OnLoadResWithPath<T>(string path, ResType type)
        {
            return LoadResByFileName<T>(path);
        }

        public override void ClearAllRes()
        {
            AssetBundleUtil.UnloadAllBundle();
        }

        public override void UnloadRes(string path, bool isForce = false)
        {
            AssetBundleUtil.UnloadAssetBundle(path);
        }

        public override void UnloadUnusedRes()
        {
            Resources.UnloadUnusedAssets();
        }

        public override void PreLoadAsset(string path)
        {
            AssetBundleUtil.LoadAssetBundle(path);
        }
        
        #region 图集相关
        public override void PreLoadAtlas(string atlasPath)
        {
            AssetBundleUtil.LoadAtlasAssetBundle(atlasPath);
        }

        public override void UnloadAtlas(string atlasPath)
        {
            AssetBundleUtil.UnloadAtlasAsset(atlasPath);
        }

        public override Sprite LoadSprite(string imgPath)
        {
            Sprite sprite = AssetBundleUtil.LoadSprite<Sprite>(imgPath);
            return sprite;
        }
        #endregion

        #region 公共资源加载释放

        public override void PreLoadCommonRes(string resPath)
        {
            AssetBundleUtil.PreLoadCommonRes(resPath);
        }

        public override void UnloadAllCommonRes()
        {
            AssetBundleUtil.UnloadAllCommonResBundle();
        }

        #endregion
		
		#region 异步加载
        protected override void OnLoadResWithPathAsync(string path, System.Type type, System.Action<UnityEngine.Object> action,string assetbundlePath=null)
        {

            AssetBundleUtil.LoadResByFileAsync(path,type,action, assetbundlePath);
        }

        public override void PreLoadAssetAsync(string path,System.Action<string,bool> action = null)
        {
            AssetBundleUtil.LoadAssetBundleAsync(path, action);
        }
        #endregion
        
    }
}

