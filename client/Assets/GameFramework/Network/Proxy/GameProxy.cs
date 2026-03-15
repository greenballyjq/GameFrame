using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.Network;
using Google.Protobuf;
using UnityEngine;

namespace GameFramework.Network.Proxy
{
    public class GameProxy : BaseProxy<GameProxy>
    {
        // 道具数据（itemId -> 数量）
        private Dictionary<string, ulong> _items = new Dictionary<string, ulong>();

        public Dictionary<string, ulong> Items => _items;

        /// <summary>
        /// 道具变化事件 - 各游戏订阅处理后续逻辑（如同步背包 InventoryManager）
        /// </summary>
        public event Action<Dictionary<string, ulong>> OnItemsUpdated;

        #region 基础数据

        /// <summary>
        /// 获取基础数据（创建时间、最后登录时间）
        /// </summary>
        public async UniTask<S2CBaseData> GetBaseData()
        {
            var root = new C2SGame
            {
                Session = ServerClient.NextSession(),
                BaseData = new C2SBaseData()
            };
            S2CGame resp = await ServerClient.PostDomainAsync<S2CGame>("/game", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.BaseData != null)
            {
                Debug.Log($"[GameProxy] 获取基础数据成功, createtime: {resp.BaseData.Createtime}, lastlogin: {resp.BaseData.Lastlogin}");
                return resp.BaseData;
            }

            Debug.LogError($"[GameProxy] 获取基础数据失败, code: {resp?.Code}");
            return null;
        }

        #endregion

        #region 自定义数据

        /// <summary>
        /// 获取自定义数据
        /// </summary>
        public async UniTask<CustomData> GetCustomData()
        {
            var root = new C2SGame
            {
                Session = ServerClient.NextSession(),
                CustomData = new C2SCustomData()
            };
            S2CGame resp = await ServerClient.PostDomainAsync<S2CGame>("/game", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.CustomData != null)
            {
                Debug.Log($"[GameProxy] 获取自定义数据成功");
                return resp.CustomData.Custom;
            }

            Debug.LogError($"[GameProxy] 获取自定义数据失败, code: {resp?.Code}");
            return null;
        }

        /// <summary>
        /// 上传自定义数据
        /// </summary>
        /// <param name="type">数据类型key</param>
        /// <param name="data">数据内容（二进制）</param>
        public async UniTask<bool> UploadCustomData(string type, byte[] data)
        {
            var root = new C2SGame
            {
                Session = ServerClient.NextSession(),
                UploadCustomData = new C2SUploadCustomData
                {
                    Type = type,
                    Data = ByteString.CopyFrom(data)
                }
            };
            S2CGame resp = await ServerClient.PostDomainAsync<S2CGame>("/game", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess)
            {
                Debug.Log($"[GameProxy] 上传自定义数据成功, type: {type}");
                return true;
            }

            Debug.LogError($"[GameProxy] 上传自定义数据失败, code: {resp?.Code}");
            return false;
        }

        #endregion

        #region 道具

        /// <summary>
        /// 拉取道具数据
        /// </summary>
        public async UniTask<Dictionary<string, ulong>> GetItemData()
        {
            var root = new C2SGame
            {
                Session = ServerClient.NextSession(),
                ItemData = new C2SItemData()
            };
            S2CGame resp = await ServerClient.PostDomainAsync<S2CGame>("/game", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.ItemData != null)
            {
                _items.Clear();
                foreach (var kv in resp.ItemData.Item.Items)
                {
                    _items[kv.Key] = kv.Value;
                }
                Debug.Log($"[GameProxy] 拉取道具数据成功, 种类数: {_items.Count}");
                return _items;
            }

            Debug.LogError($"[GameProxy] 拉取道具数据失败, code: {resp?.Code}");
            return null;
        }

        /// <summary>
        /// 上报道具消耗（使用道具时调用）
        /// </summary>
        /// <param name="itemId">道具ID</param>
        /// <param name="count">消耗数量</param>
        public async UniTask<bool> UseItem(string itemId, ulong count)
        {
            var c2sItemUpdate = new C2SItemUpdate();
            c2sItemUpdate.Items.Add(itemId, count);

            var root = new C2SGame
            {
                Session = ServerClient.NextSession(),
                ItemUpdate = c2sItemUpdate
            };

            Debug.Log($"[GameProxy] 上报道具消耗, itemId: {itemId}, count: {count}");
            S2CGame resp = await ServerClient.PostDomainAsync<S2CGame>("/game", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.ItemUpdate != null)
            {
                Debug.Log($"[GameProxy] 道具消耗上报成功");
                return true;
            }

            Debug.LogError($"[GameProxy] 道具消耗上报失败, code: {resp?.Code}");
            return false;
        }

