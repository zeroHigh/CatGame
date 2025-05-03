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
        private Button _buttonStart;
        private Transform _content;
        private int _ballIndex;


        protected override void ParseComponent()
        {
            _mainBg = Find<Image>("root/bg");
            _btnBg = Find<Button>("root/bg");
            _textScore = Find<Text>("root/score");
            _buttonStart = Find<Button>("root/content/tapStart");
            _content = Find("root/content").transform;
        }

        protected override void Refresh(params object[] arg)
        {

        }

        public void PutNewBall()
        {
            CreateBall(CatPointType.PointType.BALL, _content);
        }

        private void CreateBall(CatPointType.PointType type, Transform parent)
        {
            _ballIndex++;
            var path = SharePathUtils.GetSkinPath(type);
            var pointObj = ResourceLoader.Instance.CatLoadPrefab(path);
            pointObj.name = _ballIndex + "_0";
            var catPoint = pointObj.transform.GetComponent<RectTransform>();
            catPoint.SetParent(parent, false);
            catPoint.anchoredPosition = new Vector3(100f, 200f);
            catPoint.localEulerAngles = Vector3.zero;
            catPoint.localScale = Vector3.one;
            CatGameManager.Instance.LastBallNum += 8;
        }

        public void UpdateScore(int score)
        {
            _textScore.text = score.ToString();
        }

        private void OnBtnStartClick()
        {
            PlayCommonAudio(SharePathUtils.Audio.BtnClick);
            _buttonStart.gameObject.SetActive(false);
            CreateBall(CatPointType.PointType.BALL, _content);
            CatGameManager.Instance.StartGame(this);
        }

        private void OnBgClick()
        {
            PlayCommonAudio(SharePathUtils.Audio.AudioMiss);
        }

        protected override void AddEvent()
        {
            ListenButton(_buttonStart, OnBtnStartClick);
            ListenButton(_btnBg, OnBgClick);
        }

        protected override void RemoveEvent()
        {
            UnListenButton(_buttonStart, OnBtnStartClick);
            UnListenButton(_btnBg, OnBgClick);
        }


    }
}