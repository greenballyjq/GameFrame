using System;

namespace GameFramework.Utility
{
    /// <summary>
    /// 值更改事件的属性
    /// </summary>
    /// <typeparam name="T">基础数据类型（int、char、float、double、string等）</typeparam>
    public class BindableProperty<T> where T : IEquatable<T>
    {
        /// <summary>
        /// Value 属性值更改时触发
        /// (oldValue, newValue)
        /// </summary>
        private Action<T, T> _onValueChanged;
        
        private T _value;
        public T Value
        {
            get => _value;
            set
            {
                if (_value.Equals(value)) return;
                T oldValue = _value;
                _value = value;
                _onValueChanged?.Invoke(oldValue, value);
            }
        }

        public BindableProperty()
        {
            
        }

        public BindableProperty(T initialValue)
        {
            _value = initialValue;
        }

        /// <summary>
        /// 添加值更改事件监听
        /// 参数：(oldValue, newValue)
        /// </summary>
        /// <param name="onValueChanged"></param>
        public void RegisterOnValueChanged(Action<T, T> onValueChanged)
        {
            _onValueChanged += onValueChanged;
        }
        
        /// <summary>
        /// 移除值更改事件监听
        /// </summary>
        /// <param name="onValueChanged"></param>
        public void UnregisterOnValueChanged(Action<T, T> onValueChanged)
        {
            _onValueChanged -= onValueChanged;
        }
        
        /// <summary>
        /// 移除所有值更改事件监听
        /// </summary>
        public void UnregisterAllOnValueChanged()
        {
            _onValueChanged = null;
        }
    }
}