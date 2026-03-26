using System;
using Cysharp.Threading.Tasks;

namespace GameFramework.Core.Audio
{
    public class AudioCallback
    {
        private Action<AudioPlayer> _onCompleteCallback;
        private AudioPlayer _player;

        /// <summary>
        /// 关联的AudioPlayer（播放开始时即可获取，用于Stop等操作）
        /// </summary>
        public AudioPlayer Player => _player;

        internal void SetPlayer(AudioPlayer player) => _player = player;

        internal void ClearPlayer() => _player = null;

        public void AddCallback(Action<AudioPlayer> callback) => _onCompleteCallback += callback;

        public AudioCallback WithCallback(Action<AudioPlayer> callback)
        {
            _onCompleteCallback += callback;
            return this;
        }

        public void RemoveCallback(Action<AudioPlayer> callback) => _onCompleteCallback -= callback;
        
        public void ClearCallback() => _onCompleteCallback = null;
        
        public void InvokeCallback(AudioPlayer player) => _onCompleteCallback?.Invoke(player);
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
            callback.AddCallback((audioPlayer) => tcs.TrySetResult());
            return tcs.Task;
        }
    }
}