using ProtoBuf;
using GameProto;

namespace TestProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("测试 protobuf-net 代码生成...");

            // 创建测试数据
            var playerBase = new PlayerBase
            {
                Createtime = 1672531200000, // 2023-01-01 00:00:00 UTC
                Offtime = 1672617600000,    // 2023-01-02 00:00:00 UTC
                Gametime = 86400000         // 24小时的毫秒数
            };

            var s2cBase = new S2C_Base
            {
                PlayerBase = playerBase
            };

            // 序列化
            using var stream = new MemoryStream();
            Serializer.Serialize(stream, s2cBase);
            
            Console.WriteLine($"序列化成功，数据大小: {stream.Length} 字节");

            // 反序列化
            stream.Position = 0;
            var deserializedData = Serializer.Deserialize<S2C_Base>(stream);

            Console.WriteLine($"反序列化成功:");
            Console.WriteLine($"- 建号时间: {deserializedData.PlayerBase.Createtime}");
            Console.WriteLine($"- 离线时间: {deserializedData.PlayerBase.Offtime}");
            Console.WriteLine($"- 游戏时间: {deserializedData.PlayerBase.Gametime}");

            // 测试空消息
            var c2sBase = new C2S_Base();
            using var stream2 = new MemoryStream();
            Serializer.Serialize(stream2, c2sBase);
            Console.WriteLine($"空消息序列化成功，数据大小: {stream2.Length} 字节");

            Console.WriteLine("✅ 所有测试通过！");
        }
    }
} 