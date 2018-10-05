using System.Linq;
using Lottery.Core.Algorithm;
using Lottery.Core.Data;

namespace Lottery.Core.Plan
{
    /// <summary>
    /// 三星二码
    /// </summary>
    public class Dynamic135 : Dynamic
    {
        public override int[] GetBetAwards(OutputResult output)
        {
            return output.Output.Take(2).SelectMany(c => c.AnyFilters.SelectMany(t => t.Values)).ToArray();
        }

        public override string GetBetString(SimpleBet currentBet)
        {
            string[] keys = LotteryGenerator.GetConfig().Numbers.Where(c => c.DistinctNumbers.Intersect(currentBet.BetAward).Count() >= Number).Select(c => c.Key).ToArray();
            return $"【{string.Join(" ", keys)}】";
        }

        public override bool IsHit(SimpleBet currentBet)
        {
            int number = GameArgs == "front" ? 0 : 2;
            int[] current = currentBet.LastLotteryNumber.Select(t => int.Parse(t.ToString())).Skip(number).Take(3).ToArray();
            bool isHit = BetIndex > 0 && BetIndex <= BetCycle && LastBet.BetAward.Intersect(current).Count() >= Number;
            return isHit;
        }
    }
}
