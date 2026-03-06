using System;

namespace GameFramework.Core.Audio
{
    public class AudioCallback
    {
        private Action<AudioPlayer> _onCompleteCallback;
        
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
}