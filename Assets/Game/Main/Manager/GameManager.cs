namespace Game
{
    public class GameManager : Singleton<GameManager>
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
            var gameView = new CatMainView();
            gameView.SetDisplayObject(ResourceLoader.Instance.CatLoadPrefab(CatConst.MainPageView));
            gameView.SetParent(WindowManager.Instance.GetUIRootByLayer(WindowLayer.Bottom));
            gameView.Show();
        }
    }
}