namespace Lottery.Core.Data
{
    public enum FactorTypeEnum
    {
        Span = 1,
        Odd = 2,
        Size = 3,
        Prime = 4,
        Sum = 5,
        Max = 6,
        Min = 7,
        Hundred = 8,
        Decade = 9,
        Unit = 10,

        /// <summary>
        /// 去重数数量
        /// </summary>
        Distinct = 11,

        /// <summary>
        /// 胆码 
        /// </summary>
        Award = 12,

        /// <summary>
        /// 顺序胆码值，比如 012，125
        /// </summary>
        SequenceKey = 13,

        /// <summary>
        /// 不定胆两码组合，比如01，02
        /// </summary>
        DynamicPosition = 14,

        /// <summary>
        /// 五星形态，1：组120；2：组60；3：组30；4：组20；5：组10；6：组5；
        /// </summary>
        FiveStarForm = 15,

        /// <summary>
        /// 千位
        /// </summary>
        Thousand = 16,

        /// <summary>
        /// 万位
        /// </summary>
        Wan = 17
    }
}
