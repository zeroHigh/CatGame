using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
//using System.Linq;        // 慎用linq
using Object = UnityEngine.Object;

namespace Game
{
    public static class AssetBundleUtil
    {
        private static string _assetBundleRoot;
        /// ab路径与bundleData映射表
        private static Dictionary<string, AssetBundle> _assetBundleData;
        /// 相对路径与资源全路径映射表
        private static Dictionary<string, string> _relative2FullPath;
        /// 资源全路径与bundleData映射
        private static Dictionary<string, AssetBundle> _fullPath2BundleData;
        
        
        public static T LoadResByFile<T>(string path) where T : Object
        {
            var adjustAssetPath = AdjustAssetPath(path);//获取路径，返回的是去掉后缀的路径
            
            if (_relative2FullPath.TryGetValue(adjustAssetPath, out var fullPath) && _fullPath2BundleData.TryGetValue(fullPath, out var assetBundle))
            {
                return assetBundle?.LoadAsset<T>(fullPath);
            }
            
            if (commonResFullPath.TryGetValue(adjustAssetPath, out var commonPath) && commonResfullPathBundleData.TryGetValue(commonPath, out var commonRes))
            {
                return commonRes?.LoadAsset<T>(commonPath);
            }
            
            //老的加载方式，需要遍历所有ab，通过contain方式加载
            return LoadResForeachAllBundles<T>(adjustAssetPath);
        }

        private static T LoadResForeachAllBundles<T>(string path) where T : Object
        {
            foreach (var temp in _fullPath2BundleData)
            {
                if (temp.Key.Contains(path))
                    return temp.Value.LoadAsset<T>(temp.Key);
            }
            
            foreach (var temp in commonResfullPathBundleData)
            {
                if (temp.Key.Contains(path))
                    return temp.Value.LoadAsset<T>(temp.Key);
            }
            
            if (typeof(T) != typeof(AudioClip))
            {
                //音频文件会优先查找ab包，找不到后，才会去加载write path,所以不打印日志
                Logger.LogError("[AssetBundleUitl.LoadResForeachAllBundleData() => 加载资源失败，path:" + path + ", 资源类型:" + typeof(T) + "]");
            }
            return null;
        }
        
        public static void InitAssetBundle(string abRoot)
        {
            _assetBundleRoot = abRoot;
            _assetBundleData = new Dictionary<string, AssetBundle>();
            _relative2FullPath = new Dictionary<string, string>();
            _fullPath2BundleData = new Dictionary<string, AssetBundle>();

            commonResBundleData = new Dictionary<string, AssetBundle>();
            commonResFullPath = new Dictionary<string, string>();
            commonResfullPathBundleData = new Dictionary<string, AssetBundle>();

            IsCacheAllABAsset = false;
            _dependsDataList = new Dictionary<string, int>();
            _readyABList = new Dictionary<string, Action<string, bool>>();
            _loadingABList = new Dictionary<string, AssetBundleAsyncRequester>();
            _loadingCreaterList = new List<AssetBundleCreateRequester>();
            _loadingAssetList = new List<AssetAsyncRequester>();
            _assetBundleData = new Dictionary<string, AssetBundle>();
            _unloadABList = new List<string>();
            _assetsCacheList = new Dictionary<string, Object>();
            _assetbundleResident = new HashSet<string>();
            tempLoadings = new List<AssetBundleAsyncRequester>();
        }

        public static void LoadAssetBundle(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Logger.LogRed("[AssetBundleUtil.LoadAssetBundle() => 加载assetBundle失败，path为空]");
                return;
            }

            path = CheckAssetBundlePath(path);

            if (_assetBundleData.ContainsKey(path))
            {
                return;
            }
            var ab = AssetBundle.LoadFromFile(path);
            if (ab == null)
            {
                Logger.LogError("[AssetBundleLoader.LoadAssetBundleByPath() => Load AssetBundle出错，path:" + path + "]");
                return;
            }
            AnalysisAssetBundle(path, ab);
        }

