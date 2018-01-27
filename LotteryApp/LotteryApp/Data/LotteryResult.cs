namespace LotteryApp.Data
{
    public class LotteryResult
    {
        /// <summary>
        /// 过滤后的投注号码
        /// </summary>
        public LotteryNumber[] Numbers { get; set; }

        /// <summary>
        /// 投注数
        /// </summary>
        public int BetCount { get; set; }

        /// <summary>
        /// 投注金额
        /// </summary>
        public double BetAmount { get; set; }

        /// <summary>
        /// 最大中奖次数
        /// </summary>
        public int HitCount { get; set; }

        /// <summary>
        /// 二码不定胆中奖次数
        /// </summary>
        public int PosHitCount { get; set; }

        /// <summary>
        /// 最大间隔次数
        /// </summary>
        public int MaxIntervalCount { get; set; }

        /// <summary>
        /// 最近间隔次数
        /// </summary>
        public int LastIntervalCount { get; set; }

        /// <summary>
        /// 中奖位置列表
        /// </summary>
        public int[] HitPositions { get; set; }

        /// <summary>
        /// 筛选条件
        /// </summary>
        public string Filter { get; set; }
    }
}
