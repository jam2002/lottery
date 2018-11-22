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
        private bool isDouble;
        private int? award;
        private int[] awards;
        private int? excludeAward;
        private int[][] betArray;

        public override string GetBetString(SimpleBet currentBet)
        {
            IEnumerable<string> numbers;
            FactorTypeEnum type = currentBet.Results[0].Output[0].Type;
            isDistinct = type == FactorTypeEnum.LeftDistinct || type == FactorTypeEnum.MiddleDistinct || type == FactorTypeEnum.RightDistinct;
            isAward = type == FactorTypeEnum.LeftAward || type == FactorTypeEnum.MiddleAward || type == FactorTypeEnum.RightAward;
            isDouble = type == FactorTypeEnum.LeftDouble || type == FactorTypeEnum.MiddleDouble || type == FactorTypeEnum.RightDouble;
            award = isAward && currentBet.BetAward.Any() ? (int?)currentBet.BetAward[0] : null;
            awards = isDouble ? currentBet.BetAward.Take(2).ToArray() : new int[] { };
            excludeAward = isDouble ? (int?)currentBet.BetAward.Last() : null;
            betArray = !isDistinct && !isAward && !isDouble && !award.HasValue ? GetBetArray(currentBet.BetAward) : new int[][] { };

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
                              where IsValid(number)
                              select string.Join(string.Empty, number);
                }
            }
            else
            {
                numbers = isDouble ? new string[] { } : (isDistinct ? Enumerable.Range(0, 10).Select(c => $"{c}{c}") : (award.HasValue ? Enumerable.Range(0, 10).Select(c => c != award.Value ? $"{c}{award.Value} {award.Value}{c}" : $"{c}{c}").Distinct() : betArray.Select(t => string.Join(string.Empty, t))));
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
            bool isHit = BetIndex > 0 && BetIndex <= BetCycle && IsValid(numbers);
            return isHit;
        }

        private int[][] GetBetArray(int[] awards)
        {
            Permutation combine = new Permutation(awards.Length);
            int[][] betAwards = combine.GetRowsForAllPicks().Where(t => t.Picks == 2).Select(t => (from s in t select awards[s]).ToArray()).ToArray();
            return betAwards;
        }

        public override bool ChangeBetOnceSuccess => bool.Parse(ConfigurationManager.AppSettings["ChangeBetOnceSuccess"]);

        private bool IsValid(int[] input)
        {
            bool ret = false;
            int[] number = input.Distinct().OrderBy(c => c).ToArray();
            if (isDistinct)
            {
                ret = number.Length <= 2;
            }
            else if (isAward && award.HasValue)
            {
                ret = number.Contains(award.Value);
            }
            else if (isDouble)
            {
                ret = number.Intersect(awards).Any() && !number.Contains(excludeAward.Value) && number.Length > 1 && (number.Length > 2 ? !(number[2] - number[1] == 1 && number[1] - number[0] == 1) : true);
            }
            else
            {
                ret = betArray.Any(t => number.Intersect(t).Count() >= Number);
            }
            return ret;
        }
    }
}
