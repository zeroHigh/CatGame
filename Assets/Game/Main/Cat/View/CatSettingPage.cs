using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class CatSettingPage : UIBaseView
    {
        private Button _btnClose;
        private Button _btnShadow;
        private Slider _sliderCount;
        private Slider _sliderSpeed;
        private Text _textCount;
        private Text _textSpeed;

        protected override void ParseComponent()
        {
            _btnClose = Find<Button>("bg/btnClose");
            _btnShadow = Find<Button>("shadow");

            _sliderCount = Find<Slider>("bg/countParent/Slider");
            _sliderSpeed = Find<Slider>("bg/speedParent/Slider");

            _textCount = Find<Text>("bg/countParent/num");
            _textSpeed = Find<Text>("bg/speedParent/num");
        }

        protected override void Refresh(params object[] arg)
        {
            _sliderCount.onValueChanged.AddListener((s) =>
            {
                _textCount.text = s.ToString(CultureInfo.InvariantCulture);
            });

            _sliderSpeed.onValueChanged.AddListener((s) =>
            {
                _textSpeed.text = s.ToString(CultureInfo.InvariantCulture);
            });
            var lastCount = PlayerPrefs.GetInt(GlobalGameSetting.SettingsKey.COUNT_SETTINGS, 1);
            var lastSpeed = PlayerPrefs.GetInt(GlobalGameSetting.SettingsKey.SPEED_SETTINGS, 1);
            _sliderCount.value = lastCount;
            _sliderSpeed.value = lastSpeed;
            _textCount.text = lastCount.ToString();
            _textSpeed.text = lastSpeed.ToString();
        }

        protected override void AddEvent()
        {
            ListenButton(_btnClose, OnCloseClick);
            ListenButton(_btnShadow, OnCloseClick);
        }

        protected override void RemoveEvent()
        {
            UnListenButton(_btnClose, OnCloseClick);
            UnListenButton(_btnShadow, OnCloseClick);

            _sliderCount.onValueChanged.RemoveAllListeners();
            _sliderSpeed.onValueChanged.RemoveAllListeners();
        }


        private void OnCloseClick()
        {
            PlayerPrefs.SetInt(GlobalGameSetting.SettingsKey.COUNT_SETTINGS, (int)_sliderCount.value);
            PlayerPrefs.SetInt(GlobalGameSetting.SettingsKey.SPEED_SETTINGS, (int)_sliderSpeed.value);
            Dispose();
        }
    }
}