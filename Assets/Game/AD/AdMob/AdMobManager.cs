// using GoogleMobileAds.Api;
using UnityEngine;

namespace Game
{
    public class AdMobManager : ILSingleton<AdMobManager>
    {
//         /// <summary>
//         /// 初始化 AdMob SDK
//         /// </summary>
//         public void Init()
//         {
//             MobileAds.Initialize(initStatus =>
//             {
//                 Debug.Log("Admob SDK initialized. " + initStatus);
//             });
//         }
//
//         #region Admob Banner
//         private BannerView _bannerView;
//         // These ad units are configured to always serve test ads.
// #if UNITY_ANDROID
//         private string _adUnitId = "ca-app-pub-3940256099942544/6300978111";
// #elif UNITY_IPHONE
//         private string _adUnitId = "ca-app-pub-3940256099942544/2934735716";
// #else
//         private string _adUnitId = "unused";
// #endif
//         /// <summary>
//         /// Creates a 320x50 banner view at top of the screen.
//         /// </summary>
//         private void CreateBannerView()
//         {
//             if (_bannerView != null)
//             {
//                 DestroyAdBanner();
//             }
//             _bannerView = new BannerView(_adUnitId, AdSize.Banner, AdPosition.Bottom);
//         }
//
//         /// <summary>
//         /// Creates the banner view and loads a banner ad.
//         /// </summary>
//         public void LoadAdBanner()
//         {
//             if(_bannerView == null)
//             {
//                 CreateBannerView();
//             }
//
//             var adRequest = new AdRequest();
//             Debug.Log("Loading banner ad.");
//             if (_bannerView != null)
//                 _bannerView.LoadAd(adRequest);
//         }
//
//         public void HideBanner()
//         {
//             if (_bannerView != null)
//                 _bannerView.Hide();
//         }
//
//         public void ShowBanner()
//         {
//             if (_bannerView != null)
//                 _bannerView.Show();
//         }
//
//         /// <summary>
//         /// Destroys the banner view.
//         /// </summary>
//         private void DestroyAdBanner()
//         {
//             if (_bannerView != null)
//             {
//                 Debug.Log("Destroying banner view.");
//                 _bannerView.Destroy();
//                 _bannerView = null;
//             }
//         }
//
//         /// <summary>
//         /// listen to events the banner view may raise.
//         /// </summary>
//         private void ListenToAdEvents()
//         {
//             // Raised when an ad is loaded into the banner view.
//             _bannerView.OnBannerAdLoaded += () =>
//             {
//                 Debug.Log("Banner view loaded an ad with response : "
//                           + _bannerView.GetResponseInfo());
//             };
//             // Raised when an ad fails to load into the banner view.
//             _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
//             {
//                 Debug.LogError("Banner view failed to load an ad with error : "
//                                + error);
//             };
//             // Raised when the ad is estimated to have earned money.
//             _bannerView.OnAdPaid += (AdValue adValue) =>
//             {
//                 Debug.Log(string.Format("Banner view paid {0} {1}.",
//                     adValue.Value,
//                     adValue.CurrencyCode));
//             };
//             // Raised when an impression is recorded for an ad.
//             _bannerView.OnAdImpressionRecorded += () =>
//             {
//                 Debug.Log("Banner view recorded an impression.");
//             };
//             // Raised when a click is recorded for an ad.
//             _bannerView.OnAdClicked += () =>
//             {
//                 Debug.Log("Banner view was clicked.");
//             };
//             // Raised when an ad opened full screen content.
//             _bannerView.OnAdFullScreenContentOpened += () =>
//             {
//                 Debug.Log("Banner view full screen content opened.");
//             };
//             // Raised when the ad closed full screen content.
//             _bannerView.OnAdFullScreenContentClosed += () =>
//             {
//                 Debug.Log("Banner view full screen content closed.");
//             };
//         }
//         #endregion
//
//
//         #region Admob Interstitial
//
//         // These ad units are configured to always serve test ads.
// #if UNITY_ANDROID
//         private string _adUnitId2 = "ca-app-pub-3940256099942544/1033173712";
// #elif UNITY_IPHONE
//         private string _adUnitId2 = "ca-app-pub-3940256099942544/4411468910";
// #else
//         private string _adUnitId2 = "unused";
// #endif
//
//         private InterstitialAd _interstitialAd;
//
//         /// <summary>
//         /// Loads the interstitial ad.
//         /// </summary>
//         public void LoadInterstitialAd()
//         {
//             // Clean up the old ad before loading a new one.
//             if (_interstitialAd != null)
//             {
//                 _interstitialAd.Destroy();
//                 _interstitialAd = null;
//             }
//
//             Debug.Log("Loading the interstitial ad.");
//
//             // create our request used to load the ad.
//             var adRequest = new AdRequest();
//
//             // send the request to load the ad.
//             InterstitialAd.Load(_adUnitId, adRequest,
//                 (InterstitialAd ad, LoadAdError error) =>
//                 {
//                     // if error is not null, the load request failed.
//                     if (error != null || ad == null)
//                     {
//                         Debug.LogError("interstitial ad failed to load an ad " +
//                                        "with error : " + error);
//                         return;
//                     }
//
//                     Debug.Log("Interstitial ad loaded with response : "
//                               + ad.GetResponseInfo());
//
//                     _interstitialAd = ad;
//                 });
//         }
//
//         /// <summary>
//         /// Shows the interstitial ad.
//         /// </summary>
//         public void ShowInterstitialAd()
//         {
//             if (_interstitialAd != null && _interstitialAd.CanShowAd())
//             {
//                 Debug.Log("Showing interstitial ad.");
//                 _interstitialAd.Show();
//             }
//             else
//             {
//                 Debug.LogError("Interstitial ad is not ready yet.");
//             }
//         }
//
//         private void RegisterEventHandlers(InterstitialAd interstitialAd)
//         {
//             // Raised when the ad is estimated to have earned money.
//             interstitialAd.OnAdPaid += (AdValue adValue) =>
//             {
//                 Debug.Log(string.Format("Interstitial ad paid {0} {1}.",
//                     adValue.Value,
//                     adValue.CurrencyCode));
//             };
//             // Raised when an impression is recorded for an ad.
//             interstitialAd.OnAdImpressionRecorded += () =>
//             {
//                 Debug.Log("Interstitial ad recorded an impression.");
//             };
//             // Raised when a click is recorded for an ad.
//             interstitialAd.OnAdClicked += () =>
//             {
//                 Debug.Log("Interstitial ad was clicked.");
//             };
//             // Raised when an ad opened full screen content.
//             interstitialAd.OnAdFullScreenContentOpened += () =>
//             {
//                 Debug.Log("Interstitial ad full screen content opened.");
//             };
//             // Raised when the ad closed full screen content.
//             interstitialAd.OnAdFullScreenContentClosed += () =>
//             {
//                 Debug.Log("Interstitial ad full screen content closed.");
//             };
//             // Raised when the ad failed to open full screen content.
//             interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
//             {
//                 Debug.LogError("Interstitial ad failed to open full screen content " +
//                                "with error : " + error);
//             };
//         }
//         #endregion
    }
}