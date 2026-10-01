using System;
using Cysharp.Threading.Tasks;

namespace GameFramework.Game
{
    /// <summary>
    /// 转场作业：先卸上一阶段，再加载下一阶段。由 TransitionGate 在遮罩下执行。
    /// </summary>
    public interface ITransitionJob
    {
        UniTask UnloadPrevious();
        UniTask LoadNext(IProgress<float> progress);
    }
}
