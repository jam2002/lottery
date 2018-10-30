using Lottery.Core.Data;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Lottery.Core.Plan
{
    /// <summary>
    /// 五星三码
    /// </summary>
    public class Dynamic23 : Dynamic
    {
        public override string GetBetString(SimpleBet currentBet)
        {
            List<IEnumerable<int>> list = new List<IEnumerable<int>> { new[] { 0, 1 }, new int[] { 1, 0 }, new int[] { 0, 2 }, new int[] { 2, 0 }, new int[] { 1, 2 }, new int[] { 2, 1 } };
            return $"【{string.Join(" ", list.Select(c => string.Join(string.Empty, c.Select(q => currentBet.BetAward[q]))))}】";
        }

        public override bool ChangeBetOnceSuccess => bool.Parse(ConfigurationManager.AppSettings["ChangeBetOnceSuccess"]);
    }
}
