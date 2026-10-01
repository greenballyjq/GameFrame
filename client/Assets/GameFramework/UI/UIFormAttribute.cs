using System;

namespace GameFramework.UI
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class UIFormAttribute : Attribute
    {
        public UILayer Layer { get; }
        public string Address { get; }
        public UILifetime Lifetime { get; }

        public UIFormAttribute(UILayer layer, string address = null, UILifetime lifetime = UILifetime.Round)
        {
            Layer = layer;
            Address = address;
            Lifetime = lifetime;
        }
    }
}
