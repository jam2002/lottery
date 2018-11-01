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
            if (IsSinglePattern && (GameArgs == "front4" || GameArgs == "after4"))
            {
                int[] count = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                var query = from x in count
                            from y in count
                            from z in count
                            from p in count
                            let number = new[] { x, y, z, p }
                            where number.Distinct().Intersect(currentBet.BetAward).Count() >= Number
                            select string.Join(string.Empty, number);
                return $"【{string.Join(" ", query)}】";
            }
            else
            {
                List<IEnumerable<int>> list = new List<IEnumerable<int>> { new[] { 0, 1 }, new int[] { 1, 0 }, new int[] { 0, 2 }, new int[] { 2, 0 }, new int[] { 1, 2 }, new int[] { 2, 1 } };
                return $"【{string.Join(" ", list.Select(c => string.Join(string.Empty, c.Select(q => currentBet.BetAward[q]))))}】";
            }
        }

        public override bool IsHit(SimpleBet currentBet)
        {
            int skip = 0;
            int take = 3;
            switch (GameArgs)
            {
                case "front4":
                    skip = 0;
                    take = 4;
                    break;
                case "after4":
                    skip = 1;
                    take = 4;
                    break;
                default:
                    skip = GameArgs == "front" ? 0 : (GameArgs == "middle" ? 1 : 2);
                    take = 3;
                    break;
            }

            int[] current = currentBet.LastLotteryNumber.Select(t => int.Parse(t.ToString())).Skip(skip).Take(take).ToArray();
            bool isHit = BetIndex > 0 && BetIndex <= BetCycle && LastBet.BetAward.Intersect(current).Count() >= Number;
            return isHit;
        }

        public override bool ChangeBetOnceSuccess => bool.Parse(ConfigurationManager.AppSettings["ChangeBetOnceSuccess"]);

        public bool IsSinglePattern => bool.Parse(ConfigurationManager.AppSettings["IsSinglePattern"]);
    }
}
