using Lottery.Core.Data;
using System.Linq;

namespace Lottery.Core.Plan
{
    public class Dynamic13 : Dynamic
    {
        public override string GetBetString(SimpleBet currentBet)
        {
            if (currentBet.BetAward.Any())
            {
                int award = currentBet.BetAward[0];
                string[] values = Enumerable.Range(0, 10).SelectMany(c => new string[] { c.ToString() + award.ToString(), award.ToString() + c.ToString() }).Distinct().ToArray();
                return $"【{string.Join(" ", values)}】";
            }
            return null;
        }

        public override bool IsHit(SimpleBet currentBet)
        {
            int number = GameArgs == "front" ? 0 : 2;
            int[] current = currentBet.LastLotteryNumber.Select(t => int.Parse(t.ToString())).Skip(number).Take(3).ToArray();
            int[][] betValues = new int[][] { LastBet.BetAward };
            bool isHit = BetIndex > 0 && BetIndex <= BetCycle && betValues.Any(t => t.Intersect(current).Count() >= Number);
            return isHit;
        }

        public override string GetChangedBetString(SimpleBet currentBet, int status)
        {
            if (status == 2 && BetIndex <= 3 && currentBet.BetAward.Any())
            {
                LastBet = currentBet;
                return GetBetString(currentBet);
            }
            return base.GetChangedBetString(currentBet, status);
        }
    }
}
