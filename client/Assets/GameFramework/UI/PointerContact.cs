using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace GameFramework.UI
{
    public readonly struct PointerContact
    {
        public bool IsHover { get; }

        private PointerContact(bool isHover)
        {
            IsHover = isHover;
        }

        public static PointerContact From(PointerEventData eventData)
        {
            return new PointerContact(IsHoverContact(eventData));
        }

        private static bool IsHoverContact(PointerEventData eventData)
        {
            ExtendedPointerEventData extended = eventData as ExtendedPointerEventData;
            if (extended == null)
                return false;

            if (!CanHover(extended))
                return false;

            if (IsPressed(extended))
                return false;

            return eventData.delta.sqrMagnitude > 0f;
        }

        private static bool CanHover(ExtendedPointerEventData extended)
        {
            if (extended.device is Touchscreen)
                return false;

            if (extended.pointerType == UIPointerType.Touch)
                return false;

            return extended.device is Mouse || extended.device is Pen;
        }

        private static bool IsPressed(ExtendedPointerEventData extended)
        {
            if (extended.device is Mouse mouse)
                return mouse.leftButton.isPressed
                    || mouse.rightButton.isPressed
                    || mouse.middleButton.isPressed;

            if (extended.device is Pen pen)
                return pen.tip.isPressed;

            return true;
        }
    }
}
