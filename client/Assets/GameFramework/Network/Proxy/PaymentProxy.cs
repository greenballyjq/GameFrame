using System;
using Cysharp.Threading.Tasks;
using Google.Protobuf.Collections;
using UnityEngine;

namespace GameFramework.Network.Proxy
{
    /// <summary>
    /// 通用支付代理 - 处理商城商品查询、购买、广告购买、冷却查询
    /// 返回原始 JSON/orderId，各游戏通过事件订阅处理后续逻辑（如刷新背包）
    /// </summary>
    public class PaymentProxy : BaseProxy<PaymentProxy>
    {
        /// <summary>
        /// 购买完成事件 - 各游戏订阅处理后续逻辑（如同步背包数据）
        /// 参数为 orderId
        /// </summary>
        public event Func<string, UniTask> OnPurchaseCompleted;

        /// <summary>
        /// 商城看广告完成事件
        /// </summary>
        public event EventHandler OnMallAdComplete;

        /// <summary>
        /// 获取商城商品信息 - 返回原始 JSON 字符串，由各游戏自行反序列化
        /// </summary>
        public async UniTask<string> RequestMallItems()
        {
            var root = new C2SPayment
            {
                Session = ServerClient.NextSession(),
                Mall = new C2SMall()
            };
            S2CPayment resp = await ServerClient.PostDomainAsync<S2CPayment>("/payment", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.Mall != null)
            {
                Debug.Log($"[PaymentProxy] 获取商城数据成功, JSON: {resp.Mall.Items}");
                return resp.Mall.Items;
            }

            Debug.LogError($"[PaymentProxy] 获取商城数据失败, code: {resp?.Code}");
            return null;
        }

        /// <summary>
        /// 购买商城商品（虚拟商品）
        /// </summary>
        /// <param name="itemId">商品ID</param>
        /// <param name="count">购买数量</param>
        /// <returns>订单id，失败返回null</returns>
        public async UniTask<string> Buy(string itemId, uint count)
        {
            var root = new C2SPayment
            {
                Session = ServerClient.NextSession(),
                Buy = new C2SBuy
                {
                    ItemId = itemId,
                    Count = count
                }
            };

            Debug.Log($"[PaymentProxy] 购买道具 itemId:{itemId}, count: {count}");
            S2CPayment resp = await ServerClient.PostDomainAsync<S2CPayment>("/payment", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.Buy != null)
            {
                Debug.Log($"[PaymentProxy] 购买道具成功, 订单id: {resp.Buy.OrderId}");

                if (OnPurchaseCompleted != null)
                {
                    await OnPurchaseCompleted.Invoke(resp.Buy.OrderId);
                }

                return resp.Buy.OrderId;
            }

            Debug.LogError($"[PaymentProxy] 购买道具失败, code: {resp?.Code}");
            return null;
        }

        /// <summary>
        /// 购买实物商城商品
        /// </summary>
        /// <param name="itemId">商品ID</param>
        /// <param name="count">购买数量</param>
        /// <param name="address">收货地址</param>
        /// <param name="phone">联系电话</param>
        /// <returns>订单id，失败返回null</returns>
        public async UniTask<string> BuyPhysical(string itemId, uint count, string address, string phone)
        {
            var root = new C2SPayment
            {
                Session = ServerClient.NextSession(),
                Buy = new C2SBuy
                {
                    ItemId = itemId,
                    Count = count,
                    Address = address,
                    Phone = phone,
                }
            };

            Debug.Log($"[PaymentProxy] 购买实物道具 itemId:{itemId}, count: {count}, address: {address}, Phone: {phone}");
            S2CPayment resp = await ServerClient.PostDomainAsync<S2CPayment>("/payment", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.Buy != null)
            {
                Debug.Log($"[PaymentProxy] 购买实物道具成功, 订单id: {resp.Buy.OrderId}");

                if (OnPurchaseCompleted != null)
                {
                    await OnPurchaseCompleted.Invoke(resp.Buy.OrderId);
                }

                return resp.Buy.OrderId;
            }

            Debug.LogError($"[PaymentProxy] 购买实物道具失败, code: {resp?.Code}");
            return null;
        }

