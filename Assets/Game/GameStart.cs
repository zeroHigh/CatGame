using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class GameStart : MonoBehaviour
    {
        public static string AbRoot { get; private set; }
        public static string WritablePath { get; private set; }

        public static GameStart Instance;

        public void Awake()
        {
            Instance = this;
            AbRoot = Application.streamingAssetsPath + "/";
            WindowManager.Instance.Init(transform);
            AudioManager.Instance.Init(gameObject);
            WindowManager.Instance.AdjustScreenFit();
            DontDestroyOnLoad(gameObject);
            InitLoader();
#if !UNITY_EDITOR
            WritablePath = Application.streamingAssetsPath + "/";
#else
            WritablePath = Application.dataPath.Replace("Assets", string.Empty);
#endif
            StartCoroutine(OnGameSwordStart());
        }

        private void InitLoader()
        {
            BaseLoader loader = new DefaultResLoader();
            ResourceLoader.Instance.Init(loader);
            ResourceLoader.Instance.writablePath = WritablePath;
            ResourceLoader.Instance.AddSearchPath("Assets/");
            ResourceLoader.Instance.AddSearchPath("Assets/Common");
            ResourceLoader.Instance.AddSearchPath("Assets/Function");
        }

        private void StartLoad()
        {
            Debug.Log("StartLoad!");
            StartCoroutine(LoadData());
        }

        private IEnumerator LoadData()
        {
            Debug.Log("LoadData!");
            var isLoadCommon = false;
            LoadAssetBundle("common", delegate(bool b)
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
            AudioManager.Instance.Update();
        }

        public void OnDestroy()
        {
            Logger.LogWarning("[GameWorld.OnDestroy() => OnDestroy called....]");
            AudioManager.Instance.ReleaseAllAudioClips();
            CatGameManager.Instance.ExitGame();
        }
    }
}
