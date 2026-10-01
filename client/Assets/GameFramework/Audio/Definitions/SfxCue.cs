using UnityEngine;

namespace GameFramework.Audio
{
    /// <summary>
    /// 一条音效的内容定义：播什么、多响、多重要、走哪条总线。
    /// </summary>
    [CreateAssetMenu(fileName = "SfxCue", menuName = "GameFramework/Audio/Sfx Cue")]
    public class SfxCue : ScriptableObject
    {
        [SerializeField] private AudioClip[] _clips;
        [SerializeField] private ClipSelection _selection = ClipSelection.ShuffleRound;
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;
        [SerializeField, Range(0.01f, 3f)] private float _pitchMin = 1f;
        [SerializeField, Range(0.01f, 3f)] private float _pitchMax = 1f;
        [SerializeField] private bool _loop;
        [SerializeField, Range(0, 256)] private int _priority = 128;
        [SerializeField] private bool _spatial;
        [SerializeField] private AudioBus _bus;
        [SerializeField] private int _maxInstances = -1;
        [SerializeField] private OverflowPolicy _overflow = OverflowPolicy.RejectNew;

        /// <summary>可播的片段数量。片段数组里的空引用不计入。</summary>
        public int ClipCount
        {
            get
            {
                if (_clips == null)
                    return 0;

                int count = 0;
                for (int i = 0; i < _clips.Length; i++)
                {
                    if (_clips[i] != null)
                        count++;
                }

                return count;
            }
        }

        /// <summary>多片段时的选取方式。单片段时不起作用。</summary>
        public ClipSelection Selection
        {
            get { return _selection; }
        }

        /// <summary>这条音效的播放音量，与总线音量相乘。</summary>
        public float Volume
        {
            get { return _volume; }
        }

        /// <summary>音高取值区间的两端。两者写反时按两数之间取值，并在编辑器里给出告警。</summary>
        public float PitchMin
        {
            get { return _pitchMin; }
        }

        public float PitchMax
        {
            get { return _pitchMax; }
        }

        /// <summary>循环播放的音效不会自然结束，只能被停止或淘汰。</summary>
        public bool Loop
        {
            get { return _loop; }
        }

        /// <summary>数字越大越优先被淘汰，0 最关键。这一方向与引擎音源的优先级一致。</summary>
        public int Priority
        {
            get { return _priority; }
        }

        /// <summary>空间化。为真时必须给位置，否则受理会被拒绝。</summary>
        public bool Spatial
        {
            get { return _spatial; }
        }

        /// <summary>输出分组。留空时用配置资产上的默认总线。</summary>
        public AudioBus Bus
        {
            get { return _bus; }
        }

        /// <summary>这条音效同时最多几条。负数不限，0 表示禁止，正数为上限。</summary>
        public int MaxInstances
        {
            get { return _maxInstances; }
        }

        /// <summary>到达上限时对既有实例做什么。</summary>
        public OverflowPolicy Overflow
        {
            get { return _overflow; }
        }

        /// <summary>取第 index 个非空片段。index 的范围由 ClipCount 界定。</summary>
        public AudioClip ClipAt(int index)
        {
            if (_clips == null)
                return null;

            int seen = 0;
            for (int i = 0; i < _clips.Length; i++)
            {
                if (_clips[i] == null)
                    continue;

                if (seen == index)
                    return _clips[i];

                seen++;
            }

            return null;
        }

        private void OnValidate()
        {
            if (_pitchMin > _pitchMax)
            {
                Debug.LogWarning(
                    "[Audio] " + name + " 的音高下界大于上界，实际取值会落在这两个数之间。",
                    this);
            }

            if (_maxInstances < -1)
                _maxInstances = -1;
        }
    }
}
