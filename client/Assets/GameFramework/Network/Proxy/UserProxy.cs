using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.Network;
using UnityEngine;

namespace GameFramework.Network.Proxy
{
    public class UserProxy : BaseProxy<UserProxy>
    {
        // 用户资料
        private UserProfile _userProfile;

        // 用户资产（三千币）
        private ulong _threekp = 0;

        // 收件地址列表
        private List<AddressInfo> _addresses = new List<AddressInfo>();

        // 心跳定时器相关
        private bool _isPingActive = false;
        private CancellationTokenSource _pingCancellationTokenSource;

        public UserProfile UserProfile => _userProfile;
        public ulong Threekp => _threekp;
        public List<AddressInfo> Addresses => _addresses;

        #region 用户资料

        /// <summary>
        /// 获取用户资料
        /// </summary>
        public async UniTask<UserProfile> GetProfile()
        {
            var root = new C2SUser
            {
                Session = ServerClient.NextSession(),
                GetProfile = new C2SGetProfile()
            };
            S2CUser resp = await ServerClient.PostDomainAsync<S2CUser>("/user", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.GetProfile != null)
            {
                _userProfile = resp.GetProfile.Profile;
                Debug.Log($"[UserProxy] 获取用户资料成功 - uuid: {_userProfile.Uuid}, avatar: {_userProfile.Avatar}, " +
                          $"nickname: {_userProfile.Nickname}, platform: {_userProfile.Platform}, gold: {_userProfile.Gold}");
                return _userProfile;
            }

            Debug.LogError($"[UserProxy] 获取用户资料失败, code: {resp?.Code}");
            return null;
        }

        #endregion

        #region 资产

        /// <summary>
        /// 获取资产数据
        /// </summary>
        public async UniTask<ulong> GetCurrencyData()
        {
            var root = new C2SUser
            {
                Session = ServerClient.NextSession(),
                GetAssets = new C2SGetAssets()
            };
            S2CUser resp = await ServerClient.PostDomainAsync<S2CUser>("/user", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.GetAssets != null)
            {
                _threekp = resp.GetAssets.Threekp;
                Debug.Log($"[UserProxy] 获取资产成功: threekp={_threekp}");
                return _threekp;
            }

            Debug.LogError($"[UserProxy] 获取资产失败, code: {resp?.Code}");
            return 0;
        }

        /// <summary>
        /// 更新本地三千币（供其他Proxy结算后同步）
        /// </summary>
        public void SetThreekp(ulong value)
        {
            _threekp = value;
        }

        #endregion

        #region 绑定手机

        /// <summary>
        /// 绑定手机号
        /// </summary>
        public async UniTask<bool> BindPhone(string phone, string code)
        {
            var root = new C2SUser
            {
                Session = ServerClient.NextSession(),
                BindPhone = new C2SBindPhone
                {
                    Phone = phone,
                    Code = code
                }
            };
            S2CUser resp = await ServerClient.PostDomainAsync<S2CUser>("/user", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.BindPhone != null)
            {
                Debug.Log($"[UserProxy] 绑定手机号成功 - phone: {phone}, message: {resp.BindPhone.Message}");
                return resp.BindPhone.Success;
            }

            Debug.LogError($"[UserProxy] 绑定手机号失败, code: {resp?.Code}");
            return false;
        }

        #endregion

        #region 登出

        /// <summary>
        /// 登出
        /// </summary>
        public async UniTask<bool> Logout()
        {
            var root = new C2SUser
            {
                Session = ServerClient.NextSession(),
                Logout = new C2SLogout()
            };
            S2CUser resp = await ServerClient.PostDomainAsync<S2CUser>("/user", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.Logout != null)
            {
                Debug.Log($"[UserProxy] 登出成功");
                StopPingTimer();
                return resp.Logout.Success;
            }

            Debug.LogError($"[UserProxy] 登出失败, code: {resp?.Code}");
            return false;
        }

        #endregion

        #region 收件地址

        /// <summary>
        /// 获取收件地址列表
        /// </summary>
        public async UniTask<List<AddressInfo>> GetAddresses()
        {
            var root = new C2SUser
            {
                Session = ServerClient.NextSession(),
                GetAddresses = new C2SGetAddresses()
            };
            S2CUser resp = await ServerClient.PostDomainAsync<S2CUser>("/user", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.GetAddresses != null)
            {
                _addresses.Clear();
                _addresses.AddRange(resp.GetAddresses.Addresses);
                Debug.Log($"[UserProxy] 获取收件地址列表成功, 数量: {_addresses.Count}");
                return _addresses;
            }

            Debug.LogError($"[UserProxy] 获取收件地址列表失败, code: {resp?.Code}");
            return null;
        }

