using System;
using System.Collections.Generic;

namespace GameFramework.Serialize
{
    /// <summary>
    /// 保存的游戏物体组件序列化数据（包括子物体）
    /// </summary>
    [Serializable]
    public class SaveSerializedComponents
    {
        public SaveSerializedComponents[] Children { get; set; }
        public List<ComponentSerialized> ComponentSerializeList { get; set; }
    } 
}