using System;
using UnityEngine;

namespace Game
{
    public class GameManager : ILSingleton<GameManager>
    {
        private string _currentAbRes;


        public void StartGame()
        {
            Debug.Log("StartGame!");
            StartRealGame();
        }

        private void StartRealGame()
        {
            _currentAbRes = "function";
            CreateGameView();
        }

        private void CreateGameView()
        {
            // AdMobManager.Instance.Init();
            // AdMobManager.Instance.LoadAdBanner();
            var gameView = new CatFirstView();
            Debug.Log("FirstPageView path = " + CatConst.FirstPageView);
            var asa = ResourceLoader.Instance.CatLoadPrefab(CatConst.FirstPageView);
            Debug.Log("FirstPageView obj = " + asa);
            gameView.SetDisplayObject(asa);
            gameView.SetParent(WindowManager.Instance.GetUIRootByLayer(WindowLayer.Bottom));
            gameView.Show();
        }

        private void LoadAssetBundle(string name, Action<bool> callback)
        {
            ResourceLoader.Instance.PrepareBundleAsync(name, delegate(string s, bool b)
            {
                callback.Invoke(b);
            });
        }

        private void UnLoadAssetBundle(string name)
        {
            ResourceLoader.Instance.UnloadPreBundle(name);
        }
    }
}