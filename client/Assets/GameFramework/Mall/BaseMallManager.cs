using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameFramework.Game;
using GameFramework.Mall.Models;
using GameFramework.Network.Proxy;
using Google.Protobuf.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace GameFramework.Mall
{
    /// <summary>
    /// 商城管理器抽象基类 - 模板方法模式
    /// 共享：拉取、缓存、购买、冷却管理
    /// 各游戏重写反序列化和 VO 转换
    /// </summary>
    public abstract class BaseMallManager<TMallItemDto, TMallItemVo> : BaseGameManager
        where TMallItemDto : BaseMallItemDto
        where TMallItemVo : BaseMallItemVo
    {
        /// <summary>
        /// 远端拉取的商城商品 DTO 列表
        /// </summary>
        protected List<TMallItemDto> FetchedMallItemDtoList { get; set; }

        /// <summary>
        /// 商城商品的 VO 列表
        /// </summary>
        protected List<TMallItemVo> CachedMallItemVoList { get; set; }

        /// <summary>
        /// 商品购买冷却 id - ulong 时间戳
        /// </summary>
        protected Dictionary<string, ulong> MallItemCooldownMap { get; private set; }

        /// <summary>
        /// 拉取远端商城物品是否成功
        /// </summary>
        public bool IsFetchedMallItemSuccess { get; protected set; }

        public event EventHandler<OnFetchedMallItemEventArgs> OnFetchedMallItem;

        public class OnFetchedMallItemEventArgs : EventArgs
        {
            public ReadOnlyCollection<TMallItemDto> FetchedMallItemDataList { get; set; }
        }

        public event EventHandler<OnBuyMallItemEventArgs> OnBuyMallItem;

        public class OnBuyMallItemEventArgs : EventArgs
        {
            public string ItemId { get; set; }
            public int Count { get; set; }
            public int PriceCoin { get; set; }
        }

        #region 子类必须实现的抽象方法

        /// <summary>
        /// 从 JSON 字符串反序列化为游戏特定的 MallItemDto 列表
        /// </summary>
        protected abstract List<TMallItemDto> DeserializeMallItems(string json);

        /// <summary>
        /// 将 DTO 转换为 VO（含异步操作如加载图标 Sprite）
        /// </summary>
        protected abstract UniTask<TMallItemVo> ConvertDtoToVo(TMallItemDto dto);

        /// <summary>
        /// 判断用户是否已登录（用于等待登录完成后拉取商城数据）
        /// </summary>
        protected abstract bool IsClientLoggedIn();

        #endregion

        #region 子类可重写的虚方法

        /// <summary>
        /// 获取收货地址和电话（购买实物商品时使用）
        /// 子类应重写此方法提供具体的地址获取逻辑
        /// </summary>
        protected virtual async UniTask<(string address, string phone)?> GetShippingInfo()
        {
            Debug.LogWarning("[BaseMallManager] GetShippingInfo 未实现，请在子类中重写");
            return null;
        }

        /// <summary>
        /// 提供 Mock 数据（编辑器模式使用），返回 null 表示不 Mock
        /// </summary>
        protected virtual List<TMallItemDto> GetMockData()
        {
            return null;
        }

        #endregion

        public override void Init()
        {
            MallItemCooldownMap = new Dictionary<string, ulong>();
            FetchRemoteMallItemData().Forget();
        }

        public override void DoUpdate()
        {
        }

        public override void Release()
        {
        }

        /// <summary>
        /// 拉取远端商城商品数据
        /// </summary>
        public virtual async UniTask FetchRemoteMallItemData()
        {
            await UniTask.WaitUntil(() => IsClientLoggedIn());

#if UNITY_EDITOR
            var mockData = GetMockData();
            if (mockData != null)
            {
                FetchedMallItemDtoList = mockData;
                OnFetchedMallItem?.Invoke(this, new OnFetchedMallItemEventArgs
                {
                    FetchedMallItemDataList = FetchedMallItemDtoList.AsReadOnly()
                });
            }
            else
            {
                await FetchFromServer();
            }
#else
            await FetchFromServer();
#endif

            if (FetchedMallItemDtoList == null) return;

            await RefreshMallItemCooldown();

            // 转换为 VO
            var convertTasks = FetchedMallItemDtoList.Select(async dto =>
            {
                var vo = await ConvertDtoToVo(dto);
                vo.Cooldown = MallItemCooldownMap.GetValueOrDefault(dto.ItemID, 0UL);
                return vo;
            });
            CachedMallItemVoList = (await UniTask.WhenAll(convertTasks)).ToList();

            Debug.Log($"[BaseMallManager] 获取商城道具列表成功");
            IsFetchedMallItemSuccess = true;
        }

        private async UniTask FetchFromServer()
        {
            string json = await PaymentProxy.Instance.RequestMallItems();
            if (string.IsNullOrEmpty(json)) return;

            FetchedMallItemDtoList = DeserializeMallItems(json);
            FetchedMallItemDtoList = FetchedMallItemDtoList.OrderBy(x => x.SortOrder).ToList();

            Debug.Log($"[BaseMallManager] 打印拉取的商品信息");
            foreach (var dto in FetchedMallItemDtoList)
            {
                Debug.Log($"[BaseMallManager] {dto}");
            }

            OnFetchedMallItem?.Invoke(this, new OnFetchedMallItemEventArgs
            {
                FetchedMallItemDataList = FetchedMallItemDtoList.AsReadOnly()
            });
        }

        /// <summary>
        /// 购买虚拟商品
        /// </summary>
        public async UniTask<bool> BuyItem(string itemId, int count)
        {
            Debug.Log($"[BaseMallManager] 购买商城道具 itemId: {itemId}, count: {count}");
            var mallItemData = GetMallItemDtoByItemId(itemId);
            string orderId = await PaymentProxy.Instance.Buy(itemId, (uint)count);

            if (string.IsNullOrEmpty(orderId))
            {
                Debug.Log($"[BaseMallManager] 购买商城道具失败 itemId: {itemId}, count: {count}");
                return false;
            }

            await PaymentProxy.Instance.GetOrderList();

            OnBuyMallItem?.Invoke(this, new OnBuyMallItemEventArgs
            {
                ItemId = itemId,
                Count = count,
                PriceCoin = mallItemData?.PriceCoin ?? 0,
            });

            return true;
        }

        /// <summary>
        /// 购买实物商品
        /// </summary>
        public async UniTask<bool> BuyPhysicalItem(string itemId, int count)
        {
            var shippingInfo = await GetShippingInfo();
            if (shippingInfo == null)
            {
                Debug.Log($"[BaseMallManager] 没有收货地址，请先添加收货地址");
                return false;
            }

            var (address, phone) = shippingInfo.Value;
            Debug.Log($"[BaseMallManager] 购买实物道具 itemId: {itemId}, count: {count}, address: {address}, Phone: {phone}");
            var mallItemData = GetMallItemDtoByItemId(itemId);
            string orderId = await PaymentProxy.Instance.BuyPhysical(itemId, (uint)count, address, phone);

            if (string.IsNullOrEmpty(orderId))
            {
                Debug.Log($"[BaseMallManager] 购买实物道具失败 itemId: {itemId}, count: {count}");
                return false;
            }

            await PaymentProxy.Instance.GetOrderList();

            OnBuyMallItem?.Invoke(this, new OnBuyMallItemEventArgs
            {
                ItemId = itemId,
                Count = count,
                PriceCoin = mallItemData?.PriceCoin ?? 0,
            });

            return true;
        }

        /// <summary>
        /// 刷新商品冷却时间
        /// </summary>
        public async UniTask RefreshMallItemCooldown()
        {
            MapField<string, ulong> cooldownMap = await PaymentProxy.Instance.GetCooldowns();
            if (cooldownMap == null) return;

            foreach (var kvp in cooldownMap)
            {
                MallItemCooldownMap[kvp.Key] = kvp.Value;
                Debug.Log($"[BaseMallManager] 刷新商城道具购买冷却 itemId: {kvp.Key}" +
                          $", cooldown: {DateTimeOffset.FromUnixTimeMilliseconds((long)kvp.Value).LocalDateTime}");
            }
        }

        /// <summary>
        /// 获取商品冷却时间
        /// </summary>
        public ulong GetItemCooldown(string itemId)
        {
            Debug.Log($"[BaseMallManager] 获取{itemId}冷却");
            return MallItemCooldownMap.GetValueOrDefault(itemId, 0UL);
        }

        /// <summary>
        /// 根据 ItemID 获取 DTO
        /// </summary>
        public TMallItemDto GetMallItemDtoByItemId(string itemId)
        {
            return FetchedMallItemDtoList?.FirstOrDefault(x => x.ItemID == itemId);
        }

        /// <summary>
        /// 获取所有 DTO 列表
        /// </summary>
        public List<TMallItemDto> GetMallItemDtoList() => FetchedMallItemDtoList;

        /// <summary>
        /// 获取所有 VO 列表
        /// </summary>
        public List<TMallItemVo> GetMallItemVoList() => CachedMallItemVoList;

        /// <summary>
        /// 从 URL 加载 Sprite（通用工具方法）
        /// </summary>
        protected async UniTask<Sprite> LoadSpriteFromUrl(string url)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            await request.SendWebRequest().ToUniTask();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }

            return null;
        }
    }
}