        private static void AnalysisAssetBundle(string path, AssetBundle assetBundle)
        {
            _assetBundleData[path] = assetBundle;        // Path打包文件夹目录
            var allAssets = assetBundle.GetAllAssetNames();
            for (var i = 0; i < allAssets.Length; i++)
            {
                var tempAssetPath = allAssets[i];                // ab包里面的assetpath   Asset/xx/xx
                var assetPath = AdjustAssetPath(tempAssetPath);  //   
                _relative2FullPath[assetPath] = tempAssetPath;        // key 剔除后的路径 value assetpath
                _fullPath2BundleData[tempAssetPath] = assetBundle;    // key assetPath  value abInfo 
            }
        }

        #region 图集

        /// <summary>
        /// 提前加载的图集数据
        /// </summary>
        private static Dictionary<string, AtlasAssetData>
            PreLoadAtlasDataDic = new Dictionary<string, AtlasAssetData>();
        
        public static void LoadAtlasAssetBundle(string atlasAbPath)
        {
            if (string.IsNullOrEmpty(atlasAbPath))
            {
                Logger.LogRed("[AssetBundleUtil.LoadAtlasAssetBundle() => 加载图集assetBundle失败，path为空]");
                return;
            }
            if (atlasAbPath.StartsWith("/"))
            {
                //兼容以"/"开头的路径
                atlasAbPath = atlasAbPath.Substring(1, atlasAbPath.Length - 1);
            }
            atlasAbPath = _assetBundleRoot + atlasAbPath;
            if (_assetBundleData.ContainsKey(atlasAbPath))
            {
                return;
            }
            Debug.LogError("LoadAtlasAssetBundle:  " + atlasAbPath);
            var ab = AssetBundle.LoadFromFile(atlasAbPath);
            if (ab == null)
            {
                Logger.LogError("[AssetBundleLoader.LoadAtlasAssetBundle() => Load 图集 AssetBundle出错，path:" + atlasAbPath + "]");
                return;
            }

            if (!PreLoadAtlasDataDic.TryGetValue(atlasAbPath, out AtlasAssetData atlasData))
            {
                atlasData = new AtlasAssetData();
                atlasData.atlasBundle = ab;
                SaveSingleImg(atlasData);
            }
            else
            {
                Logger.Log(string.Format("重复加载图集ab:{0}.",atlasAbPath));
            }
        }

        private const string IgnoreExt = ".spriteatlas";

        /// <summary>
        /// key   散图的asset路径
        /// value 对应的 ab
        /// </summary>
        private static Dictionary<string, AssetBundle> singleImageAbDic = new Dictionary<string, AssetBundle>();
        /// <summary>
        /// key      散图的asset路径
        /// value    对应的ab资源的assetBundlePath
        /// </summary>
        private static Dictionary<string, string> singleImgeAbPath = new Dictionary<string, string>();

