using Lottery.Core.Data;

namespace Lottery.Core.Plan
{
    /// <summary>
    /// 五星一码
    /// </summary>
    public class Dynamic15 : Dynamic
    {
        public override string GetBetString(SimpleBet currentBet)
        {
            return $"【{string.Join(",", currentBet.BetAward)}】";
        }
    }
}
