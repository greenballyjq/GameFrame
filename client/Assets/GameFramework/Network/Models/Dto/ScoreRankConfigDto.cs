using System;

namespace GameFramework.Network.Models.Dto
{
    /// <summary>
    /// 积分称号配置dto
    /// </summary>
    [Serializable]
    public class ScoreRankConfigDto
    {
        public int ID { get; set; }
        public string Rank { get; set; }
        public int MinScore { get; set; }
        public int MaxScore { get; set; }
        public string GameType { get; set; }
        public string Title { get; set; }
    }
}