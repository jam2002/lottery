using Lottery.Core.Algorithm;

namespace Lottery.Core.Data
{
    public class SimpleBet
    {
        public string LastLotteryNumber { get; set; }

        public int[] BetAward { get; set; }

        public OutputResult[] Results { get; set; }
    }
}
