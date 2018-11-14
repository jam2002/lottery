using Kw.Combinatorics;
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
        private bool isDistinct;
        private bool isAward;
        private int? award;

        public override string GetBetString(SimpleBet currentBet)
        {
            IEnumerable<string> numbers;
            FactorTypeEnum type = currentBet.Results[0].Output[0].Type;
            isDistinct = type == FactorTypeEnum.LeftDistinct || type == FactorTypeEnum.MiddleDistinct || type == FactorTypeEnum.RightDistinct;
            isAward = type == FactorTypeEnum.LeftAward || type == FactorTypeEnum.MiddleAward || type == FactorTypeEnum.RightAward;
            award = isAward && currentBet.BetAward.Any() ? (int?)currentBet.BetAward[0] : null;
            int[][] betArray = !isDistinct && !isAward && !award.HasValue ? GetBetArray(currentBet.BetAward) : new int[][] { };

            if (EnableSinglePattern)
            {
                int[] count = Enumerable.Range(0, 10).ToArray();
                if (GameArgs == "front4" || GameArgs == "after4")
                {
                    numbers = from x in count
                              from y in count
                              from z in count
                              from p in count
                              let number = new[] { x, y, z, p }
                              where betArray.Any(t => number.Distinct().Intersect(t).Count() >= Number)
                              select string.Join(string.Empty, number);
                }
                else
                {
                    numbers = from x in count
                              from y in count
                              from z in count
                              let number = new[] { x, y, z }
                              where isDistinct ? (x == y || x == z || y == z) : (award.HasValue? number.Contains(award.Value) : betArray.Any(t => number.Distinct().Intersect(t).Count() >= Number))
                              select string.Join(string.Empty, number);
                }
            }
            else
            {
                numbers = isDistinct ? Enumerable.Range(0, 10).Select(c => $"{c}{c}") : (award.HasValue ? Enumerable.Range(0, 10).Select(c => $"{c}{award.Value} {award.Value}{c}").Distinct() : betArray.Select(t => string.Join(string.Empty, t)));
            }
            return $"【{string.Join(" ", numbers)}】";
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

            bool isHit = BetIndex > 0 && BetIndex <= BetCycle && (isDistinct ? numbers.Distinct().Count() < 3 : (award.HasValue ? numbers.Contains(award.Value) : GetBetArray(LastBet.BetAward).Any(t => t.Intersect(numbers).Count() >= Number)));
            return isHit;
        }

        private int[][] GetBetArray(int[] awards)
        {
            Permutation combine = new Permutation(awards.Length);
            int[][] betAwards = combine.GetRowsForAllPicks().Where(t => t.Picks == 2).Select(t => (from s in t select awards[s]).ToArray()).ToArray();
            return betAwards;
        }

        public override bool ChangeBetOnceSuccess => bool.Parse(ConfigurationManager.AppSettings["ChangeBetOnceSuccess"]);
    }
}
