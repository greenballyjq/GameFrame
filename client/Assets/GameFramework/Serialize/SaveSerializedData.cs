using System;
using System.Collections.Generic;

namespace GameFramework.Serialize
{
    /// <summary>
    /// 发送给服务端的序列化保存数据
    /// </summary>
    [Serializable]
    public class SaveSerializedData
    {
        public List<SaveSerializedGameObject> SaveSerializedGameObjectList { get; set; }
    }
}