using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    /// <summary>
    /// 资源类型
    /// </summary>
    public enum ResType
    {
        Prefab,
        Atlas,
        Video,
        Bytes,
        Asset,
        Material,
        Audio,
        Txt,
    }

    public abstract class BaseLoader
    {
        public virtual void Init()
        {
        }

        protected readonly List<string> searchPaths = new List<string>();
        /// 资源名对应路径
        protected readonly Dictionary<string, string> resName2Path = new Dictionary<string, string>();
        public void AddSearchPath(string path)
        {
            if (searchPaths.Contains(path))
                return;
            searchPaths.Add(path);
        }
        public void AddSearchPath(List<string> paths)
        {
            searchPaths.AddRange(paths);
        }

        public void RemoveSearchPath(string path)
        {
            if (searchPaths.Contains(path))
                searchPaths.Remove(path);
        }

        public void RemoveSearchPath(List<string> paths)
        {
            if (paths == null || paths.Count == 0)
                return;
            for (var i = 0; i < paths.Count; i++)
                RemoveSearchPath(paths[i]);
        }

        public void RemoveAllSearchPath()
        {
            searchPaths.Clear();
            resName2Path.Clear();
        }

        /// 载入场景
        public AsyncOperation LoadSceneAsync(string sceneName, LoadSceneMode loadMode)
        {
            return SceneManager.LoadSceneAsync(sceneName, loadMode);
        }

        /// 销毁场景
        public AsyncOperation UnloadSceneAsync(string sceneName)
        {
            return SceneManager.UnloadSceneAsync(sceneName);
        }

        /// 根据资源名加载文件(不带路径)
        public T LoadResByFileName<T>(string path) where T : Object
        {
            return OnLoadResByFileName<T>(path);
        }

        protected abstract T OnLoadResByFileName<T>(string path) where T : Object;

        /// 根据资源类型加载资源(带路径)
        public T LoadResWithPath<T>(string path, ResType type) where T : Object
        {
            return OnLoadResWithPath<T>(path, type);
        }

        protected abstract T OnLoadResWithPath<T>(string path, ResType type) where T : Object;

        /// 卸载一个资源
        public virtual void UnloadRes(string path, bool isForce = false)
        {
            
        }

        public virtual void UnloadUnusedRes()
        {
            
        }

        public virtual void PreLoadAsset(string path)
        {
            
        }

        #region 图集

        public abstract void PreLoadAtlas(string atlasPath);

        public abstract void UnloadAtlas(string atlasPath);
        
        public abstract Sprite LoadSprite(string imgPath);

        #endregion
        
        public virtual void ClearAllRes()
        {
            
        }

        protected virtual void OnDispose()
        {
            
        }

        public void Dispose()
        {
            OnDispose();
            searchPaths?.Clear();
            resName2Path?.Clear();
        }

        #region 公共资源加载卸载

        public virtual void PreLoadCommonRes(string resPath)
        {
            
        }

        public virtual void UnloadAllCommonRes()
        {
            
            
        }



        #endregion
		
        protected abstract void OnLoadResWithPathAsync(string path, System.Type type, System.Action<UnityEngine.Object> action, string assetbundlePath = null);

		 /// 根据资源类型加载资源(带路径) 异步方式
        public void LoadResWithPathAsync(string path, System.Type type, System.Action<UnityEngine.Object> action, string assetbundlePath = null)
        {
            OnLoadResWithPathAsync(path, type,action,assetbundlePath);
        }

        public virtual void PreLoadAssetAsync(string path, System.Action<string, bool> action = null)
        {

        }
    }
}
