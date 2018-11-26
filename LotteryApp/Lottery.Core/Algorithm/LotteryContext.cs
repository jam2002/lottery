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
                BuildFactor(FactorTypeEnum.RightDistinct, n.RightDistinct, i);
                BuildFactor(FactorTypeEnum.MiddleDistinct, n.MiddleDistinct, i);
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
                { FactorTypeEnum.Award, number.DistinctNumbers},
                { FactorTypeEnum.LeftAward, number.LeftAwards},
                { FactorTypeEnum.MiddleAward, number.MiddleAwards},
                { FactorTypeEnum.RightAward, number.RightAwards},
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
                { FactorTypeEnum.Right4Tuple, number.Right4Tuples}
            };

            foreach (var p in typeDic)
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
                case "history":
                    ret = GetHistoryResult();
                    break;
                case "repeats":
                    ret = BuildRepeats();
                    break;
                case "single":
                    ret = GetSingleResult();
                    break;
                case "double":
                    ret = BuildDoubles();
                    break;
                case "tripple":
                    ret = GetTrippleResult();
                    break;
                case "tuple":
                    ret = GetTupleResult();
                    break;
                case "mix":
                    ret = GetMixResult();
                    break;
                case "symmetric":
                    ret = GetSymmetricResult();
                    break;
                case "fivestar":
                    int[] fiveStarForms = new int[] { 3, 4, 5, 6 };
                    Dictionary<int, ReferenceFactor> factors = FactorDic[FactorTypeEnum.FiveStarForm];
                    ret = factors.Where(x => fiveStarForms.Contains(x.Key)).OrderBy(x => x.Key).Select(x => x.Value.ToResult()).ToArray();
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
            FactorTypeEnum r = FactorTypeEnum.AllPairs;
            var query = from p in FactorDic[r]
                        orderby p.Value.OccurCount descending, p.Value.FailureCount, p.Value.LastInterval descending
                        select p.Key;
            return Build(query, r);
        }

        private LotteryResult[] GetTrippleResult()
        {
            LotteryResult[] results = InputOption.RespectRepeat ? BuildRepeats() : new LotteryResult[] { };
            if (!results.Any())
            {
                results = BuildTripples();
            }
            return results;
        }

        private LotteryResult[] GetTupleResult()
        {
            LotteryResult[] results = InputOption.RespectRepeat ? BuildRepeats() : new LotteryResult[] { };
            if (!results.Any())
            {
                results = BuildTuples();
            }
            return results;
        }

        private LotteryResult[] GetSingleResult()
        {
            if (InputOption.RespectRepeat && BuildRepeats().Any())
            {
                return new LotteryResult[] { };
            }
            else
            {
                return BuildSingles();
            }
        }

        private LotteryResult[] GetMixResult()
        {
            IEnumerable<LotteryResult> results = InputOption.RespectRepeat ? BuildRepeats() : new LotteryResult[] { };
            if (!results.Any())
            {
                results = from p in BuildSingles().Concat(BuildTuples()).Concat(BuildTripples())
                          orderby p.MaxInterval, p.HitCount descending, p.FailureCount, p.LastInterval descending, p.Type descending
                          select p;
            }
            return results.Take(3).ToArray();
        }

        private LotteryResult[] GetSymmetricResult()
        {
            FactorTypeEnum r = InputOption.GameArgs == "front" ? FactorTypeEnum.LeftRepeat : (InputOption.GameArgs == "middle" ? FactorTypeEnum.MiddleRepeat : FactorTypeEnum.RightRepeat);
            FactorTypeEnum s = InputOption.GameArgs == "front" ? FactorTypeEnum.LeftAward : (InputOption.GameArgs == "middle" ? FactorTypeEnum.MiddleAward : FactorTypeEnum.RightAward);

            var query = from p in FactorDic[r]
                        join q in FactorDic[s]
                           on p.Key equals q.Key
                        where p.Value.LastInterval <= q.Value.LastInterval
                        orderby p.Value.OccurCount descending, p.Value.FailureCount, p.Value.LastInterval descending
                        select q.Key;
            return Build(query, r);
        }

        private LotteryResult[] Build(IEnumerable<int> awards, FactorTypeEnum type)
        {
            return awards.Take(3).Select(c =>
                {
                    ReferenceFactor factor = FactorDic[type][c];
                    int[] values = null;
                    switch (type)
                    {
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
                        AnyFilters = new AnyFilter[]
                        {
                              new AnyFilter{  Values = values }
                        },
                        Filter = $"不定位：{string.Join(",", values)}"
                    };
                }).ToArray();
        }

        private LotteryResult[] BuildRepeats()
        {
            Dictionary<string, FactorTypeEnum> pairDic = new Dictionary<string, FactorTypeEnum>
            {
                { "front", FactorTypeEnum.LeftDistinct},
                { "middle", FactorTypeEnum.MiddleDistinct},
                { "after", FactorTypeEnum.RightDistinct}
            };
            string gameArgs = InputOption.GameArgs.Split('.')[0];
            FactorTypeEnum? t = pairDic.ContainsKey(gameArgs) ? (FactorTypeEnum?)pairDic[gameArgs] : null;
            ReferenceFactor factor = t.HasValue && FactorDic[t.Value].ContainsKey(2) ? FactorDic[t.Value][2] : null;
            if (factor != null && factor.MaxInterval <= 5 && factor.LastInterval <= 5 && factor.OccurCount >= 4 && factor.LastInterval >= InputOption.WaitInterval)
            {
                return Build(new int[] { 2 }, t.Value);
            }
            return new LotteryResult[] { };
        }

        private LotteryResult[] BuildSingles()
        {
            Dictionary<string, FactorTypeEnum> enumDic = new Dictionary<string, FactorTypeEnum>
            {
                { "front", FactorTypeEnum.LeftAward},
                { "middle", FactorTypeEnum.MiddleAward},
                { "after", FactorTypeEnum.RightAward},
                { "all", FactorTypeEnum.Award}
            };
            string[] gameArgs = InputOption.GameArgs.Split('.').ToArray();
            FactorTypeEnum? r = enumDic.ContainsKey(gameArgs[0]) ? (FactorTypeEnum?)enumDic[gameArgs[0]] : null;
            if (r.HasValue)
            {
                var query = from p in FactorDic[r.Value]
                            where p.Value.LastInterval <= 5
                            orderby p.Value.OccurCount descending, p.Value.MaxInterval, p.Value.FailureCount, p.Value.LastInterval
                            select p.Key;
                if (gameArgs.Length > 1)
                {
                    query = query.Skip(int.Parse(gameArgs[1]));
                }
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
                { "all", FactorTypeEnum.Award}
            };

            FactorTypeEnum? r = enumDic.ContainsKey(InputOption.GameArgs) ? (FactorTypeEnum?)enumDic[InputOption.GameArgs] : null;
            if (r.HasValue)
            {
                var query = from p in FactorDic[r.Value]
                            where p.Value.LastInterval <= 5
                            orderby p.Value.OccurCount descending, p.Value.FailureCount, p.Value.LastInterval
                            select p.Key;
                int[] keys = query.Take(InputOption.TupleLength).OrderBy(c => c).ToArray();
                keys = new int[] { int.Parse("1" + string.Join(string.Empty, keys)) };
                Dictionary<string, FactorTypeEnum> tupleDic = new Dictionary<string, FactorTypeEnum>
                {
                    { "front",   FactorTypeEnum.LeftTuple},
                    { "middle", FactorTypeEnum.MiddleTuple},
                    { "after",  FactorTypeEnum.RightTuple},
                    { "all", FactorTypeEnum.AllTuples}
                };
                return Build(keys, tupleDic[InputOption.GameArgs]);
            }
            return new LotteryResult[] { };
        }

        private LotteryResult[] BuildTuples()
        {
            Dictionary<string, FactorTypeEnum> enumDic = new Dictionary<string, FactorTypeEnum>
            {
                { "front",   InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples: FactorTypeEnum.LeftTuple},
                { "middle", InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples: FactorTypeEnum.MiddleTuple},
                { "after",  InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples: FactorTypeEnum.RightTuple},
                { "front4", InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples: FactorTypeEnum.Left4Tuple},
                { "after4", InputOption.UseGeneralTrend?FactorTypeEnum.AllTuples: FactorTypeEnum.Right4Tuple},
                { "all", FactorTypeEnum.AllTuples}
            };
            FactorTypeEnum r = enumDic[InputOption.GameArgs];

            bool requireRespectRepeats = InputOption.GameArgs == "front" || InputOption.GameArgs == "middle" || InputOption.GameArgs == "after";
            int[] continuous = new int[] { 10123, 11234, 12345, 13456, 14567, 15678, 16789, 10789, 10189, 10129 };

            var query = from p in FactorDic[r]
                        where InputOption.EnableContinuous && requireRespectRepeats ? continuous.Contains(p.Key) : true
                        orderby p.Value.OccurCount descending, p.Value.MaxInterval, p.Value.FailureCount, p.Value.LastInterval descending
                        select p.Key;
            return Build(query, r);
        }

        private LotteryResult[] BuildDoubles()
        {
            Dictionary<string, FactorTypeEnum> enumDic = new Dictionary<string, FactorTypeEnum>
            {
                { "front",   InputOption.UseGeneralTrend?FactorTypeEnum.Award: FactorTypeEnum.LeftAward},
                { "middle", InputOption.UseGeneralTrend?FactorTypeEnum.Award: FactorTypeEnum.MiddleAward},
                { "after",  InputOption.UseGeneralTrend?FactorTypeEnum.Award: FactorTypeEnum.RightAward},
                { "all", FactorTypeEnum.Award}
            };
            FactorTypeEnum? r = enumDic.ContainsKey(InputOption.GameArgs) ? (FactorTypeEnum?)enumDic[InputOption.GameArgs] : null;

            if (r.HasValue)
            {
                var query = from p in FactorDic[r.Value]
                            orderby p.Value.LastInterval < 5 ? 0 : 1, p.Value.OccurCount descending, p.Value.MaxInterval, p.Value.FailureCount, p.Value.LastInterval
                            select p.Key;
                int[] keys = query.ToArray();
                keys = keys.Take(2).Concat(new int[] { keys[keys.Length - 1] }).ToArray();
                Dictionary<string, FactorTypeEnum> awardDic = new Dictionary<string, FactorTypeEnum>
                {
                    { "front",   FactorTypeEnum.LeftDouble},
                    { "middle", FactorTypeEnum.MiddleDouble},
                    { "after",  FactorTypeEnum.RightDouble}
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
                    Filter = $"双胆杀一码：{string.Join(",", keys)}"
                };

                return new LotteryResult[] { ret };
            }
            return new LotteryResult[] { };
        }

        private bool CheckInterval(int[] intervals, int maxInterval = 5)
        {
            return intervals.Skip(intervals.Length - 2).All(c => c < maxInterval);
        }

        private int[] GetIntervals(int[] occurPostions, IEnumerable<LotteryNumber> lotteryNumbers = null)
        {
            lotteryNumbers = lotteryNumbers != null ? lotteryNumbers : LotteryNumbers;
            int[] intervals = occurPostions.Select((x, i) => i == 0 ? x : x - occurPostions[i - 1] - 1).ToArray();
            intervals = intervals.Concat(new[] { lotteryNumbers.Count() - 1 - occurPostions[occurPostions.Length - 1] }).ToArray();
            return intervals;
        }
    }
}
