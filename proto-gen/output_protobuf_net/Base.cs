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
