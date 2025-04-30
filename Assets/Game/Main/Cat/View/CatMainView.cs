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
        private GameObject _ball;


        protected override void ParseComponent()
        {
            _mainBg = Find<Image>("root/bg");
            _btnBg = Find<Button>("root/bg");
            _textScore = Find<Text>("root/score");
            _buttonStart = Find<Button>("root/content/tapStart");
            _content = Find("root/content").transform;
            _ball = Find("root/content/ball");
            _ball.SetActive(false);
        }

        protected override void Refresh(params object[] arg)
        {

        }

        public void UpdateScore(int score)
        {
            _textScore.text = score.ToString();
        }

        private void OnBtnStartClick()
        {
            PlayCommonAudio(SharePathUtils.Audio.BtnClick);
            _buttonStart.gameObject.SetActive(false);
            _ball.SetActive(true);
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