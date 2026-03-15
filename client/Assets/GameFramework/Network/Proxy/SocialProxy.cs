using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.Network;
using UnityEngine;

namespace GameFramework.Network.Proxy
{
    public class SocialProxy : BaseProxy<SocialProxy>
    {
        /// <summary>
        /// 已签到事件
        /// </summary>
        public event EventHandler OnSignedIn;

        /// <summary>
        /// 签到
        /// </summary>
        public async UniTask<S2CSignIn> SignIn()
        {
            var root = new C2SSocial
            {
                Session = ServerClient.NextSession(),
                SignIn = new C2SSignIn()
            };

            Debug.Log($"[SocialProxy] 签到");

            S2CSocial resp = await ServerClient.PostDomainAsync<S2CSocial>("/social", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.SignIn != null)
            {
                Debug.Log($"[SocialProxy] 签到成功 - lastDate: {resp.SignIn.Data?.LastDate}");
                OnSignedIn?.Invoke(this, System.EventArgs.Empty);
                return resp.SignIn;
            }

            Debug.LogError($"[SocialProxy] 签到失败, code: {resp?.Code}");
            return null;
        }

        /// <summary>
        /// 补签
        /// </summary>
        /// <param name="replaceDay">补签目标天数</param>
        /// <param name="customAdId">激励广告id</param>
        public async UniTask<S2CSignIn> ReplaceSignIn(int replaceDay, string customAdId)
        {
            var root = new C2SSocial
            {
                Session = ServerClient.NextSession(),
                SignIn = new C2SSignIn
                {
                    SignInDay = replaceDay,
                    AdCustomId = customAdId
                }
            };

            Debug.Log($"[SocialProxy] 补签 - replaceDay: {replaceDay}, customAdId: {customAdId}");

            int delayMs = string.IsNullOrEmpty(customAdId) ? 0 : ServerClient.AD_VERIFY_DELAY_MS;
            S2CSocial resp = await ServerClient.PostDomainAsync<S2CSocial>("/social", root, delayMs);

            if (resp != null && resp.Code == S2COpcode.OpWxadVerifyPending)
            {
                Debug.Log($"[SocialProxy] 补签，广告验证未就绪，1s后重试");
                await UniTask.Delay(1000, ignoreTimeScale: true);
                resp = await ServerClient.PostDomainAsync<S2CSocial>("/social", root);
            }

            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.SignIn != null)
            {
                Debug.Log($"[SocialProxy] 补签成功 - lastDate: {resp.SignIn.Data?.LastDate}");
                OnSignedIn?.Invoke(this, System.EventArgs.Empty);
                return resp.SignIn;
            }

            Debug.LogError($"[SocialProxy] 补签失败, code: {resp?.Code}");
            return null;
        }

        /// <summary>
        /// 双倍签到
        /// </summary>
        /// <param name="day">第几天签到</param>
        /// <param name="customAdId">激励广告id</param>
        public async UniTask<S2CSignIn> DoubleSignIn(int day, string customAdId)
        {
            var root = new C2SSocial
            {
                Session = ServerClient.NextSession(),
                SignIn = new C2SSignIn
                {
                    SignInDay = day,
                    AdCustomId = customAdId
                }
            };

            Debug.Log($"[SocialProxy] 双倍签到 - signInDay: {day}, customAdId: {customAdId}");

            int delayMs = string.IsNullOrEmpty(customAdId) ? 0 : ServerClient.AD_VERIFY_DELAY_MS;
            S2CSocial resp = await ServerClient.PostDomainAsync<S2CSocial>("/social", root, delayMs);

            if (resp != null && resp.Code == S2COpcode.OpWxadVerifyPending)
            {
                Debug.Log($"[SocialProxy] 双倍签到，广告验证未就绪，1s后重试");
                await UniTask.Delay(2000, ignoreTimeScale: true);
                resp = await ServerClient.PostDomainAsync<S2CSocial>("/social", root);
            }

            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.SignIn != null)
            {
                Debug.Log($"[SocialProxy] 双倍签到成功 - lastDate: {resp.SignIn.Data?.LastDate}");
                OnSignedIn?.Invoke(this, System.EventArgs.Empty);
                return resp.SignIn;
            }

            Debug.LogError($"[SocialProxy] 双倍失败, code: {resp?.Code}");
            return null;
        }

        /// <summary>
        /// 获取当前用户签到信息
        /// </summary>
        public async UniTask<S2CGetSignInInfo> GetSignInInfo()
        {
            var root = new C2SSocial
            {
                Session = ServerClient.NextSession(),
                GetSignInInfo = new C2SGetSignInInfo()
            };

            Debug.Log($"[SocialProxy] 获取签到信息");

            S2CSocial resp = await ServerClient.PostDomainAsync<S2CSocial>("/social", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.GetSignInInfo != null)
            {
                var data = resp.GetSignInInfo.Data;
                string signedDaysStr = data?.SignedDays.Aggregate("", (pre, next) => $"{pre}->{next}") ?? "";
                Debug.Log(
                    $"[SocialProxy] 获取签到信息成功 - firstSigninDate: {data?.FirstSigninDate}, lastDate: {data?.LastDate}, " +
                    $"cycleStartDate: {data?.CycleStartDate}, signedDays: {signedDaysStr}");

                return resp.GetSignInInfo;
            }

            Debug.LogError($"[SocialProxy] 获取签到信息失败, code: {resp?.Code}");
            return null;
        }
    }
}
