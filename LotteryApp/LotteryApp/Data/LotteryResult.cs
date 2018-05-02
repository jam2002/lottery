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
        public int MaxInterval { get; set; }

        /// <summary>
        /// 最近间隔次数
        /// </summary>
        public int LastInterval { get; set; }

        /// <summary>
        /// 最大连中次数
        /// </summary>
        public int MaxContinuous { get; set; }

        /// <summary>
        /// 最近连中次数
        /// </summary>
        public int LastContinuous { get; set; }

        /// <summary>
        /// 中奖位置列表
        /// </summary>
        public int[] HitPositions { get; set; }

        /// <summary>
        /// 中奖间隔列表
        /// </summary>
        public int[] HitIntervals { get; set; }

        /// <summary>
        /// 筛选条件
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// 筛选标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 任选投注条件
        /// </summary>
        public AnyFilter[] AnyFilters { get; set; }
    }
}
