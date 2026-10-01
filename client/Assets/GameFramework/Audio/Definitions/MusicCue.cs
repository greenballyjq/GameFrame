using UnityEngine;

namespace GameFramework.Audio
{
    /// <summary>
    /// 一段音乐的内容定义。音乐总是循环播放，不会自然结束，只能被停止或替换。
    /// </summary>
    [CreateAssetMenu(fileName = "MusicCue", menuName = "GameFramework/Audio/Music Cue")]
    public class MusicCue : ScriptableObject
    {
        [SerializeField] private AudioClip _clip;
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;
        [SerializeField] private AudioBus _bus;

        /// <summary>留空时播放会被当作"停止当前音乐"处理，而不是无声播放一段空音乐。</summary>
        public AudioClip Clip
        {
            get { return _clip; }
        }

        /// <summary>淡变完成后要到达的音量，与总线音量相乘。</summary>
        public float Volume
        {
            get { return _volume; }
        }

        /// <summary>输出分组。留空时用配置资产上的默认总线。</summary>
        public AudioBus Bus
        {
            get { return _bus; }
        }
    }
}
