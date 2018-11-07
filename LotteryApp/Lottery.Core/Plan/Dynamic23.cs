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
            int[] numbers = currentBet.LastLotteryNumber.Select(t => int.Parse(t.ToString())).ToArray();
            switch (GameArgs)
            {
                case "front4":
                    numbers = numbers.Take(4).ToArray();
                    break;
                case "after4":
                    numbers = numbers.Skip(1).ToArray();
                    break;
                case "front":
                    numbers = numbers.Take(3).ToArray();
                    break;
                case "middle":
                    numbers = numbers.Skip(1).Take(3).ToArray();
                    break;
                case "after":
                    numbers = numbers.Skip(2).ToArray();
                    break;
            }

            bool isHit = BetIndex > 0 && BetIndex <= BetCycle && GetBetArray(LastBet.BetAward).Any(t => t.Intersect(numbers).Count() >= Number);
            return isHit;
        }

        //public override int[] GetBetAwards(OutputResult output)
        //{
        //    return output.Output.SelectMany(x => x.AnyFilters.SelectMany(t => t.Values)).ToArray();
        //}

        private int[][] GetBetArray(int[] awards)
        {
            return new int[][]
            {
                new int[] { awards [0],awards[1]},
                new int[] { awards [1],awards[0]},
                new int[] { awards [0],awards[2]},
                new int[] { awards [2],awards[0]},
                new int[] { awards [1],awards[2]},
                new int[] { awards [2],awards[1]}
            };
        }

        public override bool ChangeBetOnceSuccess => bool.Parse(ConfigurationManager.AppSettings["ChangeBetOnceSuccess"]);

        public bool IsSinglePattern => bool.Parse(ConfigurationManager.AppSettings["IsSinglePattern"]);
    }
}