        private static void SaveSingleImg(AtlasAssetData atlasData)
        {
            var atlasAb = atlasData.atlasBundle;
            string[] allAssets = atlasAb.GetAllAssetNames();
            for (var i = 0; i < allAssets.Length; i++)
            {
                string tempName = allAssets[i];      
                string ext = Path.GetExtension(tempName);
                if (ext.CompareTo(IgnoreExt) == 0)
                    continue;
                string imgAssetPath = Path.GetFileNameWithoutExtension(tempName);        // 图集ab里面的散图名字
                singleImageAbDic[imgAssetPath] = atlasAb;
                singleImgeAbPath[imgAssetPath] = tempName;
                atlasData.imgAssetPaths.Add(imgAssetPath);
            }
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="atlasAbPath"></param>
        public static void UnloadAtlasAsset(string atlasAbPath)
        {
            PreLoadAtlasDataDic.TryGetValue(atlasAbPath, out AtlasAssetData atlasData);
            if (atlasData == null)
                return;
            PreLoadAtlasDataDic.Remove(atlasAbPath);
            atlasData.Unload(singleImageAbDic, singleImgeAbPath);
        }
        public static T LoadSprite<T>(string imgPath) where T: Object
        {
            string imgName = Path.GetFileNameWithoutExtension(imgPath);
            if (!singleImageAbDic.TryGetValue(imgName, out AssetBundle atlasAb))
                return null;
            if (!singleImgeAbPath.TryGetValue(imgName, out string imgAbAssetPath))
                return null;
            return atlasAb.LoadAsset<T>(imgAbAssetPath);
        }

        #endregion
        

        public static void UnloadAssetBundle(string path)
        {
            path = CheckAssetBundlePath(path);

            if (_assetBundleData.TryGetValue(path, out var bundle))
            {
                DoUnloadBundleData(bundle);
                _assetBundleData.Remove(path);
            }
        }

        private static void DoUnloadBundleData(AssetBundle ab)
        {
            if (ab == null)
            {
                return;
            }
            RemoveAllPathKey(ab.GetAllAssetNames());
            try
            {
                ab.Unload(true);
            }
            catch (Exception e)
            {
                Logger.LogError("[AssetBundleUtil.DoUnloadBundleData: 卸载ab失败，msg:" + e.Message + "]");
            }
        }

        /// 通过资源全路径(ab.GetAllAssetNames()数组)移除路径的key
        private static void RemoveAllPathKey(string[] allAssets)
        {
            for (var i = 0; i < allAssets.Length; i++)
            {
                var fullPath = allAssets[i];
                if (_fullPath2BundleData.ContainsKey(fullPath))
                {
                    _fullPath2BundleData.Remove(fullPath);
                }
            }
        }

        public static void UnloadAllBundle()
        {
            if (_assetBundleData == null || _assetBundleData.Count == 0)
                return;
            foreach (var ab in _assetBundleData.Values)    
            {
                ab.Unload(true);
            }

            _assetBundleData.Clear();
            _relative2FullPath.Clear();
            _fullPath2BundleData.Clear();
        }

        #region adjust path

        static readonly List<string> _prefixPath = new List<string>() { 
            "assets/res/",
            "assets/jojoreadres/res/",
            "assets/versionres/focusshare/",
            "assets/versionres/commonshare/",
            "assets/templateres/res/"
        };

        private static StringBuilder _pathBuilder;
        
        private static string AdjustAssetPath(string path)
        {
            string tempPath = QuestionSubFolderPathSplit(path);
            path = tempPath.ToLower();

            if (_pathBuilder == null)
                _pathBuilder = new StringBuilder();
            else
                _pathBuilder.Clear();            
            _pathBuilder.Append(path);
            
            var extension = Path.GetExtension(path);
            if (!string.IsNullOrEmpty(extension))
            {
                _pathBuilder.Remove(_pathBuilder.Length - extension.Length, extension.Length);
            }
            for (var i = _prefixPath.Count - 1; i >= 0; i--) 
            {
                string preFixPath = _prefixPath[i];
                if (path.StartsWith(preFixPath))
                {
                    _pathBuilder.Remove(0, preFixPath.Length);
                    return _pathBuilder.ToString();
                }
            }
            return _pathBuilder.ToString();
        }

        #region Question下的[ig]处理
        
        private const string Question = "res/question/";
        private const string RegexStr = @"\[ig\][\w\d]+\/";
        /// <summary>
        /// 特殊处理下Question文件夹多一层的问题
        /// </summary>
        /// <returns></returns>
        private static string QuestionSubFolderPathSplit(string assetPath)
        {
            if (!assetPath.Contains(Question))
                return assetPath;
            MatchCollection mc = Regex.Matches(assetPath, RegexStr);
            if (mc.Count != 1)       // 理论上这里不打日志,编辑器下开发的时候就会报错处理 
                return assetPath;
            string replacePath = assetPath.Replace(mc[0].ToString(), "");
            return replacePath;
        }

        #endregion

        
        
        #endregion

        #region CommonShareRes加载卸载相关
        /*
         * 接口设计背景:
         * RunGame的时候会重复加载卸载,为提高下次进入速度
         * 解决办法:
         * 公共资源不卸载 再退出Unity的时候统一卸载
         */
        
        /// ab路径与bundleData映射表
        private static Dictionary<string, AssetBundle> commonResBundleData;
        /// 相对路径与资源全路径映射表
        private static Dictionary<string, string> commonResFullPath;
        /// 资源全路径与bundleData映射
        private static Dictionary<string, AssetBundle> commonResfullPathBundleData;
        
        
        public static void PreLoadCommonRes(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Logger.LogRed("[AssetBundleUtil.PreLoadCommonRes() => 加载assetBundle失败，path为空]");
                return;
            }

            path = CheckAssetBundlePath(path);

            if (commonResBundleData.ContainsKey(path))
            {
                return;
            }
            Debug.LogError("PreLoadCommonRes:  " + path);
            var ab = AssetBundle.LoadFromFile(path);
            if (ab == null)
            {
                Logger.LogError("[AssetBundleLoader.PreLoadCommonRes() => Load AssetBundle出错，path:" + path + "]");
                return;
            }
            AnalysisCommonResAssetBundle(path, ab);
        }

