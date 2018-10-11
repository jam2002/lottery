using Lottery.Core.Data;
using System.Linq;

namespace Lottery.Core.Plan
{
    /// <summary>
    /// 三星一码
    /// </summary>
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
            int number = GameArgs == "front" ? 0 : (GameArgs == "middle" ? 1 : 2);
            int[] current = currentBet.LastLotteryNumber.Select(t => int.Parse(t.ToString())).Skip(number).Take(3).ToArray();
            int[][] betValues = new int[][] { LastBet.BetAward };
            bool isHit = BetIndex > 0 && BetIndex <= BetCycle && betValues.Any(t => t.Intersect(current).Count() >= Number);
            return isHit;
        }
    }
}
