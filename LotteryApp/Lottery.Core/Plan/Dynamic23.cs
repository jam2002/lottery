using Lottery.Core.Data;
using System.Configuration;
using System.Linq;
using Lottery.Core.Algorithm;

namespace Lottery.Core.Plan
{
    /// <summary>
    /// 五星三码
    /// </summary>
    public class Dynamic23 : Dynamic
    {
        public override string GetBetString(SimpleBet currentBet)
        {
            int[][] betArray = GetBetArray(currentBet.BetAward);

            if (IsSinglePattern && (GameArgs == "front4" || GameArgs == "after4"))
            {
                int[] count = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                var query = from x in count
                            from y in count
                            from z in count
                            from p in count
                            let number = new[] { x, y, z, p }
                            where betArray.Any(t => number.Distinct().Intersect(t).Count() >= Number)
                            select string.Join(string.Empty, number);
                return $"【{string.Join(" ", query)}】";
            }
            else
            {
                return $"【{string.Join(" ", betArray.Select(t => string.Join(string.Empty, t)))}】";
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
            bool isHit = BetIndex > 0 && BetIndex <= BetCycle && GetBetArray(LastBet.BetAward).Any(t => t.Intersect(current).Count() >= Number);
            return isHit;
        }

        public override int[] GetBetAwards(OutputResult output)
        {
            return output.Output.SelectMany(x => x.AnyFilters.SelectMany(t => t.Values)).ToArray();
        }

        private int[][] GetBetArray(int[] awards)
        {
            return new int[][]
            {
                new int[] { awards [0],awards[1]},
                new int[] { awards [1],awards[0]},
                new int[] { awards [2],awards[3]},
                new int[] { awards [3],awards[2]},
                new int[] { awards [4],awards[5]},
                new int[] { awards [5],awards[4]}
            };
        }

        public override bool ChangeBetOnceSuccess => bool.Parse(ConfigurationManager.AppSettings["ChangeBetOnceSuccess"]);

        public bool IsSinglePattern => bool.Parse(ConfigurationManager.AppSettings["IsSinglePattern"]);
    }
}
