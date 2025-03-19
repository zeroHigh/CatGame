using UnityEngine;

namespace Game
{
    /// <summary>
    /// 资源加载逻辑，可自定义具体Loader(实现IResLoader接口即可)。通过SetLoader()设置即可
    /// </summary>
    public class ResourceLoader : ILSingleton<ResourceLoader>
    {

        protected class ResourceLoaderBehaviour : MonoBehaviour
        {
            void Awake()
            {
                DontDestroyOnLoad(this.gameObject);
            }
        }

        #region 常见对外接口

        public GameObject CatLoadPrefab(string fileName)
        {
            var prefab = Resources.Load<GameObject>(fileName);
            if (prefab != null)
            {
                return GameObject.Instantiate(prefab);
            }
            return null;
        }
        #endregion

    }
}
