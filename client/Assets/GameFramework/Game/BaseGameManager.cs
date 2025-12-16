using UnityEngine;

namespace GameFramework.Game
{
    /// <summary>
    /// 游戏业务管理器基类
    /// </summary>
    public abstract class BaseGameManager : MonoBehaviour, IGameManager
    {
        [SerializeField] private bool dontDestroyOnLoad = true;
        
        public bool DonotDestroyOnLoad => dontDestroyOnLoad;
        
        public abstract void Init();
        public abstract void DoUpdate();
        public abstract void Release();
    }
}