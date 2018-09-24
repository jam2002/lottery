using Lottery.Core.Data;
using System.Collections.Generic;
using System.Linq;

namespace Lottery.Core.Plan
{
    public class Dynamic22 : Dynamic
    {
        public override string GetBetString(SimpleBet currentBet)
        {
            List<IEnumerable<int>> list = new List<IEnumerable<int>> { new[] { 0, 1 }, new int[] { 1, 0 } };
            return $"【{string.Join(" ", list.Select(c => string.Join(string.Empty, c.Select(q => currentBet.BetAward[q]))))}】";
        }
    }
}
