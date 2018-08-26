using Lottery.Core.Data;
using System.Collections.Generic;

namespace Lottery.Core.Algorithm
{
    public class ValidationResult
    {
        public LotteryResult BetResult { get; set; }

        public double MaxAmount { get; set; }

        public double MinAmount { get; set; }

        public double Amount { get; set; }

        public int HitAllNumber { get; set; }

        public Dictionary<int, int> HitDic { get; set; }

        public string LastLotteryNumber { get; set; }
    }
}
