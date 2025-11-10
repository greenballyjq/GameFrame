namespace GameFramework.Core
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
}

