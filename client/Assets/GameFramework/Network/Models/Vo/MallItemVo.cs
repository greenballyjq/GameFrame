using GameFramework.Mall.Models;
using GameFramework.Network.Models.Dto;

namespace GameFramework.Network.Models.Vo
{
    public class MallItemVo : BaseMallItemVo
    {
        public bool IsVideoReward { get; set; }

        public MallItemVo()
        {
        }

        public MallItemVo(MallItemDto dto)
        {
            ID = dto.ID;
            ItemID = dto.ItemID;
            Name = dto.Name;
            Description = dto.Description;
            PriceCoin = dto.PriceCoin;
            SortOrder = dto.SortOrder;
            DisplayStatus = dto.DisplayStatus;
            MainType = dto.MainType;
            SubType = dto.SubType;
            IsVideoReward = dto.IsVideoReward;
        }
    }
}
