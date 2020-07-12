using Kw.Combinatorics;
using Lottery.Core.Algorithm;
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

        private bool isTripple;
        private int[] trippleAwards;

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
            FactorTypeEnum.AAward,
            FactorTypeEnum.BAward,
            FactorTypeEnum.CAward,
            FactorTypeEnum.DAward,
            FactorTypeEnum.EAward,
            FactorTypeEnum.FAward,
            FactorTypeEnum.GAward,
            FactorTypeEnum.Award,
            FactorTypeEnum.LeftRepeat,
            FactorTypeEnum.MiddleRepeat,
            FactorTypeEnum.RightRepeat,
            FactorTypeEnum.RepeatNumber,
            FactorTypeEnum.LeftPair,
            FactorTypeEnum.RightPair,
            FactorTypeEnum.APair,
            FactorTypeEnum.BPair,
            FactorTypeEnum.CPair,
            FactorTypeEnum.DPair,
            FactorTypeEnum.EPair,
            FactorTypeEnum.FPair,
            FactorTypeEnum.GPair,
            FactorTypeEnum.HPair
        };

        public override string GetBetString(SimpleBet currentBet)
        {
            IEnumerable<string> numbers;
            type = currentBet.Results[0].Output.Any() ? (FactorTypeEnum?)currentBet.Results[0].Output[0].Type : null;
            isDistinct = type == FactorTypeEnum.LeftDistinct || type == FactorTypeEnum.MiddleDistinct || type == FactorTypeEnum.RightDistinct || type == FactorTypeEnum.Distinct;
            isAward = awardTypes.Any(t => t == type);
            isDouble = type == FactorTypeEnum.LeftDouble || type == FactorTypeEnum.MiddleDouble || type == FactorTypeEnum.RightDouble || type == FactorTypeEnum.Double;
            isSpan = type == FactorTypeEnum.LeftSpan || type == FactorTypeEnum.MiddleSpan || type == FactorTypeEnum.RightSpan || type == FactorTypeEnum.Span;
            isTripple = GameName == "tripple";
            spans = isSpan && currentBet.BetAward.Any() ? currentBet.BetAward : new int[] { };
            award = isAward && currentBet.BetAward.Any() ? (int?)currentBet.BetAward[0] : null;
            int k = StartSpan % 10;
            awards = isDouble ? currentBet.BetAward.Take(k).ToArray() : new int[] { };
            excludeAwards = isDouble ? currentBet.BetAward.Skip(k).ToArray() : new int[] { };

            trippleAwards = isTripple ? currentBet.BetAward : new int[] { };
            betArray = !isDistinct && !isAward && !isDouble && !award.HasValue && !isTripple ? GetBetArray(currentBet) : new int[][] { };

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
                    numbers = LotteryGenerator.GetConfig().ThreeNumbers.Where(t => IsValid(t.RawNumbers)).Select(t => t.Key);
                }
            }
            else
            {
                numbers = GetComposites();
            }
            return $"【{string.Join(LotteryName.EndsWith("115") ? "," : " ", numbers)}】";
        }

        public override string GetChangedBetString(SimpleBet currentBet, int status)
        {
            return GetBetString(currentBet);
        }

        public override bool IsHit(SimpleBet currentBet)
        {
            int[] numbers = currentBet.LastLotteryNumber.Select(t => int.Parse(t.ToString())).ToArray();
            if (!LotteryName.Contains("|"))
            {
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

                    case "leftpair":
                        numbers = new int[] { numbers[0], numbers[1] };
                        break;
                    case "rightpair":
                        numbers = new int[] { numbers[3], numbers[4] };
                        break;
                    case "paira":
                        numbers = new int[] { numbers[0], numbers[2] };
                        break;
                    case "pairb":
                        numbers = new int[] { numbers[0], numbers[3] };
                        break;
                    case "pairc":
                        numbers = new int[] { numbers[0], numbers[4] };
                        break;
                    case "paird":
                        numbers = new int[] { numbers[1], numbers[2] };
                        break;
                    case "paire":
                        numbers = new int[] { numbers[1], numbers[3] };
                        break;
                    case "pairf":
                        numbers = new int[] { numbers[1], numbers[4] };
                        break;
                    case "pairg":
                        numbers = new int[] { numbers[2], numbers[3] };
                        break;
                    case "pairh":
                        numbers = new int[] { numbers[2], numbers[4] };
                        break;

                    case "wan":
                        numbers = new int[] { numbers[0] };
                        break;
                    case "qian":
                        numbers = new int[] { numbers[1] };
                        break;
                    case "bai":
                        numbers = new int[] { numbers[2] };
                        break;
                    case "shi":
                        numbers = new int[] { numbers[3] };
                        break;
                    case "ge":
                        numbers = new int[] { numbers[4] };
                        break;
                }
            }
            bool isHit = BetIndex > 0 && BetIndex <= BetCycle && IsValid(numbers);
            return isHit;
        }

        private int[][] GetBetArray(SimpleBet bet)
        {
            int[][] bets = bet.Results.SelectMany(t => t.Output.SelectMany(c => c.AnyFilters.Select(s => s.Values))).Take(1).ToArray();

            if (Number == 2 && bets.Any(t => t.Length >= 2))
            {
                bets = bets.SelectMany(c =>
                {
                    Permutation combine = new Permutation(c.Length);
                    return combine.GetRowsForAllPicks().Where(t => t.Picks == Number).Select(t => (from s in t select c[s]).ToArray()).ToArray();
                }).ToArray();
            }
            return bets;
        }

        private bool IsValid(int[] input)
        {
            bool ret = false;
            int[] number = input.Distinct().OrderBy(c => c).ToArray();
            int[] repeats = input.GroupBy(c => c).Where(c => c.Count() > 1).Select(c => c.Key).ToArray();
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
                if (StartSpan > 10)
                {
                    ret = !number.Intersect(awards).Any() && number.Length == 3;
                }
                else
                {
                    if (NumberLength == 3)
                    {

                        bool isQuafilied = input.Select(c => c % 2).Distinct().Count() > 1;
                        ret = number.Intersect(awards).Any();
                    }
                    else if (NumberLength == 4)
                    {
                        bool isQuafilied = number.Select(c => c % 2).Distinct().Count() > 1 && number.Distinct().Count() > 2;
                        ret = number.Intersect(awards).Any() && isQuafilied;
                    }
                    else
                    {
                        ret = number.Intersect(awards).Any() && !number.Intersect(excludeAwards).Any();
                    }
                }
            }
            else if (isTripple)
            {
                ret = (StartSpan == 1 && trippleAwards.Intersect(repeats).Any()) || trippleAwards.Intersect(number).Count() >= Number;
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
                ret = (GameArgs == "all" && NumberLength != 3) || NumberLength == 4 ? new string[] { award.Value.ToString() } : Enumerable.Range(0, 10).Select(c => c != award.Value ? $"{c}{award.Value} {award.Value}{c}" : $"{c}{c}").Distinct();
            }
            else if (isDouble)
            {
                string[] doubleTypes = new string[] { "wan", "qian", "bai", "shi", "ge" };
                if (doubleTypes.Contains(GameArgs) || NumberLength == 5)
                {
                    ret = new string[] { string.Join("", awards) };
                }
                else
                {
                    ret = awards.SelectMany(c => Enumerable.Range(0, 10).Select(t => $"{c}{t}").Concat(Enumerable.Range(0, 10).Select(t => $"{t}{c}"))).Distinct().ToArray();
                }
            }
            else if (isTripple)
            {
                ret = (from x in trippleAwards
                       from y in trippleAwards
                       where StartSpan == 1 ? true : x != y
                       select x.ToString() + y.ToString()).ToArray();
            }
            else if (betArray.Any())
            {
                ret = betArray.Select(t => LotteryName.EndsWith("115") ? string.Join(" ", t.Select(x => x.ToString("D2"))) : string.Join(string.Empty, t)).ToArray();
            }
            return ret;
        }
    }
}
