using System;
using GameFramework.Mall.Models;

namespace GameFramework.Network.Models.Dto
{
    [Serializable]
    public class ItemDto
    {
        public long ID { get; set; }
        public string ItemID { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public ItemDto()
        {
        }

        public ItemDto(BaseMallItemDto dto)
        {
            ID = dto.ID;
            ItemID = dto.ItemID;
            Icon = dto.Icon;
            Name = dto.Name;
            Description = dto.Description;
        }

        public override string ToString()
        {
            return $"[ItemID: {ItemID}, Icon: {Icon}, Name: {Name}, Description: {Description}]";
        }
    }
}
