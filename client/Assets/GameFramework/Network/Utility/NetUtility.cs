using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace GameFramework.Network.Utility
{
    public static class NetUtility
    {
        /// <summary>
        /// 从 URL 加载 Sprite（通用工具方法）
        /// </summary>
        public static async UniTask<Sprite> LoadSpriteFromUrl(string url)
        {
            Debug.Log($"[NetUtility] 从 URL 加载 Sprite: {url}");
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