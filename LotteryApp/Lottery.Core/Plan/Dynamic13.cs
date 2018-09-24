using Lottery.Core.Data;
using System.Linq;

namespace Lottery.Core.Plan
{
    public class Dynamic13 : Dynamic
    {
        public override string GetBetString(SimpleBet currentBet)
        {
            int award = currentBet.BetAward[0];
            string[] values = Enumerable.Range(0, 10).SelectMany(c => new string[] { c.ToString() + award.ToString(), award.ToString() + c.ToString() }).Distinct().ToArray();
            return $"【{string.Join(" ", values)}】";
        }
    }
}
