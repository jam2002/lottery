namespace Lottery.Core.Data
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
        /// 3或者5个数去重后的数量
        /// </summary>
        public int Distinct { get; set; }

        /// <summary>
        /// 去重之后按升序排列的胆码
        /// </summary>
        public int[] DistinctNumbers { get; set; }

        /// <summary>
        /// 按升序排列的胆码，比如 125，15
        /// </summary>
        public int SequenceKey { get; set; }

        /// <summary>
        /// 不定胆，胆码组；比如重庆时时彩，就可以前三与后三；11选5即是所有五码
        /// </summary>
        public int[][] BetKeyPairs { get; set; }

        //public 

        /// <summary>
        /// 五星形态，1：组120；2：组60；3：组30；4：组20；5：组10；6：组5；
        /// </summary>
        public int FiveStarForm { get; set; }

        public int Wan { get; set; }

        public int Thousand { get; set; }

        public int Hundred { get; set; }

        public int Decade { get; set; }

        public int Unit { get; set; }

        public OddEnum Odd { get; set; }

        public PrimeEnum Prime { get; set; }

        public SizeEnum Size { get; set; }
    }
}
