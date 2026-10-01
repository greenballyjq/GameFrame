using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameFramework.UI
{
    /// <summary>
    /// 把矩形从起点补间到捕获的休息位。同节点存在 IUIMotionBase 时只改其基准位，闲时漂浮仍叠在基准位上。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class UIRevealToRest : MonoBehaviour
    {
        /// <summary>
        /// 入场起点的计算方式。
        /// </summary>
        public enum FromMode
        {
            /// <summary>
            /// 起点为休息位加上 Config.ExtraOffset。
            /// </summary>
            Offset,

            /// <summary>
            /// 起点的 X 取屏幕中线，再叠加 Config.ExtraOffset。
            /// </summary>
            ScreenCenterX,

            /// <summary>
            /// 起点位于屏幕顶边之外，再叠加 Config.ExtraOffset。
            /// </summary>
            ScreenOutsideTop
        }

        /// <summary>
        /// 入场补间使用的缓动。OutCubic 的底层数值为 9，与已序列化预制体中的缓动序号一致。
        /// </summary>
        public enum RevealEase
        {
            /// <summary>
            /// 先快后慢。归一化时间 t 的结果为 1 减 (1 减 t) 的三次方。
            /// </summary>
            OutCubic = 9
        }

        /// <summary>
        /// 单次入场的起点、时长、延迟和缓动。
        /// </summary>
        [Serializable]
        public class Config
        {
            /// <summary>
            /// 起点计算方式。默认 Offset。
            /// </summary>
            [Tooltip("起点怎么算：相对休息位偏移 / 屏幕中线 X / 屏幕顶外")]
            public FromMode From = FromMode.Offset;

            /// <summary>
            /// 叠在计算出的起点上的额外偏移，单位为 anchoredPosition。
            /// </summary>
            [Tooltip("叠在计算出的起点上的额外偏移")]
            public Vector2 ExtraOffset;

            /// <summary>
            /// 延迟结束后的补间时长，单位秒。小于等于 0 时，延迟结束后直接落到休息位。
            /// </summary>
            [Tooltip("入场时长（秒）")]
            public float Duration = 0.55f;

            /// <summary>
            /// 开始移动前的等待，单位秒。小于 0 时按 0 处理。
            /// </summary>
            [Tooltip("入场前等待（秒），用来错开多张纸")]
            public float Delay;

            /// <summary>
            /// 补间缓动。默认 OutCubic。
            /// </summary>
            public RevealEase Ease = RevealEase.OutCubic;

            /// <summary>
            /// 为真时用不受 timeScale 影响的时间累计延迟和时长，为假时用 Time.deltaTime。
            /// </summary>
            [Tooltip("不受 TimeScale 影响")]
            public bool UseUnscaledTime;

            /// <summary>
            /// 为真时，播放模式下 OnEnable 先把矩形放到起点。
            /// </summary>
            [Tooltip("Enable 时先落到起点，避免闪一帧休息位")]
            public bool ApplyStartOnEnable = true;
        }

        [SerializeField] private Config _config = new Config();

        private RectTransform _rect;
        private IUIMotionBase _motionBase;
        private Vector2 _restAnchoredPosition;
        private bool _restCaptured;
        private bool _running;
        private bool _fromCaptured;
        private float _elapsed;
        private Vector2 _from;
        private Vector2 _to;
        private UniTaskCompletionSource _completion;

        /// <summary>
        /// 当前入场配置。修改其中的字段会影响尚未开始的等待和之后的补间。
        /// </summary>
        public Config RevealConfig => _config;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            TryGetComponent(out _motionBase);
            CaptureRest();
        }

        private void OnEnable()
        {
            if (Application.isPlaying && _config.ApplyStartOnEnable)
                SnapToStart();
        }

        private void OnDisable()
        {
            KillTween();
        }

        private void Update()
        {
            if (!_running)
                return;

            _elapsed += _config.UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float delay = _config.Delay > 0f ? _config.Delay : 0f;
            if (_elapsed < delay)
                return;

            if (!_fromCaptured)
            {
                _from = GetBase();
                _fromCaptured = true;
            }

            float duration = _config.Duration;
            float moved = _elapsed - delay;
            if (duration <= 0f || moved >= duration)
            {
                SetBase(_to);
                Finish();
                return;
            }

            float t = EvaluateEase(moved / duration);
            SetBase(Vector2.LerpUnclamped(_from, _to, t));
        }

        /// <summary>
        /// 停掉进行中的补间并立刻把基准位设为起点。没有进行中的补间时只改基准位。
        /// </summary>
        public void SnapToStart()
        {
            CaptureRest();
            KillTween();
            SetBase(ResolveStart());
        }

        /// <summary>
        /// 停掉进行中的补间并立刻把基准位设为休息位。没有进行中的补间时只改基准位。
        /// </summary>
        public void SnapToRest()
        {
            CaptureRest();
            KillTween();
            SetBase(_restAnchoredPosition);
        }

        /// <summary>
        /// 从起点补间到本次调用时捕获的休息位。物体未激活时会先激活。时长和延迟都小于等于 0 时立刻落到休息位并返回已完成的任务。
        /// 补间自然结束、或被 KillTween 停掉时，返回的任务完成且不抛出异常。物体禁用时 OnDisable 会停掉补间。
        /// </summary>
        public UniTask PlayAsync()
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            CaptureRest();
            KillTween();
            SetBase(ResolveStart());

            Vector2 rest = _restAnchoredPosition;
            if (_config.Duration <= 0f && _config.Delay <= 0f)
            {
                SetBase(rest);
                return UniTask.CompletedTask;
            }

            _to = rest;
            _elapsed = 0f;
            _fromCaptured = false;
            _running = true;
            _completion = new UniTaskCompletionSource();
            return _completion.Task;
        }

        /// <summary>
        /// 启动入场且不等待结束。停掉规则与 PlayAsync 相同。
        /// </summary>
        public void Play()
        {
            PlayAsync().Forget();
        }

        /// <summary>
        /// 停掉进行中的补间并留在当前基准位。没有进行中的补间时为空操作。进行中的 PlayAsync 会因此完成。
        /// </summary>
        public void KillTween()
        {
            if (!_running && _completion == null)
                return;

            Finish();
        }

        private void Finish()
        {
            _running = false;
            _fromCaptured = false;
            UniTaskCompletionSource completion = _completion;
            _completion = null;
            completion?.TrySetResult();
        }

        private void CaptureRest()
        {
            if (_restCaptured || _rect == null)
                return;

            _restAnchoredPosition = _rect.anchoredPosition;
            _restCaptured = true;
        }

        private Vector2 ResolveStart()
        {
            switch (_config.From)
            {
                case FromMode.ScreenCenterX:
                    return UIRectUtility.ScreenCenterX(_rect, _restAnchoredPosition, _config.ExtraOffset);
                case FromMode.ScreenOutsideTop:
                    return UIRectUtility.ScreenOutsideTop(_rect, _restAnchoredPosition, _config.ExtraOffset);
                default:
                    return _restAnchoredPosition + _config.ExtraOffset;
            }
        }

        private Vector2 GetBase()
        {
            return _motionBase != null ? _motionBase.BaseAnchoredPosition : _rect.anchoredPosition;
        }

        private void SetBase(Vector2 value)
        {
            if (_motionBase != null)
                _motionBase.BaseAnchoredPosition = value;
            else
                _rect.anchoredPosition = value;
        }

        private float EvaluateEase(float t)
        {
            switch (_config.Ease)
            {
                default:
                    float inv = 1f - Mathf.Clamp01(t);
                    return 1f - inv * inv * inv;
            }
        }
    }
}