        /// <summary>
        /// 添加收件地址
        /// </summary>
        public async UniTask<AddressInfo> AddAddress(string name, string phone, string address, bool isDefault = false)
        {
            var root = new C2SUser
            {
                Session = ServerClient.NextSession(),
                AddAddress = new C2SAddAddress
                {
                    Name = name,
                    Phone = phone,
                    Address = address,
                    IsDefault = isDefault
                }
            };
            S2CUser resp = await ServerClient.PostDomainAsync<S2CUser>("/user", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.AddAddress != null)
            {
                var newAddress = resp.AddAddress.AddressInfo;
                _addresses.Add(newAddress);
                Debug.Log($"[UserProxy] 添加收件地址成功, id: {newAddress.Id}");
                return newAddress;
            }

            Debug.LogError($"[UserProxy] 添加收件地址失败, code: {resp?.Code}");
            return null;
        }

        /// <summary>
        /// 删除收件地址
        /// </summary>
        public async UniTask<bool> DeleteAddress(ulong id)
        {
            var root = new C2SUser
            {
                Session = ServerClient.NextSession(),
                DeleteAddress = new C2SDeleteAddress
                {
                    Id = id
                }
            };
            S2CUser resp = await ServerClient.PostDomainAsync<S2CUser>("/user", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess)
            {
                _addresses.RemoveAll(a => a.Id == id);
                Debug.Log($"[UserProxy] 删除收件地址成功, id: {id}");
                return true;
            }

            Debug.LogError($"[UserProxy] 删除收件地址失败, code: {resp?.Code}");
            return false;
        }

        /// <summary>
        /// 更新收件地址
        /// </summary>
        public async UniTask<AddressInfo> UpdateAddress(ulong id, string name, string phone, string address, bool isDefault = false)
        {
            var root = new C2SUser
            {
                Session = ServerClient.NextSession(),
                UpdateAddress = new C2SUpdateAddress
                {
                    Id = id,
                    Name = name,
                    Phone = phone,
                    Address = address,
                    IsDefault = isDefault
                }
            };
            S2CUser resp = await ServerClient.PostDomainAsync<S2CUser>("/user", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.UpdateAddress != null)
            {
                var updated = resp.UpdateAddress.AddressInfo;
                var index = _addresses.FindIndex(a => a.Id == id);
                if (index >= 0)
                {
                    _addresses[index] = updated;
                }
                Debug.Log($"[UserProxy] 更新收件地址成功, id: {id}");
                return updated;
            }

            Debug.LogError($"[UserProxy] 更新收件地址失败, code: {resp?.Code}");
            return null;
        }

        #endregion

        #region 心跳

        /// <summary>
        /// 开始定时心跳
        /// </summary>
        public void StartPingTimer()
        {
            if (_isPingActive)
                return;

            _isPingActive = true;
            _pingCancellationTokenSource = new CancellationTokenSource();
            PingTimerLoop(_pingCancellationTokenSource.Token).Forget();
        }

        /// <summary>
        /// 停止定时心跳
        /// </summary>
        public void StopPingTimer()
        {
            if (!_isPingActive)
                return;

            _isPingActive = false;
            _pingCancellationTokenSource?.Cancel();
            _pingCancellationTokenSource?.Dispose();
            _pingCancellationTokenSource = null;
        }

        private async UniTaskVoid PingTimerLoop(CancellationToken cancellationToken)
        {
            while (_isPingActive && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await HeartbeatAsync();
                    await UniTask.Delay(30000, cancellationToken: cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    Debug.Log($"[UserProxy] 心跳定时器被取消");
                    break;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UserProxy] 心跳定时器出错: {e.Message}");
                    await UniTask.Delay(30000, cancellationToken: cancellationToken);
                }
            }
        }

        private async UniTask HeartbeatAsync()
        {
            var root = new C2SUser
            {
                Session = ServerClient.NextSession(),
                Heartbeat = new C2SHeartbeat
                {
                    ClientTime = (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                }
            };
            S2CUser resp = await ServerClient.PostDomainAsync<S2CUser>("/user", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.Heartbeat != null)
            {
                Debug.Log($"[UserProxy] 心跳成功 - ServerTime:{resp.Heartbeat.ServerTime}");
            }
            else
            {
                Debug.Log($"[UserProxy] 心跳失败, code: {resp?.Code}");
            }
        }

        #endregion

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            StopPingTimer();
        }
    }
}
