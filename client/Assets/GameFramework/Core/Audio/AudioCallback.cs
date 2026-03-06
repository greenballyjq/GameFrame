using System;
using Cysharp.Threading.Tasks;

namespace GameFramework.Core.Audio
{
    public class AudioCallback
    {
        private Action _onCompleteCallback;
        
        public void AddCallback(Action callback) => _onCompleteCallback += callback;

        public AudioCallback WithCallback(Action callback)
        {
            _onCompleteCallback += callback;
            return this;
        }

        public void RemoveCallback(Action callback) => _onCompleteCallback -= callback;
        
        public void ClearCallback() => _onCompleteCallback = null;
        
        public void InvokeCallback() => _onCompleteCallback?.Invoke();
    }

    public static class AudioCallbackExtensions
    {
        /// <summary>
        /// 转换为可 await 的 UniTask。callback 为 null 时立即完成。
        /// </summary>
        public static UniTask ToUniTask(this AudioCallback callback)
        {
            if (callback == null) return UniTask.CompletedTask;
            var tcs = new UniTaskCompletionSource();
            callback.AddCallback(() => tcs.TrySetResult());
            return tcs.Task;
        }
    }
}