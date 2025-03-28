namespace Game
{
    public class GameManager : ILSingleton<GameManager>
    {
        public void StartGame()
        {
            StartRealGame();
        }

        private void StartRealGame()
        {
            CreateGameView();
        }

        private void CreateGameView()
        {
            AdMobManager.Instance.Init();
            // AdMobManager.Instance.LoadAdBanner();
            var gameView = new CatFirstView();
            gameView.SetDisplayObject(ResourceLoader.Instance.CatLoadPrefab(CatConst.FirstPageView));
            gameView.SetParent(WindowManager.Instance.GetUIRootByLayer(WindowLayer.Bottom));
            gameView.Show();
        }
    }
}