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
        Wan = 17,

        RepeatNumber = 24,

        /// <summary>
        /// 前三对子胆码
        /// </summary>
        LeftRepeat = 18,

        /// <summary>
        /// 后三对子胆码
        /// </summary>
        RightRepeat = 19,

        /// <summary>
        /// 中三对子胆码
        /// </summary>
        MiddleRepeat = 25,

        /// <summary>
        /// 前三胆码
        /// </summary>
        LeftAward = 20,

        /// <summary>
        /// 后三胆码
        /// </summary>
        RightAward = 21,

        /// <summary>
        /// 中三胆码
        /// </summary>
        MiddleAward = 26,

        /// <summary>
        /// 对子与领号胆码组合
        /// </summary>
        AdjacentNumber = 22,

        /// <summary>
        /// 任意两不同数的组合
        /// </summary>
        AllPairs = 23,

        /// <summary>
        /// 任意三不同数的组合
        /// </summary>
        AllTuples = 30,

        LeftTuple = 31,

        Left4Tuple = 35,

        MiddleTuple = 32,

        RightTuple = 33,

        Right4Tuple = 34,

        /// <summary>
        /// 万千十
        /// </summary>
        TupleA = 51,
        /// <summary>
        /// 万千个
        /// </summary>
        TupleB = 52,
        /// <summary>
        /// 万百十
        /// </summary>
        TupleC = 53,
        /// <summary>
        /// 万百个
        /// </summary>
        TupleD = 54,
        /// <summary>
        /// 万十个
        /// </summary>
        TupleE = 55,
        /// <summary>
        /// 千百个
        /// </summary>
        TupleF = 56,
        /// <summary>
        /// 千十个
        /// </summary>
        TupleG = 57,

        /// <summary>
        /// 万千百个
        /// </summary>
        Tuple4A = 61,

        /// <summary>
        /// 万千十个
        /// </summary>
        Tuple4B = 62,

        /// <summary>
        /// 万百十个
        /// </summary>
        Tuple4C = 63,

        LeftDistinct = 27,

        MiddleDistinct = 28,

        RightDistinct = 29,

        Double = 45,

        LeftDouble = 36,

        MiddleDouble = 37,

        RightDouble = 38,

        LeftSpan = 39,

        MiddleSpan = 40,

        RightSpan = 41,

        Right4Award = 64,

        Left4Award = 65,

        Tuple4AAward = 66,

        Tuple4BAward = 67,

        Tuple4CAward = 68,

        AAward,

        BAward,

        CAward,

        DAward,

        EAward,

        FAward,

        GAward
    }
}
