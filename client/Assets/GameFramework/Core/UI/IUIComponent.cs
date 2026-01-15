namespace GameFramework.Core.UI
{
    /// <summary>
    /// UI子组件接口
    /// </summary>
    public interface IUIComponent
    {
        /// <summary>
        /// 初始化组件
        /// </summary>
        void Init();

        /// <summary>
        /// 清理组件
        /// </summary>
        void CleanUp();
    }

    /// <summary>
    /// UI子组件接口（泛型版本）
    /// </summary>
    /// <typeparam name="T">组件初始化数据类型</typeparam>
    public interface IUIComponent<T>
    {
        /// <summary>
        /// 初始化组件
        /// </summary>
        /// <param name="data">组件初始化数据</param>
        void Init(T data);

        /// <summary>
        /// 清理组件
        /// </summary>
        void CleanUp();
    }
}

