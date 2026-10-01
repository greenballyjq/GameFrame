using UnityEngine;
using UnityEngine.Audio;

namespace GameFramework.Audio
{
    /// <summary>
    /// 音频模块的配置资产：混音器、默认总线、发声槽位数量，以及槽位用满时的处置。
    /// </summary>
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "GameFramework/Audio/Audio Config")]
    public class AudioConfig : ScriptableObject
    {
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private AudioBus _defaultBus;
        [SerializeField, Range(1, 256)] private int _slotCount = 32;
        [SerializeField] private OverflowPolicy _slotOverflow = OverflowPolicy.RejectNew;

        /// <summary>总线音量通过它写进混音器。缺了它任何总线的音量都调不了，初始化会失败。</summary>
        public AudioMixer Mixer
        {
            get { return _mixer; }
        }

        /// <summary>音效定义与音乐定义没填总线时用它。缺了它初始化会失败。</summary>
        public AudioBus DefaultBus
        {
            get { return _defaultBus; }
        }

        /// <summary>整个模块同时能有多少条声音在播。只有音效占用槽位，音乐不占。</summary>
        public int SlotCount
        {
            get { return _slotCount; }
        }

        /// <summary>
        /// 槽位用满时的处置。三级上限里它是最后一关：这一级配成拒绝时，
        /// 音效定义与总线上配的"顶掉最旧"到不了。
        /// </summary>
        public OverflowPolicy SlotOverflow
        {
            get { return _slotOverflow; }
        }

        private void OnValidate()
        {
            if (_slotCount < 1)
                _slotCount = 1;
        }
    }
}
