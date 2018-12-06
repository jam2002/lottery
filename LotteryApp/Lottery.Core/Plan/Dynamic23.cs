using Kw.Combinatorics;
using Lottery.Core.Data;
using System.Collections.Generic;
using System.Linq;

namespace Lottery.Core.Plan
{
    /// <summary>
    /// 五星三码
    /// </summary>
    public class Dynamic23 : Dynamic
    {
        private FactorTypeEnum type;
        private bool isDistinct;

        private bool isAward;
        private int? award;

        private bool isSpan;
        private int[] spans;

        private bool isDouble;
        private int[] awards;
        private int[] excludeAwards;
        private int[] doubleSpans = new int[] { 3, 4, 5, 6, 7, 8 };

        private int[][] betArray;

        public override string GetBetString(SimpleBet currentBet)
        {
            IEnumerable<string> numbers;
            type = currentBet.Results[0].Output[0].Type;
            isDistinct = type == FactorTypeEnum.LeftDistinct || type == FactorTypeEnum.MiddleDistinct || type == FactorTypeEnum.RightDistinct || type == FactorTypeEnum.Distinct;
            isAward = type == FactorTypeEnum.LeftAward || type == FactorTypeEnum.MiddleAward || type == FactorTypeEnum.RightAward || type == FactorTypeEnum.Award;
            isDouble = type == FactorTypeEnum.LeftDouble || type == FactorTypeEnum.MiddleDouble || type == FactorTypeEnum.RightDouble;
            isSpan = type == FactorTypeEnum.LeftSpan || type == FactorTypeEnum.MiddleSpan || type == FactorTypeEnum.RightSpan || type == FactorTypeEnum.Span;
            spans = isSpan && currentBet.BetAward.Any() ? currentBet.BetAward : new int[] { };
            award = isAward && currentBet.BetAward.Any() ? (int?)currentBet.BetAward[0] : null;
            awards = isDouble ? currentBet.BetAward.Take(2).ToArray() : new int[] { };
            excludeAwards = isDouble ? currentBet.BetAward.Skip(2).ToArray() : new int[] { };
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
                numbers = GetComposites();
            }
            return $"【{string.Join(" ", numbers)}】";
        }

        public override bool IsHit(SimpleBet currentBet)
        {
            int[] numbers = currentBet.LastLotteryNumber.Select(t => int.Parse(t.ToString())).ToArray();
            string gameArgs = GameArgs.Split('.')[0];
            switch (gameArgs)
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

        private bool IsValid(int[] input)
        {
            bool ret = false;
            int[] number = input.Distinct().OrderBy(c => c).ToArray();
            int span = number[number.Length - 1] - number[0];
            if (isDistinct)
            {
                ret = number.Length <= 2;
            }
            else if (isAward && award.HasValue)
            {
                ret = number.Contains(award.Value);
            }
            else if (isSpan && spans.Any())
            {
                ret = spans.Contains(span);
            }
            else if (isDouble)
            {
                ret = number.Intersect(awards).Any() && !number.Intersect(excludeAwards).Any() && doubleSpans.Contains(span);
            }
            else
            {
                ret = betArray.Any(t => number.Intersect(t).Count() >= Number);
            }
            return ret;
        }

        private IEnumerable<string> GetComposites()
        {
            IEnumerable<string> ret = new string[] { };
            if (isDistinct)
            {
                ret = Enumerable.Range(0, 10).Select(c => $"{c}{c}");
            }
            else if (isAward && award.HasValue)
            {
                ret = type == FactorTypeEnum.Award ? new string[] { award.Value.ToString() } : Enumerable.Range(0, 10).Select(c => c != award.Value ? $"{c}{award.Value} {award.Value}{c}" : $"{c}{c}").Distinct();
            }
            else if (betArray.Any())
            {
                ret = betArray.Select(t => string.Join(string.Empty, t));
            }
            return ret;
        }
    }
}
