using UnityEngine;

namespace GameFramework.Models.Rank
{
    public class RankEntryVo
    {
        public string Uuid { get; set; }
        public string Nickname { get; set; }
        public ulong Score { get; set; }
        public uint Rank { get; set; }
        public Sprite Avatar { get; set; }
    }
}