using System;
using GameFramework.Mall.Models;

namespace GameFramework.Network.Models.Dto
{
    [Serializable]
    public class MallItemDto : BaseMallItemDto
    {
        public bool IsVideoReward { get; set; }

        public override string ToString()
        {
            return $"[ItemID: {ItemID}, Name: {Name}, PriceCoin: {PriceCoin}, SortOrder: {SortOrder}, IsVideoReward: {IsVideoReward}]";
        }
    }
}
