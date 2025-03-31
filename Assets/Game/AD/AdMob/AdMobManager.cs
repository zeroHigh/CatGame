using System;
using GoogleMobileAds.Api;
using UnityEngine;

namespace Game
{
    public class AdMobManager : Singleton<AdMobManager>
    {
        /// <summary>
        /// 初始化 AdMob SDK
        /// </summary>
        public void Init()
        {
            MobileAds.Initialize(initStatus =>
            {
                Debug.Log("Admob SDK initialized. " + initStatus);
            });
        }

        #region Admob Banner
        private BannerView _bannerView;
        // These ad units are configured to always serve test ads.
#if UNITY_ANDROID
        private string _adUnitId = "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_IPHONE
        private string _adUnitId = "ca-app-pub-3940256099942544/2934735716";
#else
        private string _adUnitId = "unused";
#endif
        /// <summary>
        /// Creates a 320x50 banner view at top of the screen.
        /// </summary>
        private void CreateBannerView()
        {
            if (_bannerView != null)
            {
                DestroyAdBanner();
            }
            _bannerView = new BannerView(_adUnitId, AdSize.Banner, AdPosition.Bottom);
        }

        /// <summary>
        /// Creates the banner view and loads a banner ad.
        /// </summary>
        public void LoadAdBanner()
        {
            if(_bannerView == null)
            {
                CreateBannerView();
            }

            var adRequest = new AdRequest.Builder().Build();
            Debug.Log("Loading banner ad.");
            if (_bannerView != null)
                _bannerView.LoadAd(adRequest);
        }

        public void HideBanner()
        {
            if (_bannerView != null)
                _bannerView.Hide();
        }

        public void ShowBanner()
        {
            if (_bannerView != null)
                _bannerView.Show();
        }

        /// <summary>
        /// Destroys the banner view.
        /// </summary>
        private void DestroyAdBanner()
        {
            if (_bannerView != null)
            {
                Debug.Log("Destroying banner view.");
                _bannerView.Destroy();
                _bannerView = null;
            }
        }

        /// <summary>
        /// listen to events the banner view may raise.
        /// </summary>
        private void ListenToAdEvents()
        {

        }
        #endregion


        #region Admob Interstitial

        // These ad units are configured to always serve test ads.
#if UNITY_ANDROID
        private string _adUnitId2 = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE
        private string _adUnitId2 = "ca-app-pub-3940256099942544/4411468910";
#else
        private string _adUnitId2 = "unused";
#endif

        private InterstitialAd _interstitialAd;

        /// <summary>
        /// Loads the interstitial ad.
        /// </summary>
        public void LoadInterstitialAd()
        {
            // Clean up the old ad before loading a new one.
            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }

            Debug.Log("Loading the interstitial ad.");
            // 初始化插页式广告
            _interstitialAd = new InterstitialAd(_adUnitId2);
            // create our request used to load the ad.
            var adRequest = new AdRequest.Builder().Build();
            // send the request to load the ad.
            _interstitialAd.LoadAd(adRequest);
            // 广告加载完成事件
            _interstitialAd.OnAdLoaded += HandleOnAdLoaded;
        }

        void HandleOnAdLoaded(object sender, EventArgs args)
        {
            // 广告加载完成后显示
            if (_interstitialAd.IsLoaded()) {
                _interstitialAd.Show();
            }
        }
        #endregion

        #region 激励视频
        private RewardedAd rewardedAd;
        public void RequestRewardedAd()
        {
            // 替换为您的广告单元ID
            string adUnitId = "ca-app-pub-3940256099942544/5224354917";

            rewardedAd = new RewardedAd(adUnitId);

            // 创建请求
            AdRequest request = new AdRequest.Builder().Build();

            // 加载广告
            rewardedAd.LoadAd(request);

            // 设置事件处理
            rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        }

        void HandleUserEarnedReward(object sender, Reward args)
        {
            // 用户完成观看奖励视频
            string type = args.Type;
            double amount = args.Amount;
            // 给予用户奖励
        }
        #endregion
    }
}