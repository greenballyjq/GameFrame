namespace GameFramework.Serialize
{
    /// <summary>
    /// 脚本自定义序列化接口
    /// </summary>
    public interface ISerializableMonoBehaviour
    {
        string SerializeMonoBehaviour();

        void DeserializeMonoBehaviour(string serialized);
    }
}