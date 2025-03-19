using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class GameStart : MonoBehaviour
    {
        public static string AbRoot { get; private set; }
        public static GameStart Instance;

        public void Awake()
        {
            Instance = this;
            AbRoot = Application.streamingAssetsPath + "/";
            WindowManager.Instance.Init(transform);
            AudioManager.Instance.Init(gameObject);
            WindowManager.Instance.AdjustScreenFit();
            DontDestroyOnLoad(gameObject);
            StartCoroutine(OnGameSwordStart());
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
