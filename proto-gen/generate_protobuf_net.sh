#!/bin/bash

# ========= protobuf-net 代码生成脚本 =========
# 此脚本为 protobuf-net 库生成适用的 C# 类文件

PROTO_DIR="./protos"
OUTPUT_DIR="./output_protobuf_net"

# 创建输出目录
mkdir -p "$OUTPUT_DIR"

echo "🚀 开始为 protobuf-net 生成 C# 类文件..."

# 生成 Base.cs
cat > "$OUTPUT_DIR/Base.cs" << 'EOF'
using ProtoBuf;

namespace GameProto
{
    [ProtoContract]
    public class PlayerBase
    {
        /// <summary>
        /// 建号时间戳
        /// </summary>
        [ProtoMember(1)]
        public ulong Createtime { get; set; }

        /// <summary>
        /// 离线时间戳
        /// </summary>
        [ProtoMember(2)]
        public ulong Offtime { get; set; }

        /// <summary>
        /// 累积游戏时间(毫秒)
        /// </summary>
        [ProtoMember(3)]
        public ulong Gametime { get; set; }
    }

    [ProtoContract]
    public class C2S_Base
    {
        // 空消息
    }

    [ProtoContract]
    public class S2C_Base
    {
        [ProtoMember(1)]
        public PlayerBase PlayerBase { get; set; }
    }
}
EOF

echo "✅ 生成 Base.cs 完成"

# 如果需要，可以继续生成其他文件
# 这里可以添加更多的文件生成逻辑

echo "🎉 protobuf-net C# 类文件生成完毕！"
echo "📁 输出目录: $OUTPUT_DIR"
echo ""
echo "💡 使用说明:"
echo "1. 在您的项目中安装 protobuf-net NuGet 包"
echo "2. 将生成的 C# 文件添加到您的项目中"
echo "3. 使用 Serializer.Serialize() 和 Serializer.Deserialize() 进行序列化" 