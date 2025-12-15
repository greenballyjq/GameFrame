using UnityEngine;

namespace GameFramework.Game
{
    /// <summary>
    /// 游戏业务管理器基类
    /// </summary>
    public abstract class BaseGameManager : MonoBehaviour, IGameManager
    {
        public abstract void Init();
        // public abstract void StartGame();
        public abstract void Update();
        public abstract void Release();
    }
}