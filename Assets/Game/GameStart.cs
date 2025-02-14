using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class GameStart : MonoBehaviour
    {
        [Tooltip("是否用AB模型跑游戏")]
        public bool UseAssetBundle;
        public static string AbRoot { get; private set; }
        public static string WritablePath { get; private set; }

        public static GameStart Instance;

        public void Awake()
        {
            Instance = this;

            WindowManager.Instance.Init(transform);
            InitLoader();
            // ResUtils.Instance.SetScreenRotation();
            WindowManager.Instance.AdjustScreenFit();
            DontDestroyOnLoad(gameObject);
            AbRoot = Application.streamingAssetsPath + "/";

#if !UNITY_EDITOR
            WritablePath = Application.streamingAssetsPath + "/";
#else
            UseAssetBundle = false;
            if (UseAssetBundle)
                WritablePath = Application.streamingAssetsPath + "/";
            else
                WritablePath = Application.dataPath.Replace("Assets", string.Empty);
#endif

            if (UseAssetBundle)
            {
                StartLoad();
            }
            else
            {
                StartCoroutine(OnGameSwordStart());
            }
        }

        private void InitLoader()
        {
            BaseLoader loader;
            if (UseAssetBundle)
                loader = new AssetBundleLoader(AbRoot);
            else
                loader = new DefaultResLoader();
            ResourceLoader.Instance.Init(loader);
            ResourceLoader.Instance.writablePath = WritablePath;
            ResourceLoader.Instance.AddSearchPath("Assets/");
            ResourceLoader.Instance.AddSearchPath("Assets/CommonShare");
        }

        private void StartLoad()
        {
            StartCoroutine(LoadData());
        }

        private IEnumerator LoadData()
        {
            var isLoadCommon = false;
            LoadAssetBundle("commonshare", delegate(bool b)
            {
                isLoadCommon = b;
            });

            yield return new WaitUntil(() => isLoadCommon);

            yield return OnGameSwordStart();
        }

        private void LoadAssetBundle(string name, Action<bool> callback)
        {
            ResourceLoader.Instance.PrepareBundleAsync(name, delegate(string s, bool b)
            {
                callback.Invoke(b);
            });
        }

        private IEnumerator OnGameSwordStart()
        {
            yield return new WaitForEndOfFrame();
            Canvas.ForceUpdateCanvases();
            yield return new WaitForEndOfFrame();

            GameManager.Instance.StartGame();
        }

        public void Update()
        {
            if(UseAssetBundle)
                ResourceLoader.Instance.Update();
        }

        public void OnDestroy()
        {
            Logger.LogWarning("[GameWorld.OnDestroy() => OnDestroy called....]");
            if (UseAssetBundle)
            {
                var bundler = new List<string>
                {
                    "commonshare"
                };
                ResourceLoader.Instance.UnloadPreBundle(bundler);
            }
        }

        public void OnApplicationPause(bool pause)
        {
            //TODO 暂停
        }

        public void OnApplicationFocus(bool focus)
        {
            if (!Global.IsMobile)
                return;
            // TODO 继续
        }
    }
}
