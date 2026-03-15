using UnityEngine;

namespace GameFramework.Mall.Models
{
    /// <summary>
    /// 商城商品 VO 基类 - 包含所有游戏通用的商品展示字段
    /// </summary>
    public class BaseMallItemVo
    {
        public long ID { get; set; }
        public string ItemID { get; set; }
        public Sprite Sprite { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int PriceCoin { get; set; }
        public int SortOrder { get; set; }
        public bool DisplayStatus { get; set; }
        public string MainType { get; set; }
        public string SubType { get; set; }
        public ulong Cooldown { get; set; }
    }
}
