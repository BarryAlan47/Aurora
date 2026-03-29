using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using DG.Tweening;
using System;
using Sirenix.OdinInspector;

/// <summary>
/// 广告管理器：封装 Banner、插屏与激励视频广告逻辑。
/// </summary>
public class MyAdManager : MonoBehaviour
{
    [LabelText("应用 ID（AppId）")]
    private string appId = "";

    [LabelText("Banner 广告位 ID")]
    private string bannerId = "";

    [LabelText("插屏广告位 ID")]
    private string intertestialId = "";

    [LabelText("激励视频广告位 ID")]
    private string rewardId = "";

    [LabelText("插屏广告实例")]
    private InterstitialAd interstitial;

    [LabelText("Banner 广告实例")]
    private BannerView bannerView;

    [LabelText("激励视频广告实例")]
    private RewardBasedVideoAd rewardBasedVideoAd;

    [HideInInspector]
    [LabelText("广告管理器单例")]
    public static MyAdManager Instance;

    /// <summary>
    /// 初始化单例并保持跨场景不销毁。
    /// </summary>
    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 初始化广告 SDK 并请求各类广告。
    /// </summary>
    public void Start()
    {

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(appId);

        this.RequestBanner();

        this.RequestInterstitial();
        RequestRewardBasedVideo();

        rewardBasedVideoAd.OnAdRewarded += HandleRewardBasedVideoRewarded;
        rewardBasedVideoAd.OnAdClosed += HandleRewardBasedVideoClosed;
    }

    /// <summary>
    /// 请求 Banner 广告。
    /// </summary>
    private void RequestBanner()
    {
        // Create a 320x50 banner at the top of the screen.
        this.bannerView = new BannerView(bannerId, AdSize.Banner, AdPosition.Bottom);

        AdRequest request = new AdRequest.Builder().Build();

        // Load the banner with the request.
        this.bannerView.LoadAd(request);
    }

    /// <summary>
    /// 请求插屏广告。
    /// </summary>
    private void RequestInterstitial()
    {
        // Initialize an InterstitialAd.
        this.interstitial = new InterstitialAd(intertestialId);
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the interstitial with the request.
        this.interstitial.LoadAd(request);
    }
    /// <summary>
    /// 展示插屏广告，未加载则重新请求。
    /// </summary>
    public void ShowInterstitialAd()
    {
        if (this.interstitial.IsLoaded())
        {
            this.interstitial.Show();
        }
        else
        {
            this.RequestInterstitial();
        }
    }

    /// <summary>
    /// 展示激励视频广告并在结束后重新请求。
    /// </summary>
    public void ShowRewardVideo()
    {
        print("RewardAds");
        if (rewardBasedVideoAd.IsLoaded())
        {
            rewardBasedVideoAd.Show();
        }

        RequestRewardBasedVideo();
    }

    /// <summary>
    /// 请求激励视频广告。
    /// </summary>
    private void RequestRewardBasedVideo()
    {
        //Video Ad Events
        this.rewardBasedVideoAd = RewardBasedVideoAd.Instance;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded ad with the request.
        this.rewardBasedVideoAd.LoadAd(request, rewardId);
    }

    /// <summary>
    /// 激励视频关闭回调：重新请求下一次广告。
    /// </summary>
    public void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    {
        RequestRewardBasedVideo();
    }

    /// <summary>
    /// 激励视频观看完成回调：发放金币奖励。
    /// </summary>
    public void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        FindObjectOfType<GameManager>().AddMoney(50);
    }
}



