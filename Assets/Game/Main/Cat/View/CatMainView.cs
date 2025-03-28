using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Game
{
    public class CatMainView : UIBaseView
    {
        private CatPointType.PointType _pointType;
        private string _pointPath;
        private Image _mainBg;
        private Text _textScore;
        private Button _btnBg;
        private Button _buttonLock;
        private Button _buttonUnlock;
        private Button _buttonHome;
        private Button _buttonSetting;
        private Transform _content;
        public int LastCount;


        protected override void ParseComponent()
        {
            _mainBg = Find<Image>("root/bg");
            _btnBg = Find<Button>("root/bg");
            _textScore = Find<Text>("root/score");
            _buttonLock = Find<Button>("root/btnLock");
            _buttonUnlock = Find<Button>("root/unlock");
            _buttonHome = Find<Button>("root/btnHome");
            _buttonSetting = Find<Button>("root/btnSetting");
            _content = Find("root/content").transform;
        }

        protected override void Refresh(params object[] arg)
        {
            _pointType = (CatPointType.PointType)arg[0];
            _pointPath = SharePathUtils.GetSkinPath(_pointType);
            InitPointBg();
            ChangeCount();
            CatGameManager.Instance.StartGame(this);
            CreatePoint();
        }

        /// <summary>
        /// 目标匹配背景
        /// </summary>
        private void InitPointBg()
        {
            switch (_pointType)
            {
                case CatPointType.PointType.BUTTERFLY:
                    _mainBg.sprite = ResourceLoader.Instance.CatLoadSprite("Common/Textures/bg/bg_2");
                    break;
                case CatPointType.PointType.FISH:
                    _mainBg.sprite = ResourceLoader.Instance.CatLoadSprite("Common/Textures/bg/bg_1");
                    break;
                case CatPointType.PointType.DIAN:
                    _mainBg.sprite = ResourceLoader.Instance.CatLoadSprite("Common/Textures/bg/bg_3");
                    break;
            }
        }

        public void ChangeCount()
        {
            LastCount = PlayerPrefs.GetInt(GlobalGameSetting.SettingsKey.COUNT_SETTINGS, 1);
        }

        public void UpdateScore(int score)
        {
            _textScore.text = score.ToString();
        }

        public void CreatePoint()
        {
            var skin = ResourceLoader.Instance.CatLoadPrefab(_pointPath);
            var catPoint = skin.transform.GetComponent<RectTransform>();
            catPoint.SetParent(_content, false);
            catPoint.localPosition = RandomPointIn();
            catPoint.localEulerAngles = Vector3.zero;
            catPoint.localScale = Vector3.one;

            var catPointView = new CatPointView();
            var id = CatGameManager.Instance.AddPoint(catPointView);
            catPointView.SetDisplayObject(skin);
            catPointView.Show(id);
        }

        private Vector2 RandomPointIn()
        {
            var startX = Random.Range(-500, 500);
            var startY = Random.Range(-200, 200);
            return new Vector2(startX, startY);
        }

        private void OnLockClick()
        {
            PlayCommonAudio(SharePathUtils.Audio.BtnClick);
            _buttonLock.gameObject.SetActive(false);
            _buttonUnlock.gameObject.SetActive(true);
            _buttonHome.gameObject.SetActive(true);
            _buttonSetting.gameObject.SetActive(true);
        }

        private void OnUnlockClick()
        {
            PlayCommonAudio(SharePathUtils.Audio.BtnClick);
            _buttonLock.gameObject.SetActive(true);
            _buttonUnlock.gameObject.SetActive(false);
            _buttonHome.gameObject.SetActive(false);
            _buttonSetting.gameObject.SetActive(false);
        }

        private void OnHomeClick()
        {
            PlayCommonAudio(SharePathUtils.Audio.BtnClick);

            // AdMobManager.Instance.LoadAdBanner();
            var gameView = new CatFirstView();
            gameView.SetDisplayObject(ResourceLoader.Instance.CatLoadPrefab(CatConst.FirstPageView));
            gameView.SetParent(WindowManager.Instance.GetUIRootByLayer(WindowLayer.Bottom));
            gameView.Show();
            CatGameManager.Instance.ExitGame();
            Dispose();
        }

        private void OnSettingClick()
        {
            PlayCommonAudio(SharePathUtils.Audio.BtnClick);
            var gameView = new CatSettingPage();
            gameView.SetDisplayObject(ResourceLoader.Instance.CatLoadPrefab(CatConst.SettingView));
            gameView.SetParent(WindowManager.Instance.GetUIRootByLayer(WindowLayer.Middle));
            gameView.Show();
        }

        private void OnBgClick()
        {
            if (_buttonLock.gameObject.activeSelf)
            {
                PlayCommonAudio(SharePathUtils.Audio.AudioMiss);
            }
            else
            {
                PlayCommonAudio(SharePathUtils.Audio.BtnClick);
                _buttonLock.gameObject.SetActive(true);
                _buttonUnlock.gameObject.SetActive(false);
                _buttonHome.gameObject.SetActive(false);
                _buttonSetting.gameObject.SetActive(false);
            }
        }

        protected override void AddEvent()
        {
            ListenButton(_buttonLock, OnLockClick);
            ListenButton(_buttonUnlock, OnUnlockClick);
            ListenButton(_buttonHome, OnHomeClick);
            ListenButton(_buttonSetting, OnSettingClick);
            ListenButton(_btnBg, OnBgClick);
        }

        protected override void RemoveEvent()
        {
            UnListenButton(_buttonLock, OnLockClick);
            UnListenButton(_buttonUnlock, OnUnlockClick);
            UnListenButton(_buttonHome, OnHomeClick);
            UnListenButton(_buttonSetting, OnSettingClick);
            UnListenButton(_btnBg, OnBgClick);
        }


    }
}