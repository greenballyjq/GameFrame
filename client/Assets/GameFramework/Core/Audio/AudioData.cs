using System;
using GameFramework.Manager;
using UnityEngine;

namespace GameFramework.Core.Audio
{
    [Serializable]
    public class AudioData
    {
        public string audioName;
        public string audioDescription;
        public AudioChannel audioChannel;
        public AudioClip audioClip;
        [Range(0, 1)] public float volume;

        public AudioData(AudioClip audioClip, float volume)
        {
            this.audioClip = audioClip;
            this.volume = volume;
        }
        
        public AudioData(string audioName, float volume)
        {
            this.audioName = audioName;
            this.volume = volume;
        }

        public AudioData(string audioName)
        {
            this.audioName = audioName;
            volume = 1f;
        }

        public AudioData()
        {
            
        }
    }
}