        /// <summary>
        /// 处理道具增量更新响应（使用道具后服务器返回的变化量）
        /// 通过 OnItemsUpdated 事件通知业务层，不直接引用游戏特有 Manager
        /// </summary>
        public void ApplyItemUpdate(S2CItemUpdate itemUpdate)
        {
            if (itemUpdate == null) return;

            var changedItems = new Dictionary<string, ulong>();
            foreach (var kv in itemUpdate.Items)
            {
                if (kv.Value == 0)
                {
                    _items.Remove(kv.Key);
                }
                else
                {
                    _items[kv.Key] = kv.Value;
                }
                changedItems[kv.Key] = kv.Value;
                Debug.Log($"[GameProxy] 道具增量更新: {kv.Key} -> {kv.Value}");
            }

            OnItemsUpdated?.Invoke(changedItems);
        }

        #endregion

        #region 道具新增

        /// <summary>
        /// 上报道具新增（看广告获得道具）
        /// </summary>
        /// <param name="itemId">道具ID</param>
        /// <param name="count">新增数量</param>
        /// <param name="adCustomId">广告验证 customData</param>
        public async UniTask<bool> AddItem(string itemId, ulong count, string adCustomId)
        {
            var c2sItemAdd = new C2SItemAdd { AdCustomId = adCustomId ?? "" };
            c2sItemAdd.Items.Add(itemId, count);

            var root = new C2SGame
            {
                Session = ServerClient.NextSession(),
                ItemAdd = c2sItemAdd
            };

            Debug.Log($"[GameProxy] 上报道具新增, itemId: {itemId}, count: {count}, adCustomId: {adCustomId}");
            int delayMs = string.IsNullOrEmpty(adCustomId) ? 0 : ServerClient.AD_VERIFY_DELAY_MS;
            S2CGame resp = await ServerClient.PostDomainAsync<S2CGame>("/game", root, delayMs);

            if (resp != null && resp.Code == S2COpcode.OpWxadVerifyPending)
            {
                Debug.Log($"[GameProxy] 广告验证未就绪，1s 后重试");
                await UniTask.Delay(1000, true);
                resp = await ServerClient.PostDomainAsync<S2CGame>("/game", root);
            }

            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.ItemAdd != null)
            {
                Debug.Log($"[GameProxy] 道具新增成功");
                await GetItemData();
                return true;
            }

            Debug.LogError($"[GameProxy] 道具新增失败, code: {resp?.Code}");
            return false;
        }

        #endregion

        #region 进度数据

        /// <summary>
        /// 获取进度数据（累计积分）
        /// </summary>
        public async UniTask<ProgressData> GetProgressData()
        {
            var root = new C2SGame
            {
                Session = ServerClient.NextSession(),
                ProgressData = new C2SProgressData()
            };
            S2CGame resp = await ServerClient.PostDomainAsync<S2CGame>("/game", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.ProgressData != null)
            {
                Debug.Log($"[GameProxy] 获取进度数据成功, point: {resp.ProgressData.Progress.Point}");
                return resp.ProgressData.Progress;
            }

            Debug.LogError($"[GameProxy] 获取进度数据失败, code: {resp?.Code}");
            return null;
        }

        #endregion

        #region 战斗记录

        /// <summary>
        /// 获取战斗历史记录
        /// </summary>
        public async UniTask<S2CBattleHistory> GetBattleHistory()
        {
            var root = new C2SGame
            {
                Session = ServerClient.NextSession(),
                BattleHistory = new C2SBattleHistory()
            };
            S2CGame resp = await ServerClient.PostDomainAsync<S2CGame>("/game", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.BattleHistory != null)
            {
                Debug.Log($"[GameProxy] 获取战斗记录成功, 记录数: {resp.BattleHistory.Records.Count}");
                return resp.BattleHistory;
            }

            Debug.LogError($"[GameProxy] 获取战斗记录失败, code: {resp?.Code}");
            return null;
        }

        #endregion

        #region 金币获取

        /// <summary>
        /// 上报金币（三千币）获取
        /// </summary>
        /// <param name="goldGain">获得的金币数量</param>
        /// <param name="sourceType">来源类型（如 "kill_monster"）</param>
        /// <param name="sourceId">来源标识</param>
        public async UniTask<ulong> GoldGain(ulong goldGain, string sourceType, string sourceId)
        {
            var root = new C2SGame
            {
                Session = ServerClient.NextSession(),
                GoldGain = new C2SGoldGain
                {
                    GoldGain = goldGain,
                    SourceType = sourceType,
                    SourceId = sourceId
                }
            };
            Debug.Log($"[GameProxy] 上报金币获取, goldGain: {goldGain}, sourceType: {sourceType}, sourceId: {sourceId}");
            S2CGame resp = await ServerClient.PostDomainAsync<S2CGame>("/game", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.GoldGain != null)
            {
                UserProxy.Instance.SetThreekp(resp.GoldGain.ThreekpTotal);
                Debug.Log($"[GameProxy] 上报金币获取成功, threekpTotal: {resp.GoldGain.ThreekpTotal}");
                return resp.GoldGain.ThreekpTotal;
            }

            Debug.LogError($"[GameProxy] 上报金币获取失败, code: {resp?.Code}");
            return 0;
        }

        #endregion
    }
}
