using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Game
{
    /// <summary>
    /// 资源加载逻辑，可自定义具体Loader(实现IResLoader接口即可)。通过SetLoader()设置即可
    /// </summary>
    public class ResourceLoader : ILSingleton<ResourceLoader>
    {
        public string writablePath = "";
        private BaseLoader loader;       
        
        protected class ResourceLoaderBehaviour : MonoBehaviour
        {
            void Awake()
            {
                DontDestroyOnLoad(this.gameObject);
            }
        }

        /// <summary>
        /// 这个作用就是拿来开协程 后期可以优化
        /// TODO： 资源加载优化点
        /// 1：避免不必要的newobj操作
        /// 2：统一协程入口，给携程开启id便于关闭操作
        /// </summary>
        private static ResourceLoaderBehaviour loaderBehaviour; 
        public void Init(BaseLoader loader)
        {
            this.loader = loader;
            this.loader.Init();
            loaderBehaviour = new GameObject("ResourceLoader").AddComponent<ResourceLoaderBehaviour>();
        }

        #region prepare load/unload bundle相关接口

        public void PrepareBundle(string bundlePath)
        {
            loader.PreLoadAsset(bundlePath);
        }

        public void PrepareBundle(List<string> value)
        {
            if (value == null || value.Count == 0)
                return;
            for(var i = 0; i < value.Count; i++)
                PrepareBundle(value[i]);
        }

        public void UnloadPreBundle(List<string> value)
        {
            if (value == null || value.Count == 0)
                return;
            for(var i = 0; i < value.Count; i++)
                UnloadPreBundle(value[i]);
        }
        
        public void UnloadPreBundle(string bundlePath)
        {
            if (string.IsNullOrEmpty(bundlePath) || !GameStart.Instance.UseAssetBundle)
            {
                return;
            }
            loader?.UnloadRes(bundlePath);
        }

        public void UnloadUnusedRes()
        {
            loader?.UnloadUnusedRes();
        }

        public void ClearAllRes()
        {
            loader?.ClearAllRes();
            UnloadUnusedRes();
        }

        #endregion

        #region Search Path
        public void AddSearchPath(string path)
        {
            loader?.AddSearchPath(path);
        }

        public void AddSearchPath(List<string> paths)
        {
            if (paths == null || paths.Count == 0)
                return;
            loader?.AddSearchPath(paths);
        }

        public void RemoveSearchPath(string path)
        {
            loader?.RemoveSearchPath(path);
        }

        public void RemoveSearchPath(List<string> paths)
        {
            if (paths == null || paths.Count == 0)
                return;
            loader?.RemoveSearchPath(paths);
        }

        public void RemoveAllSearchPath()
        {
            loader?.RemoveAllSearchPath();
        }
        #endregion

        #region loadScene、UnloadScene接口

        public AsyncOperation LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            return loader?.LoadSceneAsync(sceneName, mode);
        }

        public AsyncOperation UnloadScene(string sceneName)
        {
            return loader?.UnloadSceneAsync(sceneName);
        }

        #endregion

        #region 常见对外接口
        
        public GameObject LoadObject(string fileName)
        {
            GameObject obj = LoadResWithPath<GameObject>(fileName, ResType.Prefab);
            if (obj != null)
                return GameObject.Instantiate(obj);
            return null;
        }

        public Material LoadMaterial(string fileName)
        {
            Material material = LoadResWithPath<Material>(fileName, ResType.Material);
            if (material != null)
                return material;
            return null;
        }
        
        /// 加载资源，带后缀名加载
        public T LoadResByFileName<T>(string fileName) where T : Object
        {
            return loader?.LoadResByFileName<T>(fileName);
        }
        
        /// 根据ResType加载资源，不带后缀名加载
        public T LoadResWithPath<T>(string fileName, ResType type) where T : Object
        {
            return loader?.LoadResWithPath<T>(fileName, type);
        }
        #endregion

        #region 加载writepath目录下的txt、json文件接口

        /// <summary>
        /// 加载WritePath目录下的text文件(.txt,.json等)
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="action"></param>
        public void LoadWritePathTextAsset(string filePath, Action<string> action)
        { 
            var uri = new Uri(GetWritablePath(filePath));
            CoroutineWebRequest(uri.AbsoluteUri, (bytes) =>
            {
                var text = Encoding.UTF8.GetString(bytes);
                action?.Invoke(text);
            });
        }
        
        public void CoroutineWebRequest(string url, Action<byte[]> action)
        {
            loaderBehaviour.StartCoroutine(StartCorRequest(url, action));
        }

        private IEnumerator StartCorRequest(string filePath, Action<byte[]> action)
        {
            Logger.Log("filePath:" + filePath);
            
            var request = UnityWebRequest.Get(filePath);
            yield return request.SendWebRequest();
            if (request.isDone&& !request.isNetworkError && !request.isHttpError)
                action?.Invoke(request.downloadHandler.data);
            else
                action?.Invoke(null);
        }

        #endregion
        
        #region 音频加载相关

        public void LoadAudioClip(string filePath, Action<AudioClip> action)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                action?.Invoke(null);
                return;
            }
            AudioClip audioClip = LoadResByFileName<AudioClip>(filePath);
            if (audioClip != null)
            {
                action?.Invoke(audioClip);
            }
            else
            {
                string url = GetWritablePath(filePath);
                if (!url.Contains("http"))
                    url = "file://" + url;
                loaderBehaviour.StartCoroutine(LoadAudioFromWritablePath(url, action));//零散资源里音频文件，需要通过协程加载
            }
        }

        private IEnumerator LoadAudioFromWritablePath(string url, Action<AudioClip> completeAction)//string filePath, Action<AudioClip> completeAction)
        {
            Logger.Log("[ResourceLoader.LoadAudioClip() => 资源路径:" + url + "]");
            var webRequest = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
            yield return webRequest.SendWebRequest();
            if (webRequest.isDone && !webRequest.isHttpError && !webRequest.isNetworkError)
            {
                var downloadHandler = (DownloadHandlerAudioClip)webRequest.downloadHandler;
                if (downloadHandler != null)
                {
                    //先压缩一次音频资源
                    CompressionAudioDownloadHandler(downloadHandler);
                    completeAction?.Invoke(downloadHandler.audioClip);
                }
                else
                {
                    completeAction?.Invoke(null);
                }
            }
            else
            {
                completeAction?.Invoke(null);
                Logger.LogError("[ResourceLoader.LoadAudioFromWritablePath () => 加载音频文件失败，file:" + url + "]");
            }
        }
        
        private void CompressionAudioDownloadHandler(DownloadHandlerAudioClip downloadHandlerAudioClip)
        {
            downloadHandlerAudioClip.compressed = true;
        }

        #endregion
        
        
        private byte[] LoadByteFile(string file_name)
        {
            try
            {
                if (!string.IsNullOrEmpty(file_name))
                {
                    if (File.Exists(file_name))
                        return File.ReadAllBytes(file_name);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }

            return null;
        }

        private string GetWritablePath(string filePath)
        {
            string tmpPath = writablePath;
            return tmpPath + filePath; 
        }

        #region 新的图集ab加载
        
 
        /// <summary>
        /// 图集预加载
        /// </summary>
        /// <param name="atlasAbPath"></param>
        public void PreloadAtlas(string atlasAbPath)
        {
            if (string.IsNullOrEmpty(atlasAbPath))
                return;
            loader?.PreLoadAtlas(atlasAbPath);
        }

        public void UnloadAtlas(string atlasAbPath)
        {
            if (string.IsNullOrEmpty(atlasAbPath))
                return;
            loader?.UnloadAtlas(atlasAbPath);
        }

        /// <summary>
        /// 从ab里面找到图集
        /// </summary>
        /// <param name="atlasAabPath">散图路径 文件名/路径均可</param>
        /// <returns></returns>
        public Sprite LoadSpriteFromAb(string imgPath, bool isFullPath = false)
        {
            var img =  loader?.LoadSprite(imgPath);
            if (img != null)
            {
                return img;
            }
            return LoadSprite(imgPath, isFullPath);
        }
        #endregion
        
        
        public Sprite LoadSprite(string path, bool isFullPath = false)
        {
            Logger.Log("LoadSprite: "+path+"    "+isFullPath.ToString());
            var texturePath = path;
            if (!isFullPath)
            {
                texturePath = GetWritablePath(path);
            }
            var data = LoadByteFile(texturePath);
            if (data == null)
            {
                Logger.LogError("[ResourceLoader.LoadSprite() 加载sprite失败, path:" + path + "]");
                return null;
            }
            var texture2D = new Texture2D(1, 1);
            texture2D.LoadImage(data);
            var resultTexture = texture2D;

            //优化加载散图接口
            var ret = Texture2DOptimizationCreate(texture2D);
            if (ret != null)
            {
                resultTexture = ret;
                GameObject.Destroy(texture2D);
            }

            var sp = Sprite.Create(resultTexture, new Rect(0, 0, resultTexture.width, resultTexture.height), new Vector2(0.5f, 0.5f));
            return sp;
        }

        public void LoadSpriteAsync(string path,Action<Sprite> action, Action error = null, bool isFullPath = false)
        {
            Logger.Log("LoadSpriteAsync: " + path + "    " + isFullPath);
            var texturePath = path;
            if (!isFullPath && !path.StartsWith("http"))//http地址默认是全量地址
            {
                texturePath = GetWritablePath(path);
            }

            LoadByteFileAsync(texturePath,(byte[] data)=>{
                if (data == null)
                {
                    Logger.LogError("[ResourceLoader.LoadSprite() 加载sprite失败, path:" + path + "]");
                    error?.Invoke();
                    return;
                }
                var texture2D = new Texture2D(1, 1);
                texture2D.LoadImage(data);
                var resultTexture = texture2D;
                var sp = Sprite.Create(resultTexture, new Rect(0, 0, resultTexture.width, resultTexture.height), new Vector2(0.5f, 0.5f));
                action?.Invoke(sp);
            });
        }

        /// <summary>
        /// 异步加载文件 支持远端
        /// </summary>
        /// <param name="path"></param>
        /// <param name="action"></param>
        private void LoadByteFileAsync(string path,Action<byte[]> action) {
            var uri = new Uri(path);
            CoroutineWebRequest(uri.AbsoluteUri, (bytes) =>
            {
                action?.Invoke(bytes);
            });
        }


        public Texture2D Texture2DOptimizationCreate(Texture2D srcTexture2d)
        {
            var textureFormat = TextureFormat.RGBA4444;
            if (SystemInfo.SupportsTextureFormat(TextureFormat.ASTC_4x4))
            {
                Logger.Log("[UnityGame][纹理加载]：支持纹理压缩格式：TextureFormat.ASTC_RGBA_4x4");
                return UnityObjectHelper.CreateExternalTexture(srcTexture2d, TextureFormat.ASTC_4x4);
            }
            if (SystemInfo.SupportsTextureFormat(TextureFormat.ETC2_RGBA8))
            {
                Logger.Log("[UnityGame][纹理加载]：支持纹理压缩格式：TextureFormat.ETC2_RGBA8");
                return UnityObjectHelper.CreateExternalTexture(srcTexture2d, TextureFormat.ETC2_RGBA8);
            }
            string modelString = SystemInfo.deviceModel;
            Logger.Log($"[UnityGame][当前设备Model型号]：{modelString}");
            if (modelString.Equals("iPad3,4") || modelString.Equals("iPad3,5") || modelString.Equals("iPad3,6")  )
            {
                Logger.Log("[UnityGame][当前设备不使用纹理压缩格式]");
                return null;
            }
            Logger.Log("[UnityGame][纹理加载]：使用默认纹理压缩格式：TextureFormat.RGBA4444");
            return UnityObjectHelper.CreateExternalTexture(srcTexture2d, textureFormat);
        }

        public Texture2D LoadTexture2D(string path)
        {
            var texturePath = GetWritablePath(path);
            var data = LoadByteFile(texturePath);
            var texture2D = new Texture2D(1, 1);
            texture2D.LoadImage(data);

            return texture2D;
        }

        public byte[] LoadResData(string path)
        {            
            var texturePath = GetWritablePath(path);
            var data = LoadByteFile(texturePath);
            return data;
        }


        public Texture2D LoadTexture2DVariety(string path)
        {
            var texturePath = GetWritablePath(path);
            var data = LoadByteFile(texturePath);
            if (data == null)
            {
                return null;
            }
            var texture2D = new Texture2D(1, 1);
            texture2D.LoadImage(data);

            return texture2D;
        }
        
        public void Dispose()
        {
            if (loader != null)
            {
                loader.Dispose();
                loader = null;
            }

            if (loaderBehaviour != null)
            {
                UnityObjectHelper.DestroyGameObjectSafe(loaderBehaviour.gameObject);
                loaderBehaviour = null;
            }
            _instance = null;
        }

        #region CommonShareRes加载卸载相关
        

        public void PreLoadCommonRes(string commonResPath)
        {
            loader?.PreLoadCommonRes(commonResPath);
        }

        public void RemoveShareResource()
        {
            loader?.UnloadAllCommonRes();
        }

        #endregion

        #region 异步加载相关

        public void Update()
        {
            AssetBundleUtil.Update();
        }

        public void LoadObjectAsync(string fileName, Action<GameObject> action, string assetbundlePath = null)
        {
            LoadResWithPathAsync(fileName, typeof(GameObject), (UnityEngine.Object obj) => {
                action.Invoke(obj as GameObject);
            }, assetbundlePath);
        }

        public void LoadMaterialAsync(string fileName, Action<Material> action, string assetbundlePath = null)
        {
            LoadResWithPathAsync(fileName, typeof(Material), (UnityEngine.Object obj) => {
                action.Invoke(obj as Material);
            }, assetbundlePath);
        }

        ///异步 加载资源，带后缀名加载
        public void LoadResByFileNameAsync<T>(string fileName, Action<T> action, string assetbundlePath = null) where T : Object
        {
            LoadResWithPathAsync(fileName, typeof(UnityEngine.Object), (UnityEngine.Object obj)=> {
                action.Invoke(obj as T);
            }, assetbundlePath);
        }

        /// <summary>
        /// 根据ResType加载资源，不带后缀名加载 异步方式调用
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="type"></param>
        /// <param name="action"></param>
        /// <param name="assetbundlePath">ab包路径  可以不传 从缓存中去找可能为空</param>
        public void LoadResWithPathAsync(string fileName, Type type, Action<UnityEngine.Object> action, string assetbundlePath = null)
        {
            loader?.LoadResWithPathAsync(fileName, type, action, assetbundlePath);
        }

        /// <summary>
        /// 异步方式 加载ab包
        /// </summary>
        /// <param name="value"></param>
        /// <param name="action">加载成功回调</param>
        public void PrepareBundleAsync(List<string> value, Action<string, bool> action)
        {
            if (value == null || value.Count == 0)
                return;
            int count = value.Count;
            for (var i = 0; i < count; i++) {
                PrepareBundleAsync(value[i].ToString(), delegate (string path, bool result) {
                    if (result)
                        count--;
                    if (count <= 0 && action != null)
                    {
                        action.Invoke(path, result);
                        action = null;
                    }
                });
            }
            
        }

        public void PrepareBundleAsync(string bundlePath, Action<string, bool> action = null)
        {
            if(GameStart.Instance.UseAssetBundle)
                loader.PreLoadAssetAsync(bundlePath, action);
            else
                action?.Invoke("local", true);
        }

        public void InitManifestInfo(string path)
        {
            AssetBundleUtil.InitManifestInfo(path);
        }

        #endregion
    }
}
