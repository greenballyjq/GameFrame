using System;

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
}