        /// <summary>
        /// 商城看广告购买（如看广告获取三币）
        /// </summary>
        /// <param name="itemId">商品ID</param>
        /// <param name="customAdId">广告验证ID</param>
        /// <returns>订单id，失败返回null</returns>
        public async UniTask<string> BuyWithAd(string itemId, string customAdId)
        {
            var root = new C2SPayment
            {
                Session = ServerClient.NextSession(),
                Buy = new C2SBuy
                {
                    ItemId = itemId,
                    Count = 1,
                    AdCustomId = customAdId,
                }
            };

            Debug.Log($"[PaymentProxy] 看广告购买：{itemId}, {customAdId}");
            int delayMs = string.IsNullOrEmpty(customAdId) ? 0 : ServerClient.AD_VERIFY_DELAY_MS;
            S2CPayment resp = await ServerClient.PostDomainAsync<S2CPayment>("/payment", root, delayMs);
            if (resp != null && resp.Code == S2COpcode.OpWxadVerifyPending)
            {
                Debug.Log($"[PaymentProxy] 看广告购买，广告验证未就绪，2s后重试");
                await UniTask.Delay(2000, ignoreTimeScale: true);
                resp = await ServerClient.PostDomainAsync<S2CPayment>("/payment", root);
            }

            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.Buy != null)
            {
                Debug.Log($"[PaymentProxy] 看广告购买成功，{resp.Buy.OrderId}");
                OnMallAdComplete?.Invoke(this, EventArgs.Empty);
                return resp.Buy.OrderId;
            }

            Debug.Log($"[PaymentProxy] 看广告购买失败，{resp?.Code}");
            return null;
        }

        /// <summary>
        /// 获取商城物品冷却时间
        /// </summary>
        public async UniTask<MapField<string, ulong>> GetCooldowns()
        {
            var root = new C2SPayment
            {
                Session = ServerClient.NextSession(),
                Mall = new C2SMall()
            };

            Debug.Log($"[PaymentProxy] 获取商城物品冷却时间");
            S2CPayment resp = await ServerClient.PostDomainAsync<S2CPayment>("/payment", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.Mall != null)
            {
                Debug.Log($"[PaymentProxy] 获取商城物品冷却时间成功");
                return resp.Mall.CooldownItems;
            }

            Debug.Log($"[PaymentProxy] 获取商城物品冷却时间失败，{resp?.Code}");
            return null;
        }

        /// <summary>
        /// 获取商城商品是否在冷却中
        /// </summary>
        public async UniTask<bool> IsCooldown(string itemId)
        {
            var root = new C2SPayment
            {
                Session = ServerClient.NextSession(),
                Buy = new C2SBuy
                {
                    ItemId = itemId,
                    Count = 1,
                }
            };

            Debug.Log($"[PaymentProxy] 获取商城商品{itemId} 是否正在冷却中");
            S2CPayment resp = await ServerClient.PostDomainAsync<S2CPayment>("/payment", root);

            if (resp != null)
            {
                return resp.Code == S2COpcode.OpMallItemCooldown;
            }

            return true;
        }

        /// <summary>
        /// 获取订单列表
        /// </summary>
        public async UniTask GetOrderList()
        {
            var root = new C2SPayment
            {
                Session = ServerClient.NextSession(),
                OrderList = new C2SOrderList()
            };
            S2CPayment resp = await ServerClient.PostDomainAsync<S2CPayment>("/payment", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.OrderList != null)
            {
                Debug.Log($"[PaymentProxy] 获取订单列表成功：{resp.OrderList.Content}");
            }
            else
            {
                Debug.LogError($"[PaymentProxy] 获取订单列表失败, code: {resp?.Code}");
            }
        }
    }
}
