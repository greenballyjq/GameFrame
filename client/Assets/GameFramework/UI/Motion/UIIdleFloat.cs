using System;
using UnityEngine;

namespace GameFramework.UI
{
    /// <summary>
    /// 闲时漂浮。位移叠在 <see cref="BaseAnchoredPosition"/> 上，入场动画只改基准位。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class UIIdleFloat : MonoBehaviour, IUIMotionBase
    {
        [Serializable]
        public class Config
        {
            [Tooltip("位移幅度（像素）")]
            public Vector2 Amplitude = new Vector2(3f, 7f);

            [Tooltip("一个来回的时长（秒）")]
            public float Period = 2.8f;

            [Range(0f, 360f)]
            [Tooltip("X 相位（度），用来错开多个面板")]
            public float PhaseX;

            [Range(0f, 360f)]
            [Tooltip("Y 相位（度）")]
            public float PhaseY = 80f;

            [Tooltip("Z 轴微旋转幅度（度）")]
            public float RotationAmplitude = 0.3f;

            [Range(0f, 360f)]
            [Tooltip("旋转相位（度）")]
            public float RotationPhase;

            [Tooltip("缩放呼吸幅度（相对 1）")]
            public float ScaleAmplitude;

            [Tooltip("不受 TimeScale 影响")]
            public bool UseUnscaledTime;
        }

        [SerializeField] private Config _config = new Config();

        private RectTransform _rect;
        private Vector2 _baseAnchoredPosition;
        private Vector3 _baseLocalEuler;
        private Vector3 _baseLocalScale;
        private bool _captured;

        public Vector2 BaseAnchoredPosition
        {
            get => _baseAnchoredPosition;
            set => _baseAnchoredPosition = value;
        }

        public Config FloatConfig => _config;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            CapturePose();
        }

        private void OnDisable()
        {
            if (!_captured || _rect == null)
                return;

            _rect.anchoredPosition = _baseAnchoredPosition;
            _rect.localEulerAngles = _baseLocalEuler;
            _rect.localScale = _baseLocalScale;
        }

        private void LateUpdate()
        {
            if (!_captured)
                return;

            float period = _config.Period > 0.0001f ? _config.Period : 1f;
            float time = _config.UseUnscaledTime ? Time.unscaledTime : Time.time;
            float wave = time * (Mathf.PI * 2f / period);

            Vector2 offset = new Vector2(
                Mathf.Sin(wave + _config.PhaseX * Mathf.Deg2Rad) * _config.Amplitude.x,
                Mathf.Sin(wave + _config.PhaseY * Mathf.Deg2Rad) * _config.Amplitude.y);

            _rect.anchoredPosition = _baseAnchoredPosition + offset;

            if (_config.RotationAmplitude != 0f)
            {
                float z = Mathf.Sin(wave + _config.RotationPhase * Mathf.Deg2Rad) * _config.RotationAmplitude;
                _rect.localEulerAngles = _baseLocalEuler + new Vector3(0f, 0f, z);
            }

            if (_config.ScaleAmplitude != 0f)
            {
                float scale = 1f + Mathf.Sin(wave) * _config.ScaleAmplitude;
                _rect.localScale = Vector3.Scale(_baseLocalScale, new Vector3(scale, scale, 1f));
            }
        }

        private void CapturePose()
        {
            if (_captured || _rect == null)
                return;

            _baseAnchoredPosition = _rect.anchoredPosition;
            _baseLocalEuler = _rect.localEulerAngles;
            _baseLocalScale = _rect.localScale;
            _captured = true;
        }
    }
}
