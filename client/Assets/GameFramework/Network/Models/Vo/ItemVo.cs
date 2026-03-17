using GameFramework.Mall.Models;
using GameFramework.Network.Models.Dto;
using UnityEngine;

namespace GameFramework.Network.Models.Vo
{
    public class ItemVo
    {
        public long ID { get; set; }
        public string ItemID { get; set; }
        public Sprite Sprite { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SubType { get; set; }

        public ItemVo()
        {
        }

        public ItemVo(ItemDto dto)
        {
            ID = dto.ID;
            ItemID = dto.ItemID;
            Name = dto.Name;
            Description = dto.Description;
        }

        public ItemVo(BaseMallItemVo vo)
        {
            ID = vo.ID;
            ItemID = vo.ItemID;
            Sprite = vo.Sprite;
            Name = vo.Name;
            Description = vo.Description;
        }
    }
}
