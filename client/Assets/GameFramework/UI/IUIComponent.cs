namespace GameFramework.UI
{
    public interface IUIComponent
    {
        void Init();
        void CleanUp();
    }

    public interface IUIComponent<T>
    {
        void Init(T data);
        void CleanUp();
    }
}
