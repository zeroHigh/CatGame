using UnityEngine;
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

            CreatePoint(CatPointType.PointType.BUTTERFLY, _btnLevel1.transform);
            CreatePoint(CatPointType.PointType.FISH, _btnLevel2.transform);
            CreatePoint(CatPointType.PointType.DIAN, _btnLevel3.transform);
        }

        private void CreatePoint(CatPointType.PointType type, Transform parent)
        {
            var path = SharePathUtils.GetSkinPath(type);
            var pointObj = ResourceLoader.Instance.CatLoadPrefab(path);
            var catPoint = pointObj.transform.GetComponent<RectTransform>();
            catPoint.SetParent(parent, false);
            catPoint.localPosition = Vector3.zero;
            catPoint.localEulerAngles = Vector3.zero;
            catPoint.localScale = Vector3.one;

            var point = pointObj.transform.Find("point");
            point.gameObject.SetActive(true);
            point.gameObject.GetComponent<Animator>().speed = 0.3f;
            point.gameObject.GetComponent<Image>().raycastTarget = false;
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
            var gameView = new CatMainView2();
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