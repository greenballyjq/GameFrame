using GameFramework.Audio;
using GameFramework.Core;
using GameFramework.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameFramework.UI
{
    public class UIButton : Button
    {
        [SerializeField] private SfxCue _clickOverride;
        [SerializeField] private float _hoverBrightness;

        private static IUIButtonAudioDefaults _defaults;

        private AudioManager _audioManager;
        private bool _hoverTintApplied;
        private bool _loggedPlayFailure;

        public static void SetDefaults(IUIButtonAudioDefaults defaults)
        {
            _defaults = defaults;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ApplyHoverTint();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            if (!IsActive() || !IsInteractable())
                return;

            if (!PointerContact.From(eventData).IsHover)
                return;

            if (_defaults == null)
                return;

            Play(_defaults.Hover);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            bool shouldClick = CanInvokeClick(eventData);
            base.OnPointerClick(eventData);
            if (shouldClick)
                Play(ResolveClickCue());
        }

        private void ApplyHoverTint()
        {
            if (_hoverTintApplied || _hoverBrightness <= 0f)
                return;

            if (transition == Transition.None)
                transition = Transition.ColorTint;

            ColorBlock block = colors;
            Color highlighted = Color.white * _hoverBrightness;
            highlighted.a = 1f;
            block.highlightedColor = highlighted;
            colors = block;
            _hoverTintApplied = true;
        }

        private bool CanInvokeClick(PointerEventData eventData)
        {
            return eventData.button == PointerEventData.InputButton.Left
                && IsActive()
                && IsInteractable();
        }

        private SfxCue ResolveClickCue()
        {
            if (_clickOverride != null)
                return _clickOverride;

            return _defaults != null ? _defaults.Click : null;
        }

        private void Play(SfxCue cue)
        {
            if (cue == null)
                return;

            if (_audioManager == null && GameFrameworkManager.Instance != null)
                _audioManager = GameFrameworkManager.Instance.GetManager<AudioManager>();

            if (_audioManager == null)
                return;

            PlayResult result = _audioManager.Play(cue);
            if (result.Succeeded || _loggedPlayFailure)
                return;

            _loggedPlayFailure = true;
            Log.Warning(
                "[UIButton] 按钮音效被拒绝：" + result.Failure + "（定义 " + cue.name + "）。" +
                "同类失败只记录一次。",
                this);
        }
    }
}
