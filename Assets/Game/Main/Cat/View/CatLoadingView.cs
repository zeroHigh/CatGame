using UnityEngine.UI;

namespace Game
{
    public class CatLoadingView : UIBaseView
    {
        private Button _loading;

        protected override void ParseComponent()
        {
            _loading = Find<Button>("button");
        }

        private void OnLoadingClick()
        {
            var gameView = new CatFirstView();
            gameView.SetDisplayObject(ResourceLoader.Instance.LoadObject(CatConst.FirstPageView));
            gameView.SetParent(WindowManager.Instance.GetUIRootByLayer(WindowLayer.Bottom));
            gameView.Show();
            Dispose();
        }

        protected override void AddEvent()
        {
            ListenButton(_loading, OnLoadingClick);
        }

        protected override void RemoveEvent()
        {
            UnListenButton(_loading, OnLoadingClick);
        }

    }
}