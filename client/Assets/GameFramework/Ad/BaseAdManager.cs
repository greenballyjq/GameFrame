using System;
using GameFramework.Game;
using UnityEngine;

namespace GameFramework.Ad
{
    /// <summary>
    /// 广告管理器抽象基类 - 提供通用的广告展示逻辑
    /// 各游戏重写 GetAdId 和 GetUserId 以提供游戏特定的广告配置和用户标识
    /// </summary>
    public abstract class BaseAdManager : BaseGameManager
    {
        protected PlatformManager PlatformManager { get; private set; }

        /// <summary>
        /// 根据广告场景获取广告 ID
        /// </summary>
        /// <param name="adScenario">广告场景标识</param>
        /// <returns>平台广告 ID</returns>
        protected abstract string GetAdId(string adScenario);

        /// <summary>
        /// 获取当前用户 ID（用于激励广告的服务端验证）
        /// </summary>
        protected abstract string GetUserId();

        /// <summary>
        /// 初始化 PlatformManager，子类应在 Init() 中调用 base.InitPlatformManager()
        /// 或自行设置 PlatformManager
        /// </summary>
        protected void InitPlatformManager(PlatformManager platformManager)
        {
            PlatformManager = platformManager;
        }

        /// <summary>
        /// 展示激励广告
        /// </summary>
        /// <param name="adScenario">广告场景标识</param>
        /// <param name="onClose">广告关闭回调</param>
        public void ShowRewardAd(string adScenario, Action<AdResult> onClose)
        {
            string adId = GetAdId(adScenario);
            string userId = GetUserId();
            PlatformManager.CurrentPlatform.ShowRewardAd(adId, userId, OnAdSuccess, onClose, OnAdFail);
        }

        /// <summary>
        /// 展示指定类型的广告
        /// </summary>
        /// <param name="adType">广告类型</param>
        /// <param name="adScenario">广告场景标识</param>
        /// <param name="onClose">广告关闭回调</param>
        public void ShowAd(AdType adType, string adScenario, Action<AdResult> onClose)
        {
            string adId = GetAdId(adScenario);

            if (adType == AdType.Reward)
            {
                string userId = GetUserId();
                PlatformManager.CurrentPlatform.ShowRewardAd(adId, userId, OnAdSuccess, onClose, OnAdFail);
            }
            else
            {
                PlatformManager.CurrentPlatform.ShowAd(adType, adId, OnAdSuccess, onClose, OnAdFail);
            }
        }

        protected virtual void OnAdSuccess()
        {
        }

        protected virtual void OnAdFail(string error)
        {
        }
    }
}