        private static void AnalysisCommonResAssetBundle(string path, AssetBundle assetBundle)
        {
            commonResBundleData[path] = assetBundle;        // Path打包文件夹目录
            var allAssets = assetBundle.GetAllAssetNames();
            for (var i = 0; i < allAssets.Length; i++)
            {
                var tempAssetPath = allAssets[i];
                var assetPath = AdjustAssetPath(tempAssetPath);
                commonResFullPath[assetPath] = tempAssetPath;
                commonResfullPathBundleData[tempAssetPath] = assetBundle;
            }
        }
        
        
        
        public static void UnloadCommonRes(string path)
        {
            path = CheckAssetBundlePath(path);
            if (commonResBundleData.TryGetValue(path, out var bundle))
            {
                DoUnloadCommonResData(bundle);
                commonResBundleData.Remove(path);
            }
        }
        
        private static void DoUnloadCommonResData(AssetBundle ab)
        {
            if (ab == null)
            {
                return;
            }
            RemoveCommonResPathKey(ab.GetAllAssetNames());
            try
            {
                ab.Unload(true);
            }
            catch (Exception e)
            {
                Logger.LogError("[AssetBundleUtil.DoUnloadBundleData: 卸载ab失败，msg:" + e.Message + "]");
            }
        }

        private static void RemoveCommonResPathKey(string[] allAssets)
        {
            for (var i = 0; i < allAssets.Length; i++)
            {
                var fullPath = allAssets[i];
                if (commonResfullPathBundleData.ContainsKey(fullPath))
                {
                    commonResfullPathBundleData.Remove(fullPath);
                }
            }
        }

        public static void UnloadAllCommonResBundle()
        {
            if (commonResBundleData == null || commonResBundleData.Count == 0)
                return;
            foreach (var ab in commonResBundleData.Values)    
            {
                ab.Unload(true);
            }
        
            commonResBundleData.Clear();
            commonResFullPath.Clear();
            commonResfullPathBundleData.Clear();
        }

        private static string CheckAssetBundlePath(string path) {

            if (path.StartsWith(_assetBundleRoot))//全量路径不处理
                return path;

            if (path.StartsWith(Application.streamingAssetsPath))//全量路径不处理
                return path;

            if (path.StartsWith("/"))
            {
                //兼容以"/"开头的路径
                path = path.Substring(1, path.Length - 1);
            }

            path = _assetBundleRoot + path; 

            return path;
        }

        #endregion

        #region 新增异步加载 相关接口

        private static bool IsCacheAllABAsset;//是否缓存所有ab包资源 true时在ab包加载完成时混存 

