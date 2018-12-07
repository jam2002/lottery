using Lottery.Core.Algorithm;
using Lottery.Core.Data;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;

namespace Lottery.Core.Plan
{
    /// <summary>
    /// 五星二码
    /// </summary>
    public class Dynamic22 : Dynamic
    {
        public override string GetBetString(SimpleBet currentBet)
        {
            List<IEnumerable<int>> list = new List<IEnumerable<int>> { new[] { 0, 1 }, new int[] { 1, 0 } };
            return $"【{string.Join(" ", list.Select(c => string.Join(string.Empty, c.Select(q => currentBet.BetAward[q]))))}】";
        }

        public override int[] GetBetAwards(OutputResult output)
        {
            if (GameName == "dynamic")
            {
                int[] awards = output.Output.Where(d => d.WinCount >= 8).FirstOrDefault()?.AnyFilters.SelectMany(t => t.Values).Distinct().ToArray();
                return awards ?? new int[] { };
            }
            return base.GetBetAwards(output);
        }
    }
}
