namespace GameFramework.Serialize.Interfaces
{
    /// <summary>
    /// 可序列化接口
    /// </summary>
    public interface ISerializable
    {
        string Serialize();
        void Deserialize(string serialized);
        
        // TODO: SerializeBin And DeserializeBin 二进制序列化和反序列化
    }
}