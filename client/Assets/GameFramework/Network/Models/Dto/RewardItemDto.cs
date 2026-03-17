using System;

namespace GameFramework.Network.Models.Dto
{
    [Serializable]
    public class RewardItemDto
    {
        public string ItemID { get; set; }
        public int Quantity { get; set; }
    }
}