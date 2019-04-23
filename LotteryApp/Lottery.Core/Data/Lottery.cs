namespace Lottery.Core.Data
{
    public class Lottery
    {
        public string Key { get; set; }

        public string DisplayName { get; set; }

        public string RegexPattern { get; set; }

        public int StartIndex { get; set; }

        public int Length { get; set; }

        /// <summary>
        /// 1: API接口；2：抓HTML
        /// </summary>
        public int Source { get; set; }

        /// <summary>
        /// 是否可以出对子
        /// </summary>
        public bool HasPair { get; set; }

        /// <summary>
        /// 是否可玩不定胆
        /// </summary>
        public bool HasDynamic { get; set; }

        /// <summary>
        /// 最大投注数
        /// </summary>
        public int MaxBetCount { get; set; }

        /// <summary>
        /// 交易时间
        /// </summary>
        public string[] TradingHours { get; set; }

        public int[] IndexKeys { get; set; }

        public Lottery()
        {
            Source = 1;
        }
    }
}
