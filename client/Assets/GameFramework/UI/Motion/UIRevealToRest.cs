using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace GameFramework.UI
{
    /// <summary>
    /// 从相对屏幕 / 相对休息位的起点，补间回 Prefab 布局位置。
    /// 若同节点有 <see cref="IUIMotionBase"/>，只改基准位，不打断闲时漂浮。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class UIRevealToRest : MonoBehaviour
    {
        public enum FromMode
        {
            Offset,
            ScreenCenterX,
            ScreenOutsideTop
        }

        [Serializable]
        public class Config
        {
            [Tooltip("起点怎么算：相对休息位偏移 / 屏幕中线 X / 屏幕顶外")]
            public FromMode From = FromMode.Offset;

            [Tooltip("叠在计算出的起点上的额外偏移")]
            public Vector2 ExtraOffset;

            [Tooltip("入场时长（秒）")]
            public float Duration = 0.55f;

            [Tooltip("入场前等待（秒），用来错开多张纸")]
            public float Delay;

            public Ease Ease = Ease.OutCubic;

            [Tooltip("不受 TimeScale 影响")]
            public bool UseUnscaledTime;

            [Tooltip("Enable 时先落到起点，避免闪一帧休息位")]
            public bool ApplyStartOnEnable = true;
        }

        [SerializeField] private Config _config = new Config();

        private RectTransform _rect;
        private IUIMotionBase _motionBase;
        private Vector2 _restAnchoredPosition;
        private bool _restCaptured;
        private Tween _tween;

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

        public void SnapToStart()
        {
            CaptureRest();
            KillTween();
            SetBase(ResolveStart());
        }

        public void SnapToRest()
        {
            CaptureRest();
            KillTween();
            SetBase(_restAnchoredPosition);
        }

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

            _tween = DOTween.To(GetBase, SetBase, rest, _config.Duration)
                .SetEase(_config.Ease)
                .SetDelay(_config.Delay)
                .SetUpdate(_config.UseUnscaledTime)
                .SetLink(gameObject)
                .SetTarget(this);

            return _tween.ToUniTask();
        }

        public void Play()
        {
            PlayAsync().Forget();
        }

        public void KillTween()
        {
            if (_tween != null && _tween.IsActive())
                _tween.Kill();
            _tween = null;
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
    }
}
