using System;
using System.Collections.Generic;
using System.Linq;

namespace GameFramework.Network.Models.Dto
{
    /// <summary>
    /// 配置接口外层响应 { code, data: { content: "..." }, message }
    /// </summary>
    [Serializable]
    public class ConfigResponseWrapper
    {
        public int code { get; set; }
        public ConfigResponseData data { get; set; }
        public string message { get; set; }
    }

    [Serializable]
    public class ConfigResponseData
    {
        public string content { get; set; }
    }

    [Serializable]
    public class DailyCheckInConfigWrapper
    {
        public List<DailyCheckInConfigDto> configs { get; set; }
    }

    [Serializable]
    public class DailyCheckInConfigDto
    {
        public string Name { get; set; }
        public int TotalDays { get; set; }
        public string RefreshTimePoint { get; set; }
        public bool IsActive { get; set; }
        public long ID { get; set; }
        public List<DailyCheckInStageDto> Stages { get; set; }
    }

    [Serializable]
    public class DailyCheckInStageDto
    {
        public int day { get; set; }
        public string rewardCoin { get; set; }
        public string rewardDescription { get; set; }
        public List<RewardItemDto> rewardItems { get; set; }
    }
    
    // public class 

    [Serializable]
    public class UserLevelConfigDto
    {
        public long ID { get; set; }
        public string AssociatedGame { get; set; }
        public string LevelChange { get; set; }
        public double RequiredOnlineHours { get; set; }

        public int[] GetLevels()
        {
            return LevelChange.Split(',').Select(int.Parse).ToArray();
        }
    }
}
