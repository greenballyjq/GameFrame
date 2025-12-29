using System;

namespace GameFramework.Serialize
{
    /// <summary>
    /// Health脚本的序列化模型
    /// </summary>
    [Serializable]
    public class HealthSerializable
    {
        public float TotalHp { get; set; }
        public float CurrentHp { get; set; }
        public float DamageMultiplier { get; set; }
    }

    /// <summary>
    /// EnemyBase脚本的序列化模型
    /// </summary>
    public class EnemyBaseSerializable
    {
        public bool IsDead { get; set; }
    }
}