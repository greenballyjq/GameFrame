using Cysharp.Threading.Tasks;

namespace GameFramework.Core
{
    /// <summary>
    /// 管理器接口
    /// </summary>
    public interface IManager
    {
        UniTask InitAsync();
        void Dispose();
    }

    /// <summary>
    /// 需要每帧更新的管理器
    /// </summary>
    public interface IManagerUpdatable
    {
        void DoUpdate(float dt);
    }
}