        private static object manifest; // 为了兼容老版本所以使用object，使用时用as转换
        private static int MAX_LOADING_COUNT = 10; //同时加载的最大数量
        private static Dictionary<string, int> _dependsDataList;//ab包引用计数
        private static Dictionary<string, Action<string, bool>> _readyABList; //预备加载的列表
        private static Dictionary<string, AssetBundleAsyncRequester> _loadingABList; //正在加载的ab任务
        private static List<AssetBundleCreateRequester> _loadingCreaterList;
        private static List<AssetAsyncRequester> _loadingAssetList;//资源加载句柄

        //private static Dictionary<string, AssetBundle> _assetBundleData; //加载完成的列表
        private static List<string> _unloadABList; //准备卸载的列表
        private static Dictionary<string, UnityEngine.Object> _assetsCacheList;// asset缓存：给非公共ab包的asset提供逻辑层的复用
        private static HashSet<string> _assetbundleResident;//常驻ab包：需要手动添加公共ab包进来，常驻包不会自动卸载（即使引用计数为0），引用计数为0时可以手动卸载
        private static List<AssetBundleAsyncRequester> tempLoadings; //临时存储变量

        public static bool IsUseRefCount = false;//是否使用引用技术 回影响到卸载流程


        /// <summary>
        /// 初始化ab包清单信息  如果要使用依赖加载
        /// </summary>
        /// <param name="abPath"></param>
        public static void InitManifestInfo(string abPath)
        {
            //if (Global.appMinor < 109)
            //    return;
            _InitManifestInfo(abPath);
        }

        /// <summary>
        /// 避免老版本，IL编译后同一方法还是在引用AssetBundleManifest，版本判断根本不起作用
        /// </summary>
        /// <param name="abPath"></param>
        private static void _InitManifestInfo(string abPath)
        {
            var assetBundle = AssetBundle.LoadFromFile(abPath);
            manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }

        //设置使用引用技术
        public static void SetUseRefCount(bool use)
        {
            IsUseRefCount = use;
        }
        //获取ab包引用信息
        private static int GetReferenceCount(string assetbundlePath)
        {
            assetbundlePath = GetAssetBundleFullPath(assetbundlePath);
            if (string.IsNullOrEmpty(assetbundlePath))
                return -1;
            int count = 0;
            _dependsDataList.TryGetValue(assetbundlePath, out count);
            return count;
        }
        //增加一次引用
        private static int IncreaseReferenceCount(string assetbundlePath)
        {
            assetbundlePath = GetAssetBundleFullPath(assetbundlePath);
            if (string.IsNullOrEmpty(assetbundlePath))
                return -1;
            int count = 0;
            _dependsDataList.TryGetValue(assetbundlePath, out count);
            count++;
            _dependsDataList[assetbundlePath] = count;
            return count;
        }
        //减少一次引用
        private static int DecreaseReferenceCount(string assetbundleName)
        {
            assetbundleName = GetAssetBundleFullPath(assetbundleName);
            if (string.IsNullOrEmpty(assetbundleName) || !_dependsDataList.ContainsKey(assetbundleName))
            {
                return -1;
            }
            int count = 0;
            _dependsDataList.TryGetValue(assetbundleName, out count);
            count--;
            if (count <= 0)
            {
                _dependsDataList.Remove(assetbundleName);
                if (!IsAssetBundleResident(assetbundleName))//常驻不卸载
                    _unloadABList.Add(assetbundleName);
            }
            else
            {
                _dependsDataList[assetbundleName] = count;
            }
            return count;
        }

