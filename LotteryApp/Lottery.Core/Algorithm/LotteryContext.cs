using Kw.Combinatorics;
using Lottery.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lottery.Core.Algorithm
{
    public class LotteryContext
    {
        public Data.Lottery CurrentLottery { get; private set; }
        public LotteryMetaConfig Config { get; private set; }
        public LotteryNumber[] LotteryNumbers { get; private set; }
        public Dictionary<FactorTypeEnum, Dictionary<int, ReferenceFactor>> FactorDic { get; private set; }
        public int[] DistinctCounts { get; private set; }
        public InputOptions InputOption { get; private set; }

        public static readonly Dictionary<int, int[][]> BetGroups;
        static LotteryContext()
        {
            BetGroups = new Dictionary<int, int[][]> { };

            int[] numbers = Enumerable.Range(0, 10).ToArray();
            int[] betNumbers = Enumerable.Range(2, 3).ToArray();
            foreach (int c in betNumbers)
            {
                Combination combine = new Combination(numbers.Length);
                var tmp = combine.GetRowsForAllPicks().Where(t => t.Picks == c);
                int[][] posKeys = tmp.Select(t => (from s in t select numbers[s]).ToArray()).ToArray();
                BetGroups.Add(c, posKeys);
            }
        }

        public LotteryContext(LotteryMetaConfig config, LotteryNumber[] numbers, InputOptions input)
        {
            Config = config;
            LotteryNumbers = numbers;
            FactorDic = Enum.GetNames(typeof(FactorTypeEnum)).ToDictionary(t => (FactorTypeEnum)Enum.Parse(typeof(FactorTypeEnum), t, true), t => new Dictionary<int, ReferenceFactor> { });
            CurrentLottery = config.Lotteries.Where(x => x.Key == input.LotteryName).First();
            DistinctCounts = CurrentLottery.HasPair ? null : new int[] { 3 };
            InputOption = input;

            Init();
        }

        private void Init()
        {
            LotteryNumber n;
            for (int i = 0; i < LotteryNumbers.Length; i++)
            {
                n = LotteryNumbers[i];
                BuildFactor(FactorTypeEnum.Span, n.Span, i);
                BuildFactor(FactorTypeEnum.Odd, (int)n.Odd, i);
                BuildFactor(FactorTypeEnum.Size, (int)n.Size, i);
                BuildFactor(FactorTypeEnum.Prime, (int)n.Prime, i);
                BuildFactor(FactorTypeEnum.Sum, n.Sum, i);
                BuildFactor(FactorTypeEnum.Max, n.Max, i);
                BuildFactor(FactorTypeEnum.Min, n.Min, i);
                BuildFactor(FactorTypeEnum.Hundred, n.Hundred, i);
                BuildFactor(FactorTypeEnum.Decade, n.Decade, i);
                BuildFactor(FactorTypeEnum.Unit, n.Unit, i);
                BuildFactor(FactorTypeEnum.Distinct, n.Distinct, i);
                BuildFactor(FactorTypeEnum.SequenceKey, n.SequenceKey, i);
                BuildFactor(FactorTypeEnum.LeftDistinct, n.LeftDistinct, i);
                BuildFactor(FactorTypeEnum.LeftSpan, n.LeftSpan, i);
                BuildFactor(FactorTypeEnum.RightDistinct, n.RightDistinct, i);
                BuildFactor(FactorTypeEnum.RightSpan, n.RightSpan, i);
                BuildFactor(FactorTypeEnum.MiddleDistinct, n.MiddleDistinct, i);
                BuildFactor(FactorTypeEnum.MiddleSpan, n.MiddleSpan, i);
                BuildAwardFactor(n, i);

                if (CurrentLottery.Length == 5)
                {
                    BuildFactor(FactorTypeEnum.FiveStarForm, n.FiveStarForm, i);
                    BuildFactor(FactorTypeEnum.Thousand, n.Thousand, i);
                    BuildFactor(FactorTypeEnum.Wan, n.Wan, i);
                }
            }
            BuildInterval();
        }

        private void BuildFactor(FactorTypeEnum type, int key, int pos)
        {
            ReferenceFactor factor;
            if (!FactorDic[type].ContainsKey(key))
            {
                factor = new ReferenceFactor { Key = key, Type = type, OccurCount = 1, OccurPositions = new int[] { pos } };
                FactorDic[type].Add(key, factor);
            }
            else
            {
                factor = FactorDic[type][key];
                factor.OccurCount++;
                factor.OccurPositions = factor.OccurPositions.Concat(new[] { pos }).ToArray();
            }
        }

        private void BuildAwardFactor(LotteryNumber number, int pos)
        {
            Dictionary<FactorTypeEnum, int[]> typeDic = new Dictionary<FactorTypeEnum, int[]>
            {
                { FactorTypeEnum.Award, InputOption.StartSpan == 0? number.RawNumbers : number.DistinctNumbers},
                { FactorTypeEnum.LeftAward, number.LeftAwards},
                { FactorTypeEnum.Left4Award, number.Left4Awards},
                { FactorTypeEnum.MiddleAward, number.MiddleAwards},
                { FactorTypeEnum.RightAward, number.RightAwards},
                { FactorTypeEnum.Right4Award, number.Right4Awards},
                { FactorTypeEnum.Tuple4AAward, number.Tuple4AAwards},
                { FactorTypeEnum.Tuple4BAward, number.Tuple4BAwards},
                { FactorTypeEnum.Tuple4CAward, number.Tuple4CAwards},
                { FactorTypeEnum.RepeatNumber, number.RepeatNumbers},
                { FactorTypeEnum.LeftRepeat, number.LeftRepeats},
                { FactorTypeEnum.MiddleRepeat, number.MiddleRepeats},
                { FactorTypeEnum.RightRepeat, number.RightRepeats},
                { FactorTypeEnum.AdjacentNumber, number.AdjacentNumbers},
                { FactorTypeEnum.AllPairs, number.AllPairs},
                { FactorTypeEnum.LeftTuple, number.LeftTuples},
                { FactorTypeEnum.MiddleTuple, number.MiddleTuples},
                { FactorTypeEnum.RightTuple, number.RightTuples},
                { FactorTypeEnum.AllTuples, number.AllTuples},
                { FactorTypeEnum.Left4Tuple, number.Left4Tuples},
                { FactorTypeEnum.Right4Tuple, number.Right4Tuples},

                { FactorTypeEnum.TupleA, number.ATuples},
                { FactorTypeEnum.TupleB, number.BTuples},
                { FactorTypeEnum.TupleC, number.CTuples},
                { FactorTypeEnum.TupleD, number.DTuples},
                { FactorTypeEnum.TupleE, number.ETuples},
                { FactorTypeEnum.TupleF, number.FTuples},
                { FactorTypeEnum.TupleG, number.GTuples},

                { FactorTypeEnum.Tuple4A, number.Tuple4As},
                { FactorTypeEnum.Tuple4B, number.Tuple4Bs},
                { FactorTypeEnum.Tuple4C, number.Tuple4Cs},

                { FactorTypeEnum.AAward, number.AAwards},
                { FactorTypeEnum.BAward, number.BAwards},
                { FactorTypeEnum.CAward, number.CAwards},
                { FactorTypeEnum.DAward, number.DAwards},
                { FactorTypeEnum.EAward, number.EAwards},
                { FactorTypeEnum.FAward, number.FAwards},
                { FactorTypeEnum.GAward, number.GAwards},

                { FactorTypeEnum.LeftPair, number.LeftPairs},
                { FactorTypeEnum.RightPair, number.RightPairs},
                { FactorTypeEnum.APair, number.APairs},
                { FactorTypeEnum.BPair, number.BPairs},
                { FactorTypeEnum.CPair, number.CPairs},
                { FactorTypeEnum.DPair, number.DPairs},
                { FactorTypeEnum.EPair, number.EPairs},
                { FactorTypeEnum.FPair, number.FPairs},
                { FactorTypeEnum.GPair, number.GPairs},
                { FactorTypeEnum.HPair, number.HPairs},

                { FactorTypeEnum.LeftPTuple, number.LeftPairTuples},
                { FactorTypeEnum.RightPTuple, number.RightPairTuples},
                { FactorTypeEnum.APairTuple, number.APairTuples},
                { FactorTypeEnum.BPairTuple, number.BPairTuples},
                { FactorTypeEnum.CPairTuple, number.CPairTuples},
                { FactorTypeEnum.DPairTuple, number.DPairTuples},
                { FactorTypeEnum.EPairTuple, number.EPairTuples},
                { FactorTypeEnum.FPairTuple, number.FPairTuples},
                { FactorTypeEnum.GPairTuple, number.GPairTuples},
                { FactorTypeEnum.HPairTuple, number.HPairTuples}
            };

            foreach (var p in typeDic.Where(t => t.Value != null))
            {
                foreach (int key in p.Value)
                {
                    BuildFactor(p.Key, key, pos);
                }
            }
        }

        private void BuildInterval()
        {
            IEnumerable<ReferenceFactor> factors = FactorDic.Values.SelectMany(x => x.Values).ToArray();
            foreach (ReferenceFactor factor in factors)
            {
                int[] intervals = GetIntervals(factor.OccurPositions);

                factor.LastInterval = intervals.Last();
                if (intervals.Length >= 2)
                {
                    factor.SubInterval = intervals[intervals.Length - 2];
                }
                factor.MaxInterval = intervals.Max();
                factor.HitIntervals = intervals;
                factor.FailureCount = intervals.Select(t => (int)Math.Floor((decimal)t / 5)).Sum();
            }
        }

        public LotteryResult[] GetGameResult()
        {
            LotteryResult[] ret = null;
            switch (InputOption.GameName)
            {
                case "adjacent":
                    ret = GetAdjacentResult();
                    break;
                case "twopairs":
                case "history":
                    ret = GetHistoryResult();
                    break;
                case "anytuple":
                    ret = GetAnyTupleResult();
                    break;
                case "repeats":
                case "single":
                case "span":
                case "double":
                case "tripple":
                case "tuple":
                case "mix":
                    ret = GetDynamicResult();
                    break;
                case "symmetric":
                    ret = GetSymmetricResult();
                    break;
                case "fivestar":
                    int[] fiveStarForms = new int[] { 3, 4, 5, 6 };
                    Dictionary<int, ReferenceFactor> factors = FactorDic[FactorTypeEnum.FiveStarForm];
                    ret = factors.Where(x => fiveStarForms.Contains(x.Key)).OrderBy(x => x.Key).Select(x => x.Value.ToResult()).ToArray();
                    break;
                case "solidrepeat":
                    ret = Build(new int[] { 2 }, FactorTypeEnum.LeftDistinct);
                    break;    
                default:
                    break;
            }
            return ret;
        }

        private LotteryResult[] GetAdjacentResult()
        {
            var query = from p in FactorDic[FactorTypeEnum.AdjacentNumber]
                        join q in FactorDic[FactorTypeEnum.AllPairs]
                           on p.Key equals q.Key
                        where p.Value.LastInterval <= q.Value.LastInterval && q.Value.LastInterval <= 7
                        orderby p.Value.OccurCount descending, p.Value.FailureCount, p.Value.LastInterval descending
                        select p.Key;
            return Build(query, FactorTypeEnum.AllPairs);
        }

        private LotteryResult[] GetHistoryResult()
        {
            int awardOccurCount = InputOption.TakeNumber == 20 ? 9 : 7;
            int pairOccurCount = InputOption.TakeNumber == 20 ? 6 : 5;

            int[] validAwards = FactorDic[FactorTypeEnum.Award].Where(c => CheckInterval(c.Value.HitIntervals) && c.Value.OccurCount >= awardOccurCount)
                                                                                            .OrderByDescending(c => c.Value.OccurCount)
                                                                                            .ThenBy(c => c.Value.MaxInterval)
                                                                                            .ThenBy(c => c.Value.LastInterval)
                                                                                            .Select(c => c.Key)
                                                                                            .ToArray();

            FactorTypeEnum r = FactorTypeEnum.AllPairs;
            var query = from p in FactorDic[r]
                        let values = p.Key.ToString().Select(c => int.Parse(c.ToString())).Skip(1).ToArray()
                        where validAwards.Intersect(values).Count() == values.Length && p.Value.OccurCount >= pairOccurCount && p.Value.LastInterval < 7
                        orderby p.Value.OccurCount descending, p.Value.MaxInterval, p.Value.LastInterval descending
                        select p.Key;
            return Build(query, r);
        }

        private LotteryResult[] GetAnyTupleResult()
        {
            FactorTypeEnum r = FactorTypeEnum.AllTuples;
            var query = from p in FactorDic[r]
                        orderby p.Value.OccurCount descending, p.Value.MaxInterval, p.Value.FailureCount, p.Value.LastInterval
                        select p.Key;
            int key = query.ToArray()[0];

            Dictionary<string, FactorTypeEnum> tupleDic = new Dictionary<string, FactorTypeEnum>
            {
                { "tuplea", FactorTypeEnum.TupleA},
                { "tupleb", FactorTypeEnum.TupleB},
                { "tuplec", FactorTypeEnum.TupleC},
                { "tupled", FactorTypeEnum.TupleD},
                { "tuplee", FactorTypeEnum.TupleE},
                { "tuplef", FactorTypeEnum.TupleF},
                { "tupleg", FactorTypeEnum.TupleG},
                { "front",   FactorTypeEnum.LeftTuple},
                { "middle", FactorTypeEnum.MiddleTuple},
                { "after",  FactorTypeEnum.RightTuple}
            };
            var q = from c in tupleDic
                    let p = FactorDic[c.Value].ContainsKey(key) ? FactorDic[c.Value][key] : null
                    where p != null
                    orderby p.OccurCount descending, p.MaxInterval, p.FailureCount, p.LastInterval
                    select Build(new int[] { key }, c.Value);

            return q.Take(2).SelectMany(c => c).Where(c => tupleDic[InputOption.GameArgs] == c.Type).ToArray();
        }

        private LotteryResult[] GetDynamicResult()
        {
            bool isRepeatTrend = false;
            bool respectRepeat = InputOption.RespectRepeat || InputOption.GameName == "repeats";
            LotteryResult[] results = respectRepeat ? BuildRepeats(out isRepeatTrend) : new LotteryResult[] { };

            if (isRepeatTrend)
            {
                return respectRepeat && !InputOption.DisableRepeat ? results : new LotteryResult[] { };
            }
            else
            {
                switch (InputOption.GameName)
                {
                    case "single":
                        results = BuildSingles();
                        break;
                    case "span":
                        results = BuildSpans();
                        break;
                    case "double":
                        results = BuildDoubles();
                        break;
                    case "tripple":
                        results = BuildTripples();
                        break;
                    case "tuple":
                        results = BuildTuples();
                        break;
                    case "mix":
                        results = (from p in BuildSingles().Concat(BuildSpans()).Concat(BuildTuples()).Concat(BuildTripples())
                                   orderby p.MaxInterval, p.HitCount descending, p.FailureCount, p.LastInterval descending, p.Type descending
                                   select p).Take(3).ToArray();
                        break;
                }
            }
            return results;
        }

        private LotteryResult[] GetSymmetricResult()
        {
            FactorTypeEnum r = InputOption.GameArgs == "front" ? FactorTypeEnum.LeftRepeat : (InputOption.GameArgs == "middle" ? FactorTypeEnum.MiddleRepeat : (InputOption.GameArgs == "after" ? FactorTypeEnum.RightRepeat : FactorTypeEnum.RepeatNumber));
            FactorTypeEnum s = InputOption.GameArgs == "front" ? FactorTypeEnum.LeftAward : (InputOption.GameArgs == "middle" ? FactorTypeEnum.MiddleAward : (InputOption.GameArgs == "after" ? FactorTypeEnum.RightAward : FactorTypeEnum.Award));

            var query = from p in FactorDic[r]
                        join q in FactorDic[s]
                           on p.Key equals q.Key
                        where CheckInterval(q.Value.HitIntervals) && p.Value.LastInterval <= q.Value.LastInterval
                        orderby q.Value.LastInterval descending, q.Value.OccurCount descending, q.Value.MaxInterval
                        select q.Key;
            return Build(query, r);
        }

        private LotteryResult[] Build(IEnumerable<int> awards, FactorTypeEnum type)
        {
            if (InputOption.Rank > 0)
            {
                awards = awards.Skip(InputOption.Rank);
            }
            if (InputOption.WaitInterval > 0)
            {
                awards = awards.ToArray().Take(1).Where(c => FactorDic[type][c].LastInterval >= InputOption.WaitInterval);
            }
            bool isSpan = false;
            return awards.Select((c, i) =>
            {
                ReferenceFactor factor = FactorDic[type].ContainsKey(c) ? FactorDic[type][c] : null;
                if (factor == null)
                {
                    return null;
                }
                int[] values = null;
                switch (type)
                {
                    case FactorTypeEnum.LeftPTuple:
                    case FactorTypeEnum.RightPTuple:
                    case FactorTypeEnum.APairTuple:
                    case FactorTypeEnum.BPairTuple:
                    case FactorTypeEnum.CPairTuple:
                    case FactorTypeEnum.DPairTuple:
                    case FactorTypeEnum.EPairTuple:
                    case FactorTypeEnum.FPairTuple:
                    case FactorTypeEnum.GPairTuple:
                    case FactorTypeEnum.HPairTuple:
                    case FactorTypeEnum.TupleA:
                    case FactorTypeEnum.TupleB:
                    case FactorTypeEnum.TupleC:
                    case FactorTypeEnum.TupleD:
                    case FactorTypeEnum.TupleE:
                    case FactorTypeEnum.TupleF:
                    case FactorTypeEnum.TupleG:
                    case FactorTypeEnum.Tuple4A:
                    case FactorTypeEnum.Tuple4B:
                    case FactorTypeEnum.Tuple4C:
                    case FactorTypeEnum.LeftTuple:
                    case FactorTypeEnum.Left4Tuple:
                    case FactorTypeEnum.RightTuple:
                    case FactorTypeEnum.Right4Tuple:
                    case FactorTypeEnum.MiddleTuple:
                    case FactorTypeEnum.AllTuples:
                    case FactorTypeEnum.AllPairs:
                    case FactorTypeEnum.AdjacentNumber:
                        values = c.ToString().Select(t => int.Parse(t.ToString())).Skip(1).ToArray();
                        break;
                    case FactorTypeEnum.LeftSpan:
                    case FactorTypeEnum.MiddleSpan:
                    case FactorTypeEnum.RightSpan:
                    case FactorTypeEnum.Span:
                        isSpan = true;
                        values = awards.Skip(i).Take(2).OrderBy(t => t).ToArray();
                        break;
                    default:
                        values = new int[] { c };
                        break;
                }
                return new LotteryResult
                {
                    GameName = InputOption.GameName,
                    HitIntervals = factor.HitIntervals,
                    HitCount = factor.OccurCount,
                    FailureCount = factor.FailureCount,
                    LotteryName = InputOption.LotteryName,
                    LastInterval = factor.LastInterval,
                    MaxInterval = factor.MaxInterval,
                    Type = type,
                    BetKey = c,
                    NumberLength = InputOption.NumberLength,
                    AnyFilters = new AnyFilter[]
                    {
                            new AnyFilter{  Values = values }
                    },
                    Filter = $"{(isSpan ? "跨度" : "不定位")} ：{string.Join(",", values)}"
                };
            }).Where(c => c != null).Take(3).ToArray();
        }

        private LotteryResult[] BuildRepeats(out bool isRepeat)
        {
            isRepeat = false;
            Dictionary<string, FactorTypeEnum> pairDic = new Dictionary<string, FactorTypeEnum>
            {
                { "front", FactorTypeEnum.LeftDistinct},
                { "middle", FactorTypeEnum.MiddleDistinct},
                { "after", FactorTypeEnum.RightDistinct}
            };
            string gameArgs = InputOption.GameArgs.Split('.')[0];
            FactorTypeEnum? t = pairDic.ContainsKey(gameArgs) ? (FactorTypeEnum?)pairDic[gameArgs] : null;
            ReferenceFactor factor = t.HasValue && FactorDic[t.Value].ContainsKey(2) ? FactorDic[t.Value][2] : null;
            if (factor != null)
            {
                int[] occurPositions = factor.OccurPositions;
                int[] intervals = factor.HitIntervals;

                int considerCount = 15;
                if (InputOption.TakeNumber > considerCount)
                {
                    int stopPoint = InputOption.TakeNumber - considerCount;
                    occurPositions = factor.OccurPositions.SkipWhile(c => c == stopPoint || c + 1 <= stopPoint).Select(c => Math.Abs(c - stopPoint)).ToArray();
                    intervals = GetIntervals(occurPositions, considerCount);
                }
                isRepeat = intervals.Any() && occurPositions.Any() && intervals.Last() <= 5 && occurPositions.Count() >= 4;
                return isRepeat && factor.LastInterval >= InputOption.WaitInterval ? Build(new int[] { 2 }, t.Value) : new LotteryResult[] { };
            }
            return new LotteryResult[] { };
        }

        private LotteryResult[] BuildSingles()
        {
            Dictionary<string, Tuple<FactorTypeEnum, FactorTypeEnum>> enumDic = new Dictionary<string, Tuple<FactorTypeEnum, FactorTypeEnum>>
            {
                { "front",    new Tuple<FactorTypeEnum, FactorTypeEnum>(InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.LeftAward, FactorTypeEnum.LeftAward)},
                { "middle",  new Tuple<FactorTypeEnum, FactorTypeEnum>(InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.MiddleAward, FactorTypeEnum.MiddleAward)},
                { "after",   new Tuple<FactorTypeEnum, FactorTypeEnum>(InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.RightAward, FactorTypeEnum.RightAward)},
                { "tuplea",  new Tuple<FactorTypeEnum, FactorTypeEnum>(InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.AAward, FactorTypeEnum.AAward)},
                { "tupleb",  new Tuple<FactorTypeEnum, FactorTypeEnum>(InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.BAward, FactorTypeEnum.BAward)},
                { "tuplec",  new Tuple<FactorTypeEnum, FactorTypeEnum>( InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.CAward, FactorTypeEnum.CAward)},
                { "tupled",  new Tuple<FactorTypeEnum, FactorTypeEnum>( InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.DAward, FactorTypeEnum.DAward)},
                { "tuplee",  new Tuple<FactorTypeEnum, FactorTypeEnum>( InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.EAward, FactorTypeEnum.EAward)},
                { "tuplef",  new Tuple<FactorTypeEnum, FactorTypeEnum>( InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.FAward, FactorTypeEnum.FAward)},
                { "tupleg",  new Tuple<FactorTypeEnum, FactorTypeEnum>(  InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.GAward, FactorTypeEnum.GAward)},
                { "after4", new Tuple<FactorTypeEnum, FactorTypeEnum>(  InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.Right4Award, FactorTypeEnum.Right4Award)},
                { "front4", new Tuple<FactorTypeEnum, FactorTypeEnum>(  InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.Left4Award, FactorTypeEnum.Left4Award)},
                { "tuple4a",  new Tuple<FactorTypeEnum, FactorTypeEnum>(  InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.Tuple4AAward, FactorTypeEnum.Tuple4AAward)},
                { "tuple4b",  new Tuple<FactorTypeEnum, FactorTypeEnum>( InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.Tuple4BAward, FactorTypeEnum.Tuple4BAward)},
                { "tuple4c",  new Tuple<FactorTypeEnum, FactorTypeEnum>( InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.Tuple4CAward, FactorTypeEnum.Tuple4CAward)},

                { "leftpair",    new Tuple<FactorTypeEnum, FactorTypeEnum>(InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.LeftPair, FactorTypeEnum.LeftPair)},
                { "rightpair",  new Tuple<FactorTypeEnum, FactorTypeEnum>(InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.RightPair, FactorTypeEnum.RightPair)},
                { "paira",   new Tuple<FactorTypeEnum, FactorTypeEnum>(InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.APair, FactorTypeEnum.APair)},
                { "pairb",  new Tuple<FactorTypeEnum, FactorTypeEnum>(InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.BPair, FactorTypeEnum.BPair)},
                { "pairc",  new Tuple<FactorTypeEnum, FactorTypeEnum>(InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.CPair, FactorTypeEnum.CPair)},
                { "paird",  new Tuple<FactorTypeEnum, FactorTypeEnum>( InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.DPair, FactorTypeEnum.DPair)},
                { "paire",  new Tuple<FactorTypeEnum, FactorTypeEnum>( InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.EPair, FactorTypeEnum.EPair)},
                { "pairf",  new Tuple<FactorTypeEnum, FactorTypeEnum>( InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.FPair, FactorTypeEnum.FPair)},
                { "pairg",  new Tuple<FactorTypeEnum, FactorTypeEnum>( InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.GPair, FactorTypeEnum.GPair)},
                { "pairh",  new Tuple<FactorTypeEnum, FactorTypeEnum>(  InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.HPair, FactorTypeEnum.HPair)},

                { "all",new Tuple<FactorTypeEnum, FactorTypeEnum>(  FactorTypeEnum.Award,FactorTypeEnum.Award)}
            };

            string[] gameArgs = InputOption.GameArgs.Split('.').ToArray();
            string key = gameArgs.Length > 1 ? gameArgs[1] : gameArgs[0];
            Tuple<FactorTypeEnum, FactorTypeEnum> r = enumDic.ContainsKey(key) ? enumDic[key] : null;
            if (r != null)
            {
                var query = from p in FactorDic[r.Item1]
                            orderby CheckInterval(p.Value.HitIntervals) ? 0 : 1, p.Value.OccurCount descending, p.Value.LastInterval descending, p.Value.FailureCount
                            select p.Key;

                query = query.Take(2).Where(c => FactorDic[r.Item1][c].LastInterval >= InputOption.StartSpan).ToArray();

                return Build(query, r.Item2);
            }
            return new LotteryResult[] { };
        }

        private LotteryResult[] BuildSpans()
        {
            Dictionary<string, FactorTypeEnum> enumDic = new Dictionary<string, FactorTypeEnum>
            {
                { "front", FactorTypeEnum.LeftSpan},
                { "middle", FactorTypeEnum.MiddleSpan},
                { "after", FactorTypeEnum.RightSpan},
                { "all", FactorTypeEnum.Span}
            };
            string[] gameArgs = InputOption.GameArgs.Split('.').ToArray();
            FactorTypeEnum? r = enumDic.ContainsKey(gameArgs[0]) ? (FactorTypeEnum?)enumDic[gameArgs[0]] : null;
            if (r.HasValue)
            {
                var query = from p in FactorDic[r.Value]
                            where p.Value.LastInterval <= 7
                            orderby p.Value.OccurCount descending, p.Value.MaxInterval, p.Value.FailureCount, p.Value.LastInterval
                            select p.Key;

                return Build(query, r.Value);
            }
            return new LotteryResult[] { };
        }

        private LotteryResult[] BuildTripples()
        {
            Dictionary<string, FactorTypeEnum> enumDic = new Dictionary<string, FactorTypeEnum>
            {
                { "front",   InputOption.UseGeneralTrend?FactorTypeEnum.Award: FactorTypeEnum.LeftAward},
                { "middle", InputOption.UseGeneralTrend?FactorTypeEnum.Award: FactorTypeEnum.MiddleAward},
                { "after",  InputOption.UseGeneralTrend?FactorTypeEnum.Award: FactorTypeEnum.RightAward},
                { "tuplea", InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.AAward},
                { "tupleb", InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.BAward},
                { "tuplec", InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.CAward},
                { "tupled", InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.DAward},
                { "tuplee", InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.EAward},
                { "tuplef", InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.FAward},
                { "tupleg", InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.GAward},

                { "leftpair", InputOption.UseGeneralTrend?FactorTypeEnum.Award: FactorTypeEnum.LeftPair},
                { "rightpair", InputOption.UseGeneralTrend? FactorTypeEnum.Award: FactorTypeEnum.RightPair},
                { "paira",  InputOption.UseGeneralTrend?FactorTypeEnum.Award:  FactorTypeEnum.APair},
                { "pairb", InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.BPair},
                { "pairc", InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.CPair},
                { "paird", InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.DPair},
                { "paire", InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.EPair},
                { "pairf", InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.FPair},
                { "pairg", InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.GPair},
                { "pairh", InputOption.UseGeneralTrend?FactorTypeEnum.Award:FactorTypeEnum.HPair},

                { "front4", InputOption.UseGeneralTrend?FactorTypeEnum.Award: FactorTypeEnum.Left4Award},
                { "after4", InputOption.UseGeneralTrend?FactorTypeEnum.Award: FactorTypeEnum.Right4Award},
                { "tuple4a", InputOption.UseGeneralTrend?FactorTypeEnum.Award: FactorTypeEnum.Tuple4AAward},
                { "tuple4b", InputOption.UseGeneralTrend?FactorTypeEnum.Award: FactorTypeEnum.Tuple4AAward},
                { "tuple4c", InputOption.UseGeneralTrend?FactorTypeEnum.Award: FactorTypeEnum.Tuple4CAward},
                { "all", FactorTypeEnum.Award}
            };

            string gameArgs = InputOption.GameArgs.Split('.').ToArray()[0];
            FactorTypeEnum? r = enumDic.ContainsKey(gameArgs) ? (FactorTypeEnum?)enumDic[gameArgs] : null;
            if (r.HasValue)
            {
                var query = from p in FactorDic[r.Value]
                            orderby CheckInterval(p.Value.HitIntervals) ? 0 : 1, p.Value.OccurCount descending, p.Value.LastInterval, p.Value.MaxInterval
                            select p.Key;
                int[] keys = query.Take(InputOption.TupleLength).OrderBy(c => c).ToArray();
                keys = new int[] { int.Parse("1" + string.Join(string.Empty, keys)) };
                Dictionary<string, FactorTypeEnum> tupleDic = new Dictionary<string, FactorTypeEnum>
                {
                    { "tuple4a", FactorTypeEnum.Tuple4A},
                    { "tuple4b", FactorTypeEnum.Tuple4B},
                    { "tuple4c", FactorTypeEnum.Tuple4C},
                    { "front4", FactorTypeEnum.Left4Tuple},
                    { "after4", FactorTypeEnum.Right4Tuple},
                    { "front",   FactorTypeEnum.LeftTuple},
                    { "middle", FactorTypeEnum.MiddleTuple},
                    { "after",  FactorTypeEnum.RightTuple},
                    { "all", FactorTypeEnum.AllTuples},
                    { "tuplea", FactorTypeEnum.TupleA},
                    { "tupleb", FactorTypeEnum.TupleB},
                    { "tuplec", FactorTypeEnum.TupleC},
                    { "tupled", FactorTypeEnum.TupleD},
                    { "tuplee", FactorTypeEnum.TupleE},
                    { "tuplef", FactorTypeEnum.TupleF},
                    { "tupleg", FactorTypeEnum.TupleG},

                    { "leftpair",  FactorTypeEnum.LeftPTuple},
                    { "rightpair", FactorTypeEnum.RightPTuple},
                    { "paira",  FactorTypeEnum.APairTuple},
                    { "pairb", FactorTypeEnum.BPairTuple},
                    { "pairc", FactorTypeEnum.CPairTuple},
                    { "paird", FactorTypeEnum.DPairTuple},
                    { "paire", FactorTypeEnum.EPairTuple},
                    { "pairf", FactorTypeEnum.FPairTuple},
                    { "pairg", FactorTypeEnum.GPairTuple},
                    { "pairh", FactorTypeEnum.HPairTuple}
                };
                return Build(keys, tupleDic[gameArgs]);
            }
            return new LotteryResult[] { };
        }

        private LotteryResult[] BuildTuples()
        {
            Dictionary<string, FactorTypeEnum> enumDic = new Dictionary<string, FactorTypeEnum>
            {
                { "front",   InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples: FactorTypeEnum.LeftTuple},
                { "middle", InputOption.UseGeneralTrend? FactorTypeEnum.AllTuples: FactorTypeEnum.MiddleTuple},
                { "after",  InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples:  FactorTypeEnum.RightTuple},
                { "tuplea", InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples:FactorTypeEnum.TupleA},
                { "tupleb", InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples:FactorTypeEnum.TupleB},
                { "tuplec", InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples:FactorTypeEnum.TupleC},
                { "tupled", InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples:FactorTypeEnum.TupleD},
                { "tuplee", InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples:FactorTypeEnum.TupleE},
                { "tuplef", InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples:FactorTypeEnum.TupleF},
                { "tupleg", InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples:FactorTypeEnum.TupleG},
                { "front4", InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples: FactorTypeEnum.Left4Tuple},
                { "after4", InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples: FactorTypeEnum.Right4Tuple},
                { "tuple4a", InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples: FactorTypeEnum.Tuple4A},
                { "tuple4b", InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples: FactorTypeEnum.Tuple4B},
                { "tuple4c", InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples: FactorTypeEnum.Tuple4C},

                { "leftpair",  InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples: FactorTypeEnum.LeftPTuple},
                { "rightpair", InputOption.UseGeneralTrend? FactorTypeEnum.AllTuples: FactorTypeEnum.RightPTuple},
                { "paira",  InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples:  FactorTypeEnum.APairTuple},
                { "pairb", InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples:FactorTypeEnum.BPairTuple},
                { "pairc", InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples:FactorTypeEnum.CPairTuple},
                { "paird", InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples:FactorTypeEnum.DPairTuple},
                { "paire", InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples:FactorTypeEnum.EPairTuple},
                { "pairf", InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples:FactorTypeEnum.FPairTuple},
                { "pairg", InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples:FactorTypeEnum.GPairTuple},
                { "pairh", InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples:FactorTypeEnum.HPairTuple},

                { "all", FactorTypeEnum.AllTuples}
            };
            Dictionary<string, FactorTypeEnum> tupleDic = new Dictionary<string, FactorTypeEnum>
            {
                { "tuple4a", FactorTypeEnum.Tuple4A},
                { "tuple4b", FactorTypeEnum.Tuple4B},
                { "tuple4c", FactorTypeEnum.Tuple4C},
                { "front4", FactorTypeEnum.Left4Tuple},
                { "after4", FactorTypeEnum.Right4Tuple},
                { "front",   FactorTypeEnum.LeftTuple},
                { "middle", FactorTypeEnum.MiddleTuple},
                { "after",  FactorTypeEnum.RightTuple},
                { "all", FactorTypeEnum.AllTuples},
                { "tuplea", FactorTypeEnum.TupleA},
                { "tupleb", FactorTypeEnum.TupleB},
                { "tuplec", FactorTypeEnum.TupleC},
                { "tupled", FactorTypeEnum.TupleD},
                { "tuplee", FactorTypeEnum.TupleE},
                { "tuplef", FactorTypeEnum.TupleF},
                { "tupleg", FactorTypeEnum.TupleG},

                { "leftpair",  FactorTypeEnum.LeftPTuple},
                { "rightpair", FactorTypeEnum.RightPTuple},
                { "paira",  FactorTypeEnum.APairTuple},
                { "pairb", FactorTypeEnum.BPairTuple},
                { "pairc", FactorTypeEnum.CPairTuple},
                { "paird", FactorTypeEnum.DPairTuple},
                { "paire", FactorTypeEnum.EPairTuple},
                { "pairf", FactorTypeEnum.FPairTuple},
                { "pairg", FactorTypeEnum.GPairTuple},
                { "pairh", FactorTypeEnum.HPairTuple}
            };

            string gameArgs = InputOption.GameArgs.Split('.').ToArray()[0];
            FactorTypeEnum r = enumDic[gameArgs];

            int[] continuous = InputOption.TupleLength == 4 ? new int[] { 10123, 11234, 12345, 13456, 14567, 15678, 16789, 10789, 10189, 10129 } : new int[] { 1012, 1123, 1234, 1345, 1456, 1567, 1678, 1789, 1089, 1019 };
            int[] validAwards = FactorDic[FactorTypeEnum.Award].Where(c => CheckInterval(c.Value.HitIntervals)).Select(c => c.Key).ToArray();

            IEnumerable<int> query = from p in FactorDic[r]
                                     let values = p.Key.ToString().Select(c => int.Parse(c.ToString())).Skip(1).ToArray()
                                     where InputOption.EnableContinuous ? continuous.Contains(p.Key) : true
                                     orderby p.Value.OccurCount descending, p.Value.MaxInterval, p.Value.LastInterval descending
                                     select p.Key;

            if (InputOption.UseGeneralTrend && InputOption.GeneralTrendInterval > 0)
            {
                query = query.ToArray().Take(1).Where(c => FactorDic[r][c].LastInterval >= InputOption.GeneralTrendInterval);
            }
            return Build(query, tupleDic[gameArgs]);
        }

        private LotteryResult[] BuildDoubles()
        {
            Dictionary<string, FactorTypeEnum> enumDic = new Dictionary<string, FactorTypeEnum>
            {
                { "front2",   InputOption.UseGeneralTrend?FactorTypeEnum.Award: FactorTypeEnum.LeftAward},
                { "middle2", InputOption.UseGeneralTrend?FactorTypeEnum.Award: FactorTypeEnum.RightAward},
                { "after2",  InputOption.UseGeneralTrend?FactorTypeEnum.Award: FactorTypeEnum.RightAward},
                { "front",   InputOption.UseGeneralTrend?FactorTypeEnum.Award: FactorTypeEnum.LeftAward},
                { "middle", InputOption.UseGeneralTrend?FactorTypeEnum.Award: FactorTypeEnum.MiddleAward},
                { "after",  InputOption.UseGeneralTrend?FactorTypeEnum.Award: FactorTypeEnum.RightAward},
                { "all", FactorTypeEnum.Award}
            };
            FactorTypeEnum? r = enumDic.ContainsKey(InputOption.GameArgs) ? (FactorTypeEnum?)enumDic[InputOption.GameArgs] : null;

            if (r.HasValue)
            {
                var query = from p in FactorDic[r.Value]
                            orderby CheckInterval(p.Value.HitIntervals, 6) ? 0 : 1, p.Value.OccurCount descending, p.Value.MaxInterval, p.Value.FailureCount, p.Value.LastInterval
                            select p.Key;
                int[] keys = query.ToArray();
                int k= InputOption.StartSpan % 10;
                keys = InputOption.StartSpan > 10 ? keys.Skip(keys.Length - k).OrderBy(c => c).ToArray() : keys.Take(k).OrderBy(c => c).ToArray();
                Dictionary<string, FactorTypeEnum> awardDic = new Dictionary<string, FactorTypeEnum>
                {
                    { "front2",   FactorTypeEnum.LeftDouble},
                    { "middle2", FactorTypeEnum.RightDouble},
                    { "after2",  FactorTypeEnum.RightDouble},
                    { "front",   FactorTypeEnum.LeftDouble},
                    { "middle", FactorTypeEnum.MiddleDouble},
                    { "after",  FactorTypeEnum.RightDouble},
                    { "all",  FactorTypeEnum.Double}
                };

                LotteryResult ret = new LotteryResult
                {
                    GameName = InputOption.GameName,
                    HitIntervals = new int[] { },
                    LotteryName = InputOption.LotteryName,
                    Type = awardDic[InputOption.GameArgs],
                    AnyFilters = new AnyFilter[]
                    {
                         new AnyFilter{  Values = keys }
                    },
                    Filter = $"双胆：{string.Join(",", keys)}"
                };

                return new LotteryResult[] { ret };
            }
            return new LotteryResult[] { };
        }

        private bool CheckInterval(int[] intervals, int maxInterval = 5)
        {
            return intervals.Skip(intervals.Length - 2).All(c => c < maxInterval);
        }

        private int[] GetIntervals(int[] occurPostions, int? number = null)
        {
            number = number ?? InputOption.TakeNumber;
            int[] intervals = occurPostions.Select((x, i) => i == 0 || i - 1 >= occurPostions.Length ? x : x - occurPostions[i - 1] - 1).ToArray();
            intervals = intervals.Concat(new[] { number.Value - 1 - occurPostions[occurPostions.Length - 1] }).ToArray();
            return intervals;
        }
    }
}
