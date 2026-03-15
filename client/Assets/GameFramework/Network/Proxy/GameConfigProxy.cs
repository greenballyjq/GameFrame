using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.Network;
using LitJson;
using UnityEngine;

namespace GameFramework.Network.Proxy
{
    /// <summary>
    /// 通用配置代理 - 通过注册模式，各游戏注册需要的配置名和回调，统一拉取
    /// </summary>
    public class GameConfigProxy : BaseProxy<GameConfigProxy>
    {
        private readonly Dictionary<string, JsonData> _configCache = new Dictionary<string, JsonData>();
        private readonly List<(string name, Action<JsonData> setter)> _registrations = new List<(string, Action<JsonData>)>();

        /// <summary>
        /// 配置是否拉取完毕
        /// </summary>
        public bool ConfigsFetchComplete { get; private set; } = false;

        /// <summary>
        /// 注册一个配置项 - 在 FetchAllConfigs 时自动拉取并回调
        /// </summary>
        /// <param name="configName">服务器配置名称</param>
        /// <param name="onFetched">拉取成功后的回调，传入解析后的 JsonData</param>
        public void RegisterConfig(string configName, Action<JsonData> onFetched)
        {
            _registrations.Add((configName, onFetched));
        }

        /// <summary>
        /// 通用配置获取
        /// </summary>
        /// <param name="configName">配置名称</param>
        /// <returns>配置数据字符串</returns>
        public async UniTask<string> GetConfiguration(string configName)
        {
            var root = new C2SGame
            {
                Session = ServerClient.NextSession(),
                Configuration = new C2SConfiguration
                {
                    Name = configName
                }
            };
            Debug.Log($"[GameConfigProxy] 获取配置: {configName}");
            S2CGame resp = await ServerClient.PostDomainAsync<S2CGame>("/game", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.Configuration != null)
            {
                Debug.Log($"[GameConfigProxy] 获取配置成功: {configName}");
                Debug.Log($"[GameConfigProxy] 返回数据：{resp.Configuration.Data}");
                return resp.Configuration.Data;
            }

            Debug.LogError($"[GameConfigProxy] 获取配置失败: {configName}, code: {resp?.Code}");
            return null;
        }

        /// <summary>
        /// 获取配置并解析外层JSON，返回 data.content 解析后的 JsonData
        /// </summary>
        public JsonData ParseConfigContent(string configJson)
        {
            if (string.IsNullOrEmpty(configJson)) return null;

            try
            {
                var jsonData = JsonMapper.ToObject(configJson);
                string contentJson = (string)jsonData["data"]["content"];
                return JsonMapper.ToObject(contentJson);
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameConfigProxy] 配置解析失败: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取缓存的配置数据
        /// </summary>
        public JsonData GetCachedConfig(string configName)
        {
            _configCache.TryGetValue(configName, out var data);
            return data;
        }

        /// <summary>
        /// 批量拉取所有已注册的配置
        /// </summary>
        public async UniTask FetchAllConfigs()
        {
            foreach (var reg in _registrations)
            {
                string raw = await GetConfiguration(reg.name);
                var data = ParseConfigContent(raw);
                _configCache[reg.name] = data;
                reg.setter?.Invoke(data);
            }
            ConfigsFetchComplete = true;
        }
    }
}
