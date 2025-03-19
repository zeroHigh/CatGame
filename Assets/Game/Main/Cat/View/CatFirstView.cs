using UnityEngine.UI;

namespace Game
{
    public class CatFirstView : UIBaseView
    {
        private Button _btnLeft;
        private Button _btnRight;
        private Button _btnSetting;
        private Button _btnLevel1;
        private Button _btnLevel2;
        private Button _btnLevel3;


        protected override void ParseComponent()
        {
            _btnLeft = Find<Button>("root/btnLeft");
            _btnRight = Find<Button>("root/btnRight");
            _btnSetting = Find<Button>("root/btnSetting");

            _btnLevel1 = Find<Button>("root/content/level1");
            _btnLevel2 = Find<Button>("root/content/level2");
            _btnLevel3 = Find<Button>("root/content/level3");
        }

        private void OnLevel1Click()
        {
            _btnLevel1.interactable = false;
            GoMainPageView(CatPointType.PointType.BUTTERFLY);
        }

        private void OnLevel2Click()
        {
            _btnLevel2.interactable = false;
            GoMainPageView(CatPointType.PointType.FISH);
        }

        private void OnLevel3Click()
        {
            _btnLevel3.interactable = false;
            GoMainPageView(CatPointType.PointType.DIAN);
        }

        private void GoMainPageView(CatPointType.PointType type)
        {
            PlayCommonAudio(SharePathUtils.Audio.BtnClick);
            AdMobManager.Instance.HideBanner();
            var gameView = new CatMainView();
            gameView.SetDisplayObject(ResourceLoader.Instance.CatLoadPrefab(CatConst.MainPageView));
            gameView.SetParent(WindowManager.Instance.GetUIRootByLayer(WindowLayer.Bottom));
            gameView.Show(type);
            Dispose();
        }

        private void OnLeftClick()
        {
            PlayCommonAudio(SharePathUtils.Audio.BtnClick);
        }

        private void OnRightClick()
        {
            PlayCommonAudio(SharePathUtils.Audio.BtnClick);
        }

        private void OnSettingClick()
        {
            PlayCommonAudio(SharePathUtils.Audio.BtnClick);
            var gameView = new CatSettingPage();
            gameView.SetDisplayObject(ResourceLoader.Instance.CatLoadPrefab(CatConst.SettingView));
            gameView.SetParent(WindowManager.Instance.GetUIRootByLayer(WindowLayer.Middle));
            gameView.Show();
        }

        protected override void AddEvent()
        {
            ListenButton(_btnLeft, OnLeftClick);
            ListenButton(_btnRight, OnRightClick);
            ListenButton(_btnSetting, OnSettingClick);

            ListenButton(_btnLevel1, OnLevel1Click);
            ListenButton(_btnLevel2, OnLevel2Click);
            ListenButton(_btnLevel3, OnLevel3Click);
        }



        protected override void RemoveEvent()
        {
            UnListenButton(_btnLeft, OnLeftClick);
            UnListenButton(_btnRight, OnRightClick);
            UnListenButton(_btnSetting, OnSettingClick);

            UnListenButton(_btnLevel1, OnLeftClick);
            UnListenButton(_btnLevel2, OnLeftClick);
            UnListenButton(_btnLevel3, OnLeftClick);
        }
    }
}