using Lottery.Core.Data;

namespace Lottery.Core.Algorithm
{
    public class OutputResult
    {
        public string DisplayName { get; set; }

        public string LastLotteryNumber { get; set; }

        public int Number { get; set; }

        public LotteryResult[] Output { get; set; }
    }
}
