using System;

namespace Game
{
    public class GameManager : ILSingleton<GameManager>
    {
        private string _currentAbRes;


        public void StartGame()
        {
            // 数据请求等
            // CatDataManager.Instance.RequestSpeedCarInfo("Test", b =>
            // {
            //     // StartRealGame(ModuleType.SpeedCar);
            //     StartRealGame(ModuleType.AdventureIsland);
            // });

            StartRealGame();
        }

        private void StartRealGame()
        {
            if (GameStart.Instance.UseAssetBundle)
            {
                //加载对应游戏资源
                LoadAssetBundle(_currentAbRes, b =>
                {
                    CreateGameView();
                });
            }
            else
            {
                CreateGameView();
            }
        }

        private void CreateGameView()
        {
            var catLoadingView = new CatLoadingView();
            catLoadingView.SetDisplayObject(ResourceLoader.Instance.LoadObject(CatConst.LoadingView));
            var par = WindowManager.Instance.GetUIRootByLayer(WindowLayer.Bottom);
            catLoadingView.SetParent(par);
            catLoadingView.Show();
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