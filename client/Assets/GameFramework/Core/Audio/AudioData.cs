using System;

namespace GameFramework.Core.Audio
{
    [Serializable]
    public class AudioData
    {
        public string AudioName { get; set; }
        public float Volume { get; set; }

        public AudioData(string audioName, float volume)
        {
            AudioName = audioName;
            Volume = volume;
        }

        public AudioData(string audioName)
        {
            AudioName = audioName;
            Volume = 1f;
        }

        public AudioData()
        {
            
        }
    }
}