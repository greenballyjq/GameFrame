namespace GameFramework.Game
{
    /// <summary>
    /// 游戏业务管理器接口
    /// </summary>
    public interface IGameManager
    {
        /// <summary>
        /// 初始化管理器
        /// </summary>
        void Init();
        
        // void StartGame();

        /// <summary>
        /// 每帧更新
        /// </summary>
        void Update();

        /// <summary>
        /// 释放资源
        /// </summary>
        void Release();
    }
}