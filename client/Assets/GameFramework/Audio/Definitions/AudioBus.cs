using UnityEngine;
using UnityEngine.Audio;

namespace GameFramework.Audio
{
    /// <summary>
    /// 输出分组：一组内容共享的混音目标与实例预算。
    /// </summary>
    [CreateAssetMenu(fileName = "AudioBus", menuName = "GameFramework/Audio/Audio Bus")]
    public class AudioBus : ScriptableObject
    {
        [SerializeField] private AudioMixerGroup _outputGroup;
        [SerializeField] private string _volumeParam;
        [SerializeField] private int _maxInstances = -1;
        [SerializeField] private OverflowPolicy _overflow = OverflowPolicy.RejectNew;

        /// <summary>这一组声音在混音器上的目标。留空时声音直接进主输出，不受任何分组音量控制。</summary>
        public AudioMixerGroup OutputGroup
        {
            get { return _outputGroup; }
        }

        /// <summary>混音器上暴露的音量参数名。这个参数没在混音器上暴露时，设置音量会失败。</summary>
        public string VolumeParam
        {
            get { return _volumeParam; }
        }

        /// <summary>这条总线上同时最多几条。负数不限，0 表示禁止，正数为上限。</summary>
        public int MaxInstances
        {
            get { return _maxInstances; }
        }

        /// <summary>
        /// 到达上限时对既有实例做什么。槽位已满、且模块配置的槽位处置是拒绝时，
        /// 这里配的"顶掉最旧"到不了。
        /// </summary>
        public OverflowPolicy Overflow
        {
            get { return _overflow; }
        }

        private void OnValidate()
        {
            if (_maxInstances < -1)
                _maxInstances = -1;
        }
    }
}
