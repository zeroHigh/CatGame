using System;
using Game.Main.Cat;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class CatMainView : UIBaseView
    {
        private CatPointType.PointType _pointType;
        private Text _textScore;
        private ButtonExtension _buttonLock;
        private Button _buttonUnlock;
        private Button _buttonHome;
        private Button _buttonSetting;
        private Transform _content;



        protected override void ParseComponent()
        {
            _buttonLock = Find<ButtonExtension>("root/btnLock");
            _buttonUnlock = Find<Button>("root/unlock");
            _buttonHome = Find<Button>("root/btnHome");
            _buttonSetting = Find<Button>("root/btnSetting");
            _content = Find("root/content").transform;
        }

        protected override void Refresh(params object[] arg)
        {
            _pointType = (CatPointType.PointType)arg[0];
            var path = SharePathUtils.GetSkinPath(_pointType);
            var skin = ResourceLoader.Instance.LoadObject(path);
            var tt = skin.transform.GetComponent<RectTransform>();
            tt.SetParent(_content, false);
            tt.localPosition = Vector2.zero;
            tt.localEulerAngles = Vector3.zero;
            tt.localScale = Vector3.one;
        }

        private void OnLockClick()
        {
            _buttonLock.gameObject.SetActive(false);
            _buttonUnlock.gameObject.SetActive(true);
            _buttonHome.gameObject.SetActive(true);
            _buttonSetting.gameObject.SetActive(true);
        }

        private void OnUnlockClick()
        {
            _buttonLock.gameObject.SetActive(true);
            _buttonUnlock.gameObject.SetActive(false);
            _buttonHome.gameObject.SetActive(false);
            _buttonSetting.gameObject.SetActive(false);
        }

        private void OnHomeClick()
        {
            var gameView = new CatFirstView();
            gameView.SetDisplayObject(ResourceLoader.Instance.LoadObject(CatConst.FirstPageView));
            gameView.SetParent(WindowManager.Instance.GetUIRootByLayer(WindowLayer.Bottom));
            gameView.Show();
            Dispose();
        }

        private void OnSettingClick()
        {
            var gameView = new CatSettingPage();
            gameView.SetDisplayObject(ResourceLoader.Instance.LoadObject(CatConst.SettingView));
            gameView.SetParent(WindowManager.Instance.GetUIRootByLayer(WindowLayer.Middle));
            gameView.Show();
        }

        protected override void AddEvent()
        {
            _buttonLock.onLongPress.AddListener(OnLockClick);
            ListenButton(_buttonUnlock, OnUnlockClick);
            ListenButton(_buttonHome, OnHomeClick);
            ListenButton(_buttonSetting, OnSettingClick);
        }

        protected override void RemoveEvent()
        {
            _buttonLock.onLongPress.RemoveAllListeners();
            UnListenButton(_buttonUnlock, OnUnlockClick);
            UnListenButton(_buttonHome, OnHomeClick);
            UnListenButton(_buttonSetting, OnSettingClick);
        }


    }
}