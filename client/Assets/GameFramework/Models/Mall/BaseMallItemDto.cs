using System;

namespace GameFramework.Mall.Models
{
    /// <summary>
    /// 商城商品 DTO 基类 - 包含所有游戏通用的商品字段
    /// </summary>
    [Serializable]
    public class BaseMallItemDto
    {
        public long ID { get; set; }
        public string ItemID { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int PriceCoin { get; set; }
        public int SortOrder { get; set; }
        public bool DisplayStatus { get; set; }
        public string MainType { get; set; }
        public string SubType { get; set; }

        public override string ToString()
        {
            return $"[ItemID: {ItemID}, Name: {Name}, PriceCoin: {PriceCoin}, SortOrder: {SortOrder}]";
        }
    }
}
