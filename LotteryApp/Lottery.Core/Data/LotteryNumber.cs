﻿namespace Lottery.Core.Data
{
    public class LotteryNumber
    {
        public string Key { get; set; }

        public int Max { get; set; }

        public int Min { get; set; }

        public int Sum { get; set; }

        public int Span { get; set; }

        /// <summary>
        /// 0路个数和
        /// </summary>
        public int ZeroCount { get; set; }

        /// <summary>
        /// 1路个数和
        /// </summary>
        public int OneCount { get; set; }

        /// <summary>
        /// 2路个数和
        /// </summary>
        public int TwoCount { get; set; }

        /// <summary>
        /// 去重之后胆码的数量
        /// </summary>
        public int Distinct { get; set; }


        public int[] LeftRawAwards {get;set;}

        public int[] MiddleRawAwards {get;set;}

        public int[] RightRawAwards {get;set;}

        public int[] Left4RawAwards {get;set;}

        public int[] Right4RawAwards {get;set;}

        /// <summary>
        /// 前三对子胆码
        /// </summary>
        public int[] LeftRepeats { get; set; }

        public int[] LeftAwards { get; set; }

        public int[] Left4Awards { get; set; }

        public int[] LeftTuples { get; set; }

        public int[] Left4Tuples { get; set; }

        public int LeftDistinct { get; set; }
        public int LeftSpan { get; set; }

        public int[] ATuples { get; set; }
        public int[] BTuples { get; set; }
        public int[] CTuples { get; set; }
        public int[] DTuples { get; set; }
        public int[] ETuples { get; set; }
        public int[] FTuples { get; set; }
        public int[] GTuples { get; set; }

        public int[] Tuple4As { get; set; }
        public int[] Tuple4Bs { get; set; }
        public int[] Tuple4Cs { get; set; }

        public int[] AAwards { get; set; }

        public int[] BAwards { get; set; }

        public int[] CAwards { get; set; }

        public int[] DAwards { get; set; }

        public int[] EAwards { get; set; }

        public int[] FAwards { get; set; }

        public int[] GAwards { get; set; }

        public int[] LeftPairs { get; set; }

        public int[] RightPairs { get; set; }

        public int[] APairs { get; set; }

        public int[] BPairs { get; set; }

        public int[] CPairs { get; set; }

        public int[] DPairs { get; set; }

        public int[] EPairs { get; set; }

        public int[] FPairs { get; set; }

        public int[] GPairs { get; set; }

        public int[] HPairs { get; set; }

        public int[] Tuple4AAwards { get; set; }

        public int[] Tuple4BAwards { get; set; }

        public int[] Tuple4CAwards { get; set; }

        public int[] MiddleRepeats { get; set; }

        public int[] MiddleAwards { get; set; }

        public int[] MiddleTuples { get; set; }

        public int MiddleDistinct { get; set; }

        public int MiddleSpan { get; set; }

        /// <summary>
        /// 后三对子胆码
        /// </summary>
        public int[] RightRepeats { get; set; }

        public int[] RightAwards { get; set; }

        public int[] Right4Awards { get; set; }

        public int[] RightTuples { get; set; }

        public int[] Right4Tuples { get; set; }

        public int RightDistinct { get; set; }

        public int RightSpan { get; set; }

        public int[] RawNumbers { get; set; }

        /// <summary>
        /// 去重之后按升序排列的胆码
        /// </summary>
        public int[] DistinctNumbers { get; set; }

        public int[] RepeatNumbers { get; set; }

        /// <summary>
        /// 重号与相邻号组合
        /// </summary>
        public int[] AdjacentNumbers { get; set; }

        /// <summary>
        /// 任意两不同数组合
        /// </summary>
        public int[] AllPairs { get; set; }

        /// <summary>
        /// 任意三不同数组合
        /// </summary>
        public int[] AllTuples { get; set; }

        /// <summary>
        /// 按升序排列的胆码，比如 125，15
        /// </summary>
        public string SequenceKey { get; set; }

        /// <summary>
        /// 不定胆，胆码组；比如重庆时时彩，就可以前三与后三；11选5即是所有五码
        /// </summary>
        public int[][] BetKeyPairs { get; set; }

        /// <summary>
        /// 五星形态，1：组120；2：组60；3：组30；4：组20；5：组10；6：组5；
        /// </summary>
        public int FiveStarForm { get; set; }

        public int Wan { get; set; }

        public int Thousand { get; set; }

        public int Hundred { get; set; }

        public int Decade { get; set; }

        public int Unit { get; set; }

        public int OddType {get;set;}

        public OddEnum Odd { get; set; }

        public PrimeEnum Prime { get; set; }

        public SizeEnum Size { get; set; }

        public int[] LeftPairTuples { get; set; }

        public int[] RightPairTuples { get; set; }

        public int[] APairTuples { get; set; }

        public int[] BPairTuples { get; set; }

        public int[] CPairTuples { get; set; }

        public int[] DPairTuples { get; set; }

        public int[] EPairTuples { get; set; }

        public int[] FPairTuples { get; set; }

        public int[] GPairTuples { get; set; }

        public int[] HPairTuples { get; set; }
    }
}
