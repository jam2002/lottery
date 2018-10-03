namespace Lottery.Core.Data
{
    /// <summary>
    /// 统计参考因子
    /// </summary>
    public class ReferenceFactor
    {
        /// <summary>
        /// 键值
        /// </summary>
        public int Key { get; set; }

        /// <summary>
        /// 类别
        /// </summary>
        public FactorTypeEnum Type { get; set; }

        /// <summary>
        /// 出现次数
        /// </summary>
        public int OccurCount { get; set; }

        /// <summary>
        /// 最大间隔次数
        /// </summary>
        public int MaxInterval { get; set; }

        /// <summary>
        /// 最近间隔次数
        /// </summary>
        public int LastInterval { get; set; }

        /// <summary>
        /// 出现位置列表
        /// </summary>
        public int[] OccurPositions { get; set; }

        /// <summary>
        /// 中奖间隔列表
        /// </summary>
        public int[] HitIntervals { get; set; }


        public int FailureCount { get; set; }
    }
}
