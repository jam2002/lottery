﻿using Kw.Combinatorics;
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
        private FactorTypeEnum? type;

        private bool isAward;
        private int? award;

        private bool isSpan;
        private int[] spans;

        private bool isDouble;
        private int[] awards;
        private int[] excludeAwards;
        private int[] doubleSpans;

        private int[][] betArray;
        private FactorTypeEnum[] awardTypes = new FactorTypeEnum[]
        {
            FactorTypeEnum.LeftAward,
            FactorTypeEnum.MiddleAward,
            FactorTypeEnum.RightAward,
            FactorTypeEnum.Right4Award,
            FactorTypeEnum.Left4Award,
            FactorTypeEnum.Tuple4AAward,
            FactorTypeEnum.Tuple4BAward,
            FactorTypeEnum.Tuple4CAward,
            FactorTypeEnum.Award
        };

        public override string GetBetString(SimpleBet currentBet)
        {
            IEnumerable<string> numbers;
            type = currentBet.Results[0].Output.Any() ? (FactorTypeEnum?)currentBet.Results[0].Output[0].Type : null;
            isDistinct = type == FactorTypeEnum.LeftDistinct || type == FactorTypeEnum.MiddleDistinct || type == FactorTypeEnum.RightDistinct || type == FactorTypeEnum.Distinct;
            isAward = awardTypes.Any(t => t == type);
            isDouble = type == FactorTypeEnum.LeftDouble || type == FactorTypeEnum.MiddleDouble || type == FactorTypeEnum.RightDouble || type == FactorTypeEnum.Double;
            isSpan = type == FactorTypeEnum.LeftSpan || type == FactorTypeEnum.MiddleSpan || type == FactorTypeEnum.RightSpan || type == FactorTypeEnum.Span;
            spans = isSpan && currentBet.BetAward.Any() ? currentBet.BetAward : new int[] { };
            award = isAward && currentBet.BetAward.Any() ? (int?)currentBet.BetAward[0] : null;
            awards = isDouble ? currentBet.BetAward.Take(2).ToArray() : new int[] { };
            excludeAwards = isDouble ? currentBet.BetAward.Skip(2).ToArray() : new int[] { };
            doubleSpans = Enumerable.Range(StartSpan, SpanLength).ToArray();
            betArray = !isDistinct && !isAward && !isDouble && !award.HasValue ? GetBetArray(currentBet.BetAward) : new int[][] { };

            if (awards.All(c => c == 0 || c == 1 || c == 8 || c == 9))
            {
                doubleSpans = new int[] { 4, 5, 6, 7, 8, 9 };
            }

            if (EnableSinglePattern)
            {
                int[] count = Enumerable.Range(0, 10).ToArray();
                if (NumberLength == 5)
                {
                    numbers = from x in count
                              from y in count
                              from z in count
                              from p in count
                              from q in count
                              let number = new[] { x, y, z, p, q }
                              where IsValid(number)
                              select string.Join(string.Empty, number);
                }
                else if (NumberLength == 4)
                {
                    numbers = from x in count
                              from y in count
                              from z in count
                              from p in count
                              let number = new[] { x, y, z, p }
                              where IsValid(number)
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

        public override string GetChangedBetString(SimpleBet currentBet, int status)
        {
            return GetBetString(currentBet);
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
                case "after2":
                    numbers = numbers.Skip(3).ToArray();
                    break;
                case "middle2":
                    numbers = numbers.Skip(2).Take(2).ToArray();
                    break;
                case "front2":
                    numbers = numbers.Take(2).ToArray();
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
                case "tuplea":
                    numbers = new int[] { numbers[0], numbers[1], numbers[3] };
                    break;
                case "tupleb":
                    numbers = new int[] { numbers[0], numbers[1], numbers[4] };
                    break;
                case "tuplec":
                    numbers = new int[] { numbers[0], numbers[2], numbers[3] };
                    break;
                case "tupled":
                    numbers = new int[] { numbers[0], numbers[2], numbers[4] };
                    break;
                case "tuplee":
                    numbers = new int[] { numbers[0], numbers[3], numbers[4] };
                    break;
                case "tuplef":
                    numbers = new int[] { numbers[1], numbers[2], numbers[4] };
                    break;
                case "tupleg":
                    numbers = new int[] { numbers[1], numbers[3], numbers[4] };
                    break;
                case "tuple4a":
                    numbers = new int[] { numbers[0], numbers[1], numbers[2], numbers[4] };
                    break;
                case "tuple4b":
                    numbers = new int[] { numbers[0], numbers[1], numbers[3], numbers[4] };
                    break;
                case "tuple4c":
                    numbers = new int[] { numbers[0], numbers[2], numbers[3], numbers[4] };
                    break;
            }
            bool isHit = BetIndex > 0 && BetIndex <= BetCycle && IsValid(numbers);
            return isHit;
        }

        private int[][] GetBetArray(int[] awards)
        {
            Permutation combine = new Permutation(awards.Length);
            int number = TupleLength == 4 && Number == 3 ? 3 : 2;
            int[][] betAwards = combine.GetRowsForAllPicks().Where(t => t.Picks == number).Select(t => (from s in t select awards[s]).ToArray()).ToArray();
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
                int zeroCount = number.Select(c => c % 3).Distinct().Count();
                int sumRemain = input.Sum() % 10;
                ret = number.Intersect(awards).Any() && !number.Intersect(excludeAwards).Any() && doubleSpans.Contains(span) && zeroCount > 1 && sumRemain > 0;
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
                ret = type == FactorTypeEnum.Award && GameArgs == "all" ? new string[] { award.Value.ToString() } : Enumerable.Range(0, 10).Select(c => c != award.Value ? $"{c}{award.Value} {award.Value}{c}" : $"{c}{c}").Distinct();
            }
            else if (isDouble)
            {
                ret = awards.SelectMany(c => Enumerable.Range(0, 10).Select(t => $"{c}{t}").Concat(Enumerable.Range(0, 10).Select(t => $"{t}{c}"))).Distinct().ToArray();
            }
            else if (betArray.Any())
            {
                ret = betArray.Select(t => string.Join(string.Empty, t));
            }
            return ret;
        }
    }
}
