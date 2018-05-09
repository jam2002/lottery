﻿namespace Lottery.Core.Data
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

        /// <summary>
        /// 过滤优先级从低到高排序
        /// </summary>
        public string OrderKey { get; set; }

        /// <summary>
        ///  热度：1：极冷；2：相当冷；3：冷；4：温；5：热；6：相当热；7：极热
        /// </summary>
        public int Heat { get; set; }

        /// <summary>
        /// 1: 间隔越来越小；2：间隔越来越大；3：间隔不变
        /// </summary>
        public int Trend { get; set; }
    }
}
