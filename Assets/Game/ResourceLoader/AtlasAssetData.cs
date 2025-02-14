using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class AtlasAssetData
    {
        public AssetBundle atlasBundle;

        /// <summary>
        /// 图集里面的散图assetPath路径  
        /// </summary>
        public List<string> imgAssetPaths = new List<string>();

        public void Unload(Dictionary<string, AssetBundle> abDic,Dictionary<string, string> imgAbDic)
        {
            foreach (string imgPath in imgAssetPaths)
            {
                if (abDic.ContainsKey(imgPath))
                {
                    abDic.Remove(imgPath);
                }                
                if (imgAbDic.ContainsKey(imgPath))
                {
                    imgAbDic.Remove(imgPath);
                }
            }
            imgAssetPaths.Clear();
            imgAssetPaths = null;
            
            atlasBundle.Unload(false);
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

    }
}