        private static bool CreateAssetBundleCreateRequest(string assetbundlePath)
        {
            Logger.LogRed("创建加载器：" + assetbundlePath);
            assetbundlePath = GetAssetBundleFullPath(assetbundlePath);

            if (string.IsNullOrEmpty(assetbundlePath) || IsAssetBundleLoaded(assetbundlePath) || _loadingABList.ContainsKey(assetbundlePath))
            {
                return false;
            }
            //var creater = AssetBundle.LoadFromFileAsync(assetbundlePath);
            //小游戏这里需要下载
            UnityWebRequest creater;
#if UNITY_EDITOR//web版本 这里需要兼容一下EDITOR环境下的路径
            creater = UnityWebRequestAssetBundle.GetAssetBundle("file://" + assetbundlePath);
#else
            creater = UnityWebRequestAssetBundle.GetAssetBundle(assetbundlePath);
#endif
            _loadingCreaterList.Add(new AssetBundleCreateRequester(assetbundlePath, creater));
            // 创建器持有的引用：创建器对每个ab来说是全局唯一的
            IncreaseReferenceCount(assetbundlePath);
            return true;
        }

        //获取ab包缓存
        public static AssetBundle GetAssetBundleCache(string assetbundlePath)
        {
            AssetBundle target = null;
            _assetBundleData.TryGetValue(assetbundlePath, out target);
            return target;
        }
        //删除ab包缓存
        public static void RemoveAssetBundleCache(string assetbundlePath)
        {
            _assetBundleData.Remove(assetbundlePath);
        }
        //添加ab包缓存
        public static void AddAssetBundleCache(string assetbundlePath, AssetBundle assetBundle)
        {
            AnalysisAssetBundle(assetbundlePath, assetBundle);
        }
        //是否存在资源缓存
        public static bool IsAssetCache(string assetPath)
        {
            if (_assetsCacheList.ContainsKey(assetPath) && _assetsCacheList[assetPath] != null)
            {
                return true;
            }
            return false;
        }
        //获取资源缓存
        public static UnityEngine.Object GetAssetCache(string assetPath)
        {
            UnityEngine.Object target = null;
            _assetsCacheList?.TryGetValue(assetPath, out target);
            return target;
        }
        //添加资源缓存
        public static void AddAssetCache(string assetPath, UnityEngine.Object asset)
        {
            _assetsCacheList[assetPath] = asset;
        }
        //是否是常驻ab包
        public static bool IsAssetBundleResident(string assebundlePath)
        {
            assebundlePath = GetAssetBundleFullPath(assebundlePath);
            return _assetbundleResident.Contains(assebundlePath);
        }
        //是否已经加载ab包
        public static bool IsAssetBundleLoaded(string assebundlePath)
        {
            assebundlePath = GetAssetBundleFullPath(assebundlePath);
            return _assetBundleData.ContainsKey(assebundlePath);
        }
        //设置ab包常驻状态
        public static void SetAssetBundleResident(string assebundlePath, bool resident)
        {
            assebundlePath = GetAssetBundleFullPath(assebundlePath);
            bool exist = _assetbundleResident.Contains(assebundlePath);
            if (resident && !exist)
            {
                _assetbundleResident.Add(assebundlePath);
            }
            else if (!resident && exist)
            {
                _assetbundleResident.Remove(assebundlePath);
            }
        }

        public static UnityWebRequest GetAssetBundleAsyncCreater(string assetbundlePath)
        {
            foreach (var requester in _loadingCreaterList)
            {
                if (requester?.abPath == assetbundlePath)
                {
                    return requester?.abCreaterRequest;
                }
            }
            return null;
        }

        private static void UpdateUnLoad()
        {
            if (_unloadABList.Count == 0 || !IsUseRefCount) return;

            //从后向前 避免引用技术归零的ab进入卸载列表
            for (int i = _unloadABList.Count - 1; i >= 0; i--)
            {
                string path = _unloadABList[i];
                int count = GetReferenceCount(path);
                if (count <= 0)
                {
                    UnloadAssetBundle(path);
                    string[] dependencies = GetDependencies(path);
                    if (dependencies != null)
                        foreach (var dependencie in dependencies)
                        {
                            DecreaseReferenceCount(dependencie);
                        }
                }
            }
        }

        private static void UpdateReady()
        {
            if (_readyABList.Count == 0) return;

            int slotCount = _loadingCreaterList.Count;
            while (slotCount < MAX_LOADING_COUNT && _readyABList.Count > 0)
            {
                var e = _readyABList.GetEnumerator();
                e.MoveNext();
                var temp = e.Current;
                var key = temp.Key;
                Action<string, bool> action = temp.Value;
                LoadAssetBundleAsync(key, action);
                _readyABList.Remove(key);
                slotCount++;
            }
        }


        private static void UpdateLoading()
        {
            UpdateABCreater();//因为如果使用依赖加载 一个加载任务可能对应多个创建器

            if (_loadingABList.Count == 0) return;
            tempLoadings.Clear();
            foreach (var abRequester in _loadingABList.Values)
            {
                abRequester.Update();
                if (abRequester.isDone)
                {
                    tempLoadings.Add(abRequester);
                    abRequester.callbackAction?.Invoke(abRequester.assetbundleName, true);
                }
            }

            foreach (var abRequester in tempLoadings)
            {
                _loadingABList.Remove(abRequester.assetbundleName);
                abRequester?.Dispose();
            }

            if (_loadingAssetList.Count == 0) return;
            for (int i = _loadingAssetList.Count - 1; i >= 0; i--)
            {
                AssetAsyncRequester aar = _loadingAssetList[i];
                aar.Update();
                if (aar.isDone)
                {
                    aar.callbackAction?.Invoke(aar.asset);
                    aar.Dispose();
                    _loadingAssetList.RemoveAt(i);
                }

            }
        }

        private static void UpdateABCreater()
        {
            if (_loadingCreaterList.Count == 0) return;

            for (int i = _loadingCreaterList.Count - 1; i >= 0; i--)
            {
                var abCreater = _loadingCreaterList[i];
                if (abCreater.abCreaterRequest != null && abCreater.abCreaterRequest.isDone && abCreater.abCreaterRequest.downloadProgress == 1.0f)
                {
                    _loadingCreaterList.RemoveAt(i);
                    AnalysisAssetBundle(abCreater.abPath, abCreater.assetbundle);
                    abCreater?.Dispose();
                    //DecreaseReferenceCount(abCreater.abPath);//解除创建器对ab包的引用
                }
            }
        }

        public static void Update()
        {
            UpdateLoading();
            UpdateReady();
            UpdateUnLoad();
        }
        //异步加载ab包
        public static void LoadAssetBundleAsync(string path, System.Action<string, bool> action = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                Logger.LogRed("[AssetBundleUtil.LoadAssetBundle() => 加载assetBundle失败，path为空]");
                return;
            }

            //以有等待任务
            if (_readyABList.ContainsKey(path))
                return;

            //当前加载数量达到设定上限 放入等待列表
            if (_loadingABList.Count >= MAX_LOADING_COUNT)
            {
                _readyABList[path] = action;
                return;
            }

            path = CheckAssetBundlePath(path);

            //已有ab包
            if (_assetBundleData.ContainsKey(path))
            {
                return;
            }
            //已有正在加载任务
            if (_loadingABList.ContainsKey(path))
            {
                return;
            }
            AssetBundleAsyncRequester request = AssetBundleAsyncRequester.Get();
            string assetBundleName = Path.GetFileName(path);
            string[] dependencies = GetDependencies(path);

            HandleManifestReference(dependencies, assetBundleName, request);

            request.Init(path, dependencies, action);
            CreateAssetBundleCreateRequest(path);
            _loadingABList[path] = request;
            //// 加载器持有的引用：同一个ab能同时存在多个加载器，等待ab创建器完成
            //IncreaseReferenceCount(path);
        }

        /// <summary>
        /// 降低逻辑复杂度
        /// </summary>
        /// <param name="dependencies"></param>
        /// <param name="assetBundleName"></param>
        /// <param name="request"></param>
        private static void HandleManifestReference(string[] dependencies, string assetBundleName, AssetBundleAsyncRequester request)
        {
            if (manifest != null && dependencies != null)
            {
                for (int i = 0; i < dependencies.Length; i++)
                {
                    var dependance = dependencies[i];
                    if (!string.IsNullOrEmpty(dependance) && dependance != assetBundleName)
                    {
                        bool result = CreateAssetBundleCreateRequest(dependance);
            
                        // A依赖于B，A对B持有引用
                        IncreaseReferenceCount(dependance);
                        if (result)
                            _loadingABList[GetAssetBundleFullPath(dependance)] = request;
            
                    }
                }
            }
        }


        //获取ab包全路径
        public static string GetAssetBundleFullPath(string assetBundleName)
        {
            if (string.IsNullOrEmpty(assetBundleName)) return assetBundleName;
            if (assetBundleName.StartsWith(_assetBundleRoot)) return assetBundleName;
            string path = _assetBundleRoot + assetBundleName;
            if (File.Exists(path))
            {
                return path;
            }
            else
            {
                path = Application.streamingAssetsPath + "/" + assetBundleName;
                return path;
            }
        }
        //获取ab包引用 目前默认为直接引用
        public static string[] GetDependencies(string assetBundleName)
        {
            if (manifest == null || string.IsNullOrEmpty(assetBundleName))
            {
                return null;
            }
            var _m = manifest as AssetBundleManifest;
            return _m.GetDirectDependencies(Path.GetFileName(assetBundleName));//这里暂时使用获取直接引用
        }


        public static AssetBundleAsyncRequester GetAssetBundleAsyncRequester(string assetbundlePath)
        {
            AssetBundleAsyncRequester requester = null;
            _loadingABList.TryGetValue(assetbundlePath, out requester);
            return requester;
        }

        private static UnityEngine.Object LoadResForeachAllBundles(string path, Type type)
        {
            foreach (var temp in _fullPath2BundleData)
            {
                if (temp.Key.Contains(path))
                    return temp.Value.LoadAsset(temp.Key,type);
            }

            foreach (var temp in commonResfullPathBundleData)
            {
                if (temp.Key.Contains(path))
                    return temp.Value.LoadAsset(temp.Key,type);
            }

            if (type != typeof(AudioClip))
            {
                //音频文件会优先查找ab包，找不到后，才会去加载write path,所以不打印日志
                Logger.LogError("[AssetBundleUitl.LoadResForeachAllBundleData() => 加载资源失败，path:" + path + ", 资源类型:" + type.ToString() + "]");
            }
            return null;

        }

        //异步加载资源 根据文件 先找缓存 若无 再动态加载ab包
        public static void LoadResByFileAsync(string path, Type type, Action<UnityEngine.Object> action, string assetBundlePath = null)
        {
            var adjustAssetPath = AdjustAssetPath(path);//获取路径，返回的是去掉后缀的路径
            UnityEngine.Object obj = null;
            if (_relative2FullPath.TryGetValue(adjustAssetPath, out var fullPath) && _fullPath2BundleData.TryGetValue(fullPath, out var assetBundle))
            {
                obj = assetBundle?.LoadAsset(fullPath, type);
                action?.Invoke(obj);
                return;
            }
            //老的加载方式，需要遍历所有ab，通过contain方式加载
            obj = LoadResForeachAllBundles(adjustAssetPath, type);
            if (obj != null)
            {
                action?.Invoke(obj);
                return;
            }

            if (string.IsNullOrEmpty(assetBundlePath))
            {
                action?.Invoke(obj);
                return;
            }

            LoadAssetAsync(path, action, assetBundlePath);
        }
        ///异步加载资源 先加载ab包 在load asset
        private static void LoadAssetAsync(string path, Action<UnityEngine.Object> action, string assetbundlePath)
        {
            var requester = AssetAsyncRequester.Get();
            _loadingAssetList.Add(requester);
            LoadAssetBundleAsync(assetbundlePath);
            var abRequester = GetAssetBundleAsyncRequester(assetbundlePath);
            string[] des = abRequester.waitingList?.ToArray();
            requester.Init(path, des, assetbundlePath, action);
        }

#endregion
    }
}
