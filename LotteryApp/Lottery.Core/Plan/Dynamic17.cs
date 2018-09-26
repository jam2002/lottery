using Lottery.Core.Data;
using System.Linq;

namespace Lottery.Core.Plan
{
    public class Dynamic17 : Dynamic
    {
        public override string GetBetString(SimpleBet currentBet)
        {
            int award = currentBet.BetAward[0];
            string[] values = Enumerable.Range(0, 10).SelectMany(c => new string[] { c.ToString() + award.ToString(), award.ToString() + c.ToString() }).Distinct().ToArray();
            return $"【{string.Join(" ", values)}】";
        }

        public override bool IsHit(SimpleBet currentBet)
        {
            int[] current = currentBet.LastLotteryNumber.Select(t => int.Parse(t.ToString())).Skip(1).Take(3).ToArray();
            int[][] betValues = new int[][] { LastBet.BetAward };
            bool isHit = BetIndex > 0 && BetIndex <= BetCycle && betValues.Any(t => t.Intersect(current).Count() >= Number);
            return isHit;
        }
    }
}
