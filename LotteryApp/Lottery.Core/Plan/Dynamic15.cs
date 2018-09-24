using Lottery.Core.Data;

namespace Lottery.Core.Plan
{
    public class Dynamic15 : Dynamic
    {
        public override string GetBetString(SimpleBet currentBet)
        {
            return $"【{string.Join(",", currentBet.BetAward)}】";
        }
    }
}
