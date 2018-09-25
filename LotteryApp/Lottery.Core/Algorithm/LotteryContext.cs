using Kw.Combinatorics;
using Lottery.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearRegression;
using MathNet.Numerics;

namespace Lottery.Core.Algorithm
{
    public class LotteryContext
    {
        public Data.Lottery CurrentLottery { get; private set; }
        public LotteryMetaConfig Config { get; private set; }
        public LotteryNumber[] LotteryNumbers { get; private set; }
        public Dictionary<FactorTypeEnum, Dictionary<int, ReferenceFactor>> FactorDic { get; private set; }
        public Dictionary<FactorTypeEnum, int> MaxSkipDic { get; private set; }
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
            FactorDic = new Dictionary<FactorTypeEnum, Dictionary<int, ReferenceFactor>>
            {
                { FactorTypeEnum.Span, new Dictionary<int, ReferenceFactor> { } },
                { FactorTypeEnum.Odd, new Dictionary<int, ReferenceFactor> { } },
                { FactorTypeEnum.Size, new Dictionary<int, ReferenceFactor> { } },
                { FactorTypeEnum.Prime, new Dictionary<int, ReferenceFactor> { } },
                { FactorTypeEnum.Sum, new Dictionary<int, ReferenceFactor> { } },
                { FactorTypeEnum.Max, new Dictionary<int, ReferenceFactor> { } },
                { FactorTypeEnum.Min, new Dictionary<int, ReferenceFactor> { } },
                { FactorTypeEnum.Wan, new Dictionary<int, ReferenceFactor> { } },
                { FactorTypeEnum.Thousand, new Dictionary<int, ReferenceFactor> { } },
                { FactorTypeEnum.Hundred, new Dictionary<int, ReferenceFactor> { } },
                { FactorTypeEnum.Decade, new Dictionary<int, ReferenceFactor> { } },
                { FactorTypeEnum.Unit, new Dictionary<int, ReferenceFactor> { } },
                { FactorTypeEnum.Distinct, new Dictionary<int, ReferenceFactor> { } },
                { FactorTypeEnum.Award, new Dictionary<int, ReferenceFactor> { } },
                { FactorTypeEnum.SequenceKey, new Dictionary<int, ReferenceFactor> { } },
                { FactorTypeEnum.DynamicPosition, new Dictionary<int, ReferenceFactor> { } },
                { FactorTypeEnum.FiveStarForm, new Dictionary<int, ReferenceFactor> { } },
                { FactorTypeEnum.RepeatNumber, new Dictionary<int, ReferenceFactor> { } },
                { FactorTypeEnum.AdjacentNumber, new Dictionary<int, ReferenceFactor> { } },
                { FactorTypeEnum.AllPairs, new Dictionary<int, ReferenceFactor> { } }
            };
            MaxSkipDic = new Dictionary<FactorTypeEnum, int>
            {
                { FactorTypeEnum.Odd, 1},
                { FactorTypeEnum.Size,1},
                { FactorTypeEnum.Prime,1},
                { FactorTypeEnum.Max,6},
                { FactorTypeEnum.Min,6},
                { FactorTypeEnum.Hundred, 1},
                { FactorTypeEnum.Decade,1},
                { FactorTypeEnum.Unit,1},
                { FactorTypeEnum.Award,6},
            };
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
            foreach (int key in number.DistinctNumbers)
            {
                BuildFactor(FactorTypeEnum.Award, key, pos);
            }

            foreach (int key in number.RepeatNumbers)
            {
                BuildFactor(FactorTypeEnum.RepeatNumber, key, pos);
            }

            foreach (int key in number.AdjacentNumbers)
            {
                BuildFactor(FactorTypeEnum.AdjacentNumber, key, pos);
            }

            foreach (int key in number.AllPairs)
            {
                BuildFactor(FactorTypeEnum.AllPairs, key, pos);
            }
        }

        private void BuildInterval()
        {
            IEnumerable<ReferenceFactor> factors = FactorDic.Values.SelectMany(x => x.Values).ToArray();
            foreach (ReferenceFactor factor in factors)
            {
                int[] intervals = GetIntervals(factor.OccurPositions);

                factor.LastInterval = LotteryNumbers.Length - factor.OccurPositions[factor.OccurCount - 1] - 1;
                factor.MaxInterval = intervals.Max();
                factor.HitIntervals = intervals;
                factor.OrderKey = (LotteryNumbers.Length - factor.LastInterval).ToString("D2") + factor.OccurCount.ToString("D2");
            }
        }

        public LotteryResult[] GetGameResult()
        {
            LotteryResult[] ret = null;
            switch (InputOption.GameName)
            {
                case "dynamic":
                    ret = GetDynamicPosResult();
                    break;
                case "adjacent":
                    ret = GetAdjacentResult();
                    break;
                case "anytwo":
                    ret = new[] { GetAnyTwoResultByHeat() };
                    break;
                case "fivestar":
                    int[] fiveStarForms = new int[] { 3, 4, 5, 6 };
                    Dictionary<int, ReferenceFactor> factors = FactorDic[FactorTypeEnum.FiveStarForm];
                    ret = factors.Where(x => fiveStarForms.Contains(x.Key)).OrderBy(x => x.Key).Select(x => x.Value.ToResult()).ToArray();
                    break;
                case "mix":
                    ret = new[] { GetMixResult() };
                    break;
                case "groupThree":
                    ReferenceFactor factor = FactorDic[FactorTypeEnum.Distinct].ContainsKey(2) ? FactorDic[FactorTypeEnum.Distinct][2] : null;
                    if (factor != null && factor.MaxInterval <= 6)
                    {
                        LotteryResult r = factor.ToResult();
                        r.Filter = "组三";
                        return new[] { r };
                    }
                    break;
                default:
                    break;
            }

            if (ret != null)
            {
                foreach (LotteryResult r in ret)
                {
                    r.LotteryName = InputOption.LotteryName;

                }
            }

            return ret;
        }

        private LotteryResult GetMixResult()
        {
            int[] spanTakeNumbers = new int[] { 3, 2, 4, 1, 5, 6 };

            int[] orderSpans = FactorDic[FactorTypeEnum.Span].Where(x => x.Value.LastInterval < 10).OrderByDescending(x => x.Value.OrderKey).Select(x => x.Key).ToArray();
            int[] orderSums = FactorDic[FactorTypeEnum.Sum].Where(x => x.Value.OccurCount > 1 || (x.Key >= 8 && x.Key <= 19)).OrderBy(x => x.Value.OrderKey).Select(x => x.Key).ToArray();
            int[] hotSums = Enumerable.Range(10, 10).ToArray();
            orderSums = orderSums.Concat(hotSums).Distinct().ToArray();

            int[] orderHundreds = FactorDic[FactorTypeEnum.Hundred].OrderBy(x => x.Value.OrderKey).Select(x => x.Key).ToArray();
            int[] orderDecades = FactorDic[FactorTypeEnum.Decade].OrderBy(x => x.Value.OrderKey).Select(x => x.Key).ToArray();
            int[] orderUnits = FactorDic[FactorTypeEnum.Unit].OrderBy(x => x.Value.OrderKey).Select(x => x.Key).ToArray();

            int[] orderMax = FactorDic[FactorTypeEnum.Max].Where(x => x.Value.LastInterval <= 15 && x.Key >= 4).OrderBy(x => x.Value.OrderKey).Select(x => x.Key).ToArray();
            orderMax = orderMax.Skip(orderMax.Length - 6 > 0 ? orderMax.Length - 6 : 0).ToArray();

            OddEnum[] orderOdds = FactorDic[FactorTypeEnum.Odd].Where(x => x.Value.LastInterval <= 15).OrderBy(x => x.Value.OrderKey).Select(x => (OddEnum)x.Key).ToArray();
            SizeEnum[] orderSizes = FactorDic[FactorTypeEnum.Size].Where(x => x.Value.LastInterval <= 15).OrderBy(x => x.Value.OrderKey).Select(x => (SizeEnum)x.Key).ToArray();
            PrimeEnum[] orderPrimes = FactorDic[FactorTypeEnum.Prime].Where(x => x.Value.LastInterval <= 15).OrderBy(x => x.Value.OrderKey).Select(x => (PrimeEnum)x.Key).ToArray();

            int[][] allSkips = (from x in GetSkipArray(FactorTypeEnum.Odd, 8 - orderOdds.Length)
                                from y in GetSkipArray(FactorTypeEnum.Size, 8 - orderSizes.Length)
                                from z in GetSkipArray(FactorTypeEnum.Prime, 8 - orderPrimes.Length)
                                from p in GetSkipArray(FactorTypeEnum.Hundred, 10 - orderHundreds.Length)
                                from q in GetSkipArray(FactorTypeEnum.Decade, 10 - orderDecades.Length)
                                from r in GetSkipArray(FactorTypeEnum.Unit, 10 - orderUnits.Length)
                                from t in GetSkipArray(FactorTypeEnum.Max, 6 - orderMax.Length)
                                select new[] { x, y, z, p, q, r, t }).ToArray();

            List<LotteryResult> list = new List<LotteryResult> { };
            for (int i = 0; i < spanTakeNumbers.Length; i++)
            {
                int[] spans = orderSpans.Take(spanTakeNumbers[i]).ToArray();
                bool excludeThree = !spans.Any(t => t % 3 == 0);

                IEnumerable<LotteryResult> results = allSkips.Select(x => GetFilteredResult(spans,
                                                                                                                                                     orderOdds.Skip(x[0]).ToArray(),
                                                                                                                                                     orderSizes.Skip(x[1]).ToArray(),
                                                                                                                                                     orderPrimes.Skip(x[2]).ToArray(),
                                                                                                                                                     orderSums,
                                                                                                                                                     orderHundreds.Skip(x[3]).ToArray(),
                                                                                                                                                     orderDecades.Skip(x[4]).ToArray(),
                                                                                                                                                     orderUnits.Skip(x[5]).ToArray(),
                                                                                                                                                     orderMax.Skip(x[6]).ToArray(),
                                                                                                                                                     null,
                                                                                                                                                     DistinctCounts,
                                                                                                                                                     excludeThree)).ToArray();
                list.AddRange(results);
            }

            return InferResult(list);
        }

        private LotteryResult[] GetDynamicPosResult()
        {
            string[] arguments = (string.IsNullOrEmpty(InputOption.GameArgs) || InputOption.GameArgs.StartsWith("-") ? "22" : InputOption.GameArgs).Split(',');
            int[] number = arguments[0].Select(x => int.Parse(x.ToString())).ToArray();
            int keyCount = number[0];//码数，任选二，任选三，任选四
            int betCount = number.Length > 1 ? number[1] : keyCount;//投注数，任选二选二码，任选三选三码，四码，任选四选四码，五码

            int[] numbers = arguments.Length > 1 ? arguments[1].Select(x => int.Parse(x.ToString())).OrderBy(x => x).ToArray() : (CurrentLottery.Length <= 5 ? new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 } : new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 });
            IEnumerable<LotteryResult> list = null;

            if (keyCount > 1)
            {
                Combination combine = new Combination(betCount);
                Func<int[], int[][]> getBetPosKeys = p => combine.GetRowsForAllPicks().Where(t => t.Picks == keyCount).Select(t => (from s in t select p[s]).ToArray()).ToArray();

                list = BetGroups[betCount].Select(x => GetFilteredResult(null, null, null, null, null, null, null, null, null, null, null, null, null, getBetPosKeys(x))).ToArray();
            }
            else
            {
                list = numbers.Select(x => GetFilteredResult(null, null, null, null, null, null, null, null, null, null, null, null, null, new int[][] { new[] { x } })).ToArray();
            }
            LotteryResult[] ret = InferResults(list);

            return ret;
        }

        private LotteryResult[] GetAdjacentResult()
        {
            var query = from p in FactorDic[FactorTypeEnum.AdjacentNumber]
                        join q in FactorDic[FactorTypeEnum.AllPairs]
                           on new { p.Key, p.Value.LastInterval } equals new { q.Key, q.Value.LastInterval }
                        where p.Value.LastInterval <= 7
                        orderby q.Value.LastInterval, q.Value.OccurCount descending
                        select p.Key;
            return query.Take(3).Select(c =>
            {
                ReferenceFactor factor = FactorDic[FactorTypeEnum.AllPairs][c];
                return new LotteryResult
                {
                    GameName = "adjacent",
                    HitIntervals = factor.HitIntervals,
                    HitCount = factor.OccurCount,
                    LotteryName = InputOption.LotteryName,
                    LastInterval = factor.LastInterval,
                    MaxInterval = factor.MaxInterval,
                    AnyFilters = new AnyFilter[]
                    {
                        new AnyFilter{  Values =new int[] { (c - 100) / 10, (c - 100) % 10 } }
                    }
                };
            }).ToArray();
        }

        private LotteryResult GetAnyTwoResultByHit()
        {
            int[] poses = new int[] { 0, 1, 2, 3, 4 };
            Combination combine = new Combination(poses.Length);
            int[][] posKeys = combine.GetRowsForAllPicks().Where(t => t.Picks == 2).Select(t => (from s in t select poses[s]).ToArray()).ToArray(); //获取健位置索引组合，比如万百，万千，万十

            int takeContinueNumber = string.IsNullOrWhiteSpace(InputOption.GameArgs) ? 5 : int.Parse(InputOption.GameArgs.Split(',').Last()); //获取连续区域长度

            FactorTypeEnum[] posFactors = new FactorTypeEnum[] { FactorTypeEnum.Wan, FactorTypeEnum.Thousand, FactorTypeEnum.Hundred, FactorTypeEnum.Decade, FactorTypeEnum.Unit };
            Dictionary<FactorTypeEnum, int[][]> betValueDic = new Dictionary<FactorTypeEnum, int[][]> { };
            foreach (FactorTypeEnum posFactor in posFactors)
            {
                Dictionary<int, ReferenceFactor> posReference = FactorDic[posFactor];
                int[] values = posReference.Values.Where(x => x.LastInterval < 15)
                                                                           .Select(x => x.Key)
                                                                           .OrderBy(x => x)
                                                                           .ToArray();                               //获取值组合，此处杀了最近15期未出的号码

                if (values.Length < 5)
                {
                    values = posReference.Values.OrderBy(x => x.LastInterval).Select(x => x.Key).Except(values).Take(5 - values.Length).Concat(values).OrderBy(x => x).ToArray();
                }

                int[][] valuePosKeys = Enumerable.Range(0, values.Length).Select(x => x + takeContinueNumber - 1 < values.Length ? Enumerable.Range(x, takeContinueNumber).ToArray() : Enumerable.Range(x, values.Length - x).Concat(Enumerable.Range(0, x + takeContinueNumber - values.Length)).ToArray()).ToArray();  //获取值位置 连续四位的索引组合
                combine = new Combination(values.Length - takeContinueNumber);
                int[][] remainPosKeys = combine.GetRowsForAllPicks().Where(t => t.Picks == 5 - takeContinueNumber).Select(t => (from s in t select s).ToArray()).ToArray(); //获取去掉四个连续位置后，取一码的索引位置组合

                IEnumerable<int[]> betValues = valuePosKeys.SelectMany(x =>
                {
                    int[] continued = x.Select(t => values[t]).OrderBy(t => t).ToArray();
                    if (takeContinueNumber < 5)
                    {
                        int[] remained = values.Except(continued).ToArray();
                        return remainPosKeys.Select(t => t.Select(s => remained[s]).Concat(continued).OrderBy(s => s).ToArray()).ToArray();
                    }
                    return new int[][] { continued };
                });

                betValues = betValues.Select(x => new { Key = string.Join("", x), Array = x }).GroupBy(x => x.Key).Select(x => x.First().Array).ToArray();

                betValueDic.Add(posFactor, betValues.ToArray());
            }

            Dictionary<int, FactorTypeEnum> posMappings = new Dictionary<int, FactorTypeEnum>
            {
                { 0, FactorTypeEnum.Wan},
                { 1, FactorTypeEnum.Thousand},
                { 2, FactorTypeEnum.Hundred},
                { 3, FactorTypeEnum.Decade},
                { 4, FactorTypeEnum.Unit}
            };

            IEnumerable<LotteryResult> list = posKeys.SelectMany(x =>
            {
                var betArray = x.Select(t => new { Pos = t, Values = betValueDic[posMappings[t]] }).ToArray();
                var filters = from t in betArray[0].Values
                              from s in betArray[1].Values
                              select new AnyFilter[] { new AnyFilter { Pos = betArray[0].Pos, Values = t }, new AnyFilter { Pos = betArray[1].Pos, Values = s } };

                return filters.Select(t => GetFilteredResult(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, t)).ToArray();
            }).ToArray();

            return InferResult(list);
        }

        private LotteryResult GetAnyTwoResultByHeat()
        {
            FactorTypeEnum[] posFactors = new FactorTypeEnum[] { FactorTypeEnum.Wan, FactorTypeEnum.Thousand, FactorTypeEnum.Hundred, FactorTypeEnum.Decade, FactorTypeEnum.Unit };

            Dictionary<FactorTypeEnum, int> posMappings = new Dictionary<FactorTypeEnum, int>
            {
                { FactorTypeEnum.Wan,0},
                { FactorTypeEnum.Thousand,1},
                { FactorTypeEnum.Hundred,2},
                { FactorTypeEnum.Decade,3},
                { FactorTypeEnum.Unit,4}
            };

            var query = posFactors.Select(x =>
             {
                 Dictionary<int, ReferenceFactor> posReference = FactorDic[x];
                 Dictionary<int, int> heatDic = posReference.Select(t => t.Value).GroupBy(t => t.Heat).ToDictionary(t => t.Key, t => t.Count());
                 for (int i = 1; i <= 7; i++)
                 {
                     if (!heatDic.ContainsKey(i))
                     {
                         heatDic.Add(i, 0);
                     }
                 }

                 return new { ReferenceDic = posReference, HeatDic = heatDic, Factor = x };
             }).OrderByDescending(x => x.HeatDic[1])
                   .ThenByDescending(x => x.HeatDic[2])
                   .ThenByDescending(x => x.HeatDic[3])
                   .ThenBy(x => x.HeatDic[7])
                   .ThenBy(x => x.HeatDic[6])
                   .ToArray();

            AnyFilter[] betArray = query
               .Take(2)
               .Select((x,i) =>
               {
                   int[] values = x.ReferenceDic.Values.OrderBy(t => t.Heat)
                                                                            .ThenByDescending(t => t.LastInterval)
                                                                            .Skip(5)
                                                                            .Select(t => t.Key)
                                                                            .OrderBy(t => t)
                                                                            .ToArray();
                   return new AnyFilter { Pos = posMappings[x.Factor], Values = values };
               })
               .OrderBy(x => x.Pos)
               .ToArray();

            return new LotteryResult { AnyFilters = betArray, Filter = string.Join("   ", betArray.Select(t => Format(t))) };
        }

        private bool CheckInterval(int[] intervals)
        {
            int[] unconIntervals = intervals.Where(c => c > 0).ToArray();
            return unconIntervals.Skip(unconIntervals.Length - 3).All(c => c <= 5);
        }

        private LotteryResult InferResult(IEnumerable<LotteryResult> list)
        {
            return list.OrderByDescending(t => t.HitCount).ThenBy(t => t.MaxInterval).ThenBy(t => t.LastInterval).FirstOrDefault();
        }

        private LotteryResult[] InferResults(IEnumerable<LotteryResult> list)
        {
            var query = from p in FactorDic[FactorTypeEnum.RepeatNumber]
                        join q in FactorDic[FactorTypeEnum.Award]
                           on new { p.Key, p.Value.LastInterval } equals new { q.Key, q.Value.LastInterval }
                        where q.Value.LastInterval <=5
                        orderby q.Value.LastInterval, q.Value.OccurCount descending
                        select p.Key;
            int[] pairs = query.ToArray();
            int[] warms = FactorDic[FactorTypeEnum.Award].Where(c => c.Value.LastInterval <= 5).OrderByDescending(c => c.Value.OccurCount).Take(7).Select(c => c.Key).ToArray();

            int[] repeats = FactorDic[FactorTypeEnum.Award].Keys.Where(t =>
             {
                 bool isValid = false;
                 if (InputOption.GameArgs == "13")
                 {
                     //bool currentLimit = intervals.Reverse().TakeWhile(c => c == 0).Count() <= 3;
                     //List<int> heads = new List<int> { };
                     //int head = 0;
                     //for (int i = 0; i < intervals.Length; i++)
                     //{
                     //    if (intervals[i] != 0)
                     //    {
                     //        head = i;
                     //    }
                     //    heads.Add(head);
                     //}
                     //var continuousHits = heads.GroupBy(c => c).Select(c => new { key = c.Key, count = c.Count() }).ToArray();
                     //bool isNotOverHeat = continuousHits.Where(c => c.count >= 4).Count() < 3;
                     //bool isNotCurrentOverHeat = continuousHits.Last().count <= 3;
                     //bool isNotOrphan = continuousHits.Skip(continuousHits.Length - 3).Where(c => c.count == 1).Count() < 2;

                     isValid = pairs.Contains(t) && warms.Contains(t);
                 }
                 else
                 {
                     int[] intervals = FactorDic[FactorTypeEnum.Award][t].HitIntervals;
                     isValid = CheckInterval(intervals);
                 }
                 return isValid;
             }).ToArray();

            LotteryResult[] availableList = list.Where(t => t.HitIntervals != null && CheckInterval(t.HitIntervals) && t.AnyFilters.SelectMany(q => q.Values).Distinct().All(q => repeats.Contains(q)))
                                                                        .OrderByDescending(t => t.WinCount)
                                                                        .ThenByDescending(t => t.LastInterval)
                                                                        .ThenBy(t => t.MaxInterval)
                                                                        .Take(3)
                                                                        .ToArray();

            return availableList;
        }

        private LotteryResult GetFilteredResult(int[] spans, OddEnum[] odds, SizeEnum[] sizes, PrimeEnum[] primes, int[] sums, int[] hundreds, int[] decades, int[] units, int[] maxes, int[] mines, int[] distincts, bool? excludeThree, int[] sequenceKeys = null, int[][] dynamicPosKeys = null, int[] fiveStarForms = null, AnyFilter[] anyFilters = null)
        {
            IEnumerable<LotteryNumber> lotteryNumbers= LotteryNumbers;

            IEnumerable<LotteryNumber> query = lotteryNumbers;

            #region 过滤
            if (spans != null && spans.Any())
            {
                query = query.Where(x => spans.Contains(x.Span));
            }
            if (sums != null && sums.Any())
            {
                query = query.Where(x => sums.Contains(x.Sum));
            }
            if (hundreds != null && hundreds.Any())
            {
                query = query.Where(x => hundreds.Contains(x.Hundred));
            }
            if (decades != null && decades.Any())
            {
                query = query.Where(x => decades.Contains(x.Decade));
            }
            if (units != null && units.Any())
            {
                query = query.Where(x => units.Contains(x.Unit));
            }
            if (maxes != null && maxes.Any())
            {
                query = query.Where(x => maxes.Contains(x.Max));
            }
            if (mines != null && mines.Any())
            {
                query = query.Where(x => mines.Contains(x.Min));
            }

            if (odds != null && odds.Any())
            {
                query = query.Where(x => odds.Contains(x.Odd));
            }
            if (sizes != null && sizes.Any())
            {
                query = query.Where(x => sizes.Contains(x.Size));
            }
            if (primes != null && primes.Any())
            {
                query = query.Where(x => primes.Contains(x.Prime));
            }

            if (distincts != null && distincts.Any())
            {
                query = query.Where(x => distincts.Contains(x.Distinct));
            }
            if (excludeThree.HasValue && excludeThree.Value)
            {
                query = query.Where(x => x.ZeroCount != 3 && x.OneCount != 3 && x.TwoCount != 3);
            }
            if (sequenceKeys != null)
            {
                query = query.Where(x => sequenceKeys.Contains(x.SequenceKey));
            }
            Dictionary<string, int> posCounter = new Dictionary<string, int> { };
            if (dynamicPosKeys != null && dynamicPosKeys.Any())
            {
                query = query.Where(x =>
                {
                    int count = (from p in x.BetKeyPairs
                                 from q in dynamicPosKeys
                                 where p.Intersect(q).Count() == q.Length
                                 select q).Count();
                    if (count > 0)
                    {
                        posCounter[x.Key] = count;
                    }
                    return count > 0;
                });
            }
            if (fiveStarForms != null && fiveStarForms.Any())
            {
                query = query.Where(x => fiveStarForms.Contains(x.FiveStarForm));
            }
            if (anyFilters != null && anyFilters.Any())
            {
                query = query.Where(x =>
                {
                    int[] values = new int[] { x.Wan, x.Thousand, x.Hundred, x.Decade, x.Unit };
                    return anyFilters.All(t => t.Values.Contains(values[t.Pos]));
                });
            }
            #endregion

            LotteryResult ret = new LotteryResult() { Numbers = query.ToArray() };
            ret.BetAmount = ret.Numbers.Length * 2;
            ret.BetCount = ret.Numbers.Length;

            Dictionary<string, LotteryNumber> numberDic = ret.Numbers.GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.First());
            LotteryResult[] hitQuery = lotteryNumbers.Select((x, i) => new LotteryResult { HitCount = numberDic.ContainsKey(x.Key) ? 1 : 0, HitPositions = new[] { i } }).Where(x => x.HitCount > 0).ToArray();

            ret.HitCount = hitQuery.Length;
            ret.HitPositions = hitQuery.SelectMany(x => x.HitPositions).ToArray();
            IEnumerable<string> hitKeys = lotteryNumbers.Where(x => numberDic.ContainsKey(x.Key)).Select(x => x.Key).Distinct().ToArray();
            ret.PosHitCount = posCounter.Where(x => hitKeys.Contains(x.Key)).Select(x => x.Value).Sum();

            if (ret.HitCount > 0)
            {
                int[] intervals = GetIntervals(ret.HitPositions, lotteryNumbers);
                ret.MaxInterval = intervals.Max();
                ret.LastInterval = intervals[intervals.Length - 1];
                ret.HitIntervals = intervals;
                ret.WinCount = intervals.Where(q => q < InputOption.BetCycle).Count();
            }

            Dictionary<string, object> filterDic = new Dictionary<string, object>
            {
                { "跨度", spans},
                { "和值", sums},
                { "最大值", maxes},
                { "最小值", mines},
                { "百位", hundreds},
                { "十位", decades},
                { "个位", units},
                { "大小", sizes},
                { "奇偶", odds},
                { "质合", primes},
                { "不定位", dynamicPosKeys},
                { "任选二", anyFilters}
            };
            ret.Filter = string.Join(Environment.NewLine, filterDic.Where(x => x.Value != null).Select(x =>
            {
                string filterDisplay = null;
                switch (x.Key)
                {
                    case "大小":
                        filterDisplay = string.Join(",", (SizeEnum[])x.Value);
                        break;
                    case "奇偶":
                        filterDisplay = string.Join(",", (OddEnum[])x.Value);
                        break;
                    case "质合":
                        filterDisplay = string.Join(",", (PrimeEnum[])x.Value);
                        break;
                    case "不定位":
                        filterDisplay = string.Join(";", Format(((int[][])x.Value).SelectMany(t => t).Distinct().ToArray()));
                        break;
                    case "任选二":
                        filterDisplay = string.Join(";", ((AnyFilter[])x.Value).Select(t => Format(t)));
                        break;
                    default:
                        filterDisplay = Format((int[])x.Value);
                        break;
                }
                return string.Concat(x.Key, "：", filterDisplay);
            }).ToArray());

            Dictionary<int, string> nameMapping = new Dictionary<int, string>
            {
                { 0, "万"},
                { 1, "千"},
                { 2, "百"},
                { 3, "十"},
                { 4, "个"}
            };
            if (anyFilters != null && anyFilters.Any())
            {
                ret.AnyFilters = anyFilters;
                ret.Title = string.Join(string.Empty, anyFilters.Select(t => nameMapping[t.Pos]));
            }
            if (dynamicPosKeys != null && dynamicPosKeys.Any())
            {
                ret.AnyFilters = dynamicPosKeys.Select(x => new AnyFilter { Pos = -1, Values = x }).ToArray();
            }

            return ret;
        }

        private int[] GetSkipArray(FactorTypeEnum type, int alreadySubtraced)
        {
            int remain = MaxSkipDic[type] - alreadySubtraced;
            remain = remain <= 0 ? 1 : (remain + 1);
            return Enumerable.Range(0, remain).ToArray();
        }

        private int[] GetIntervals(int[] occurPostions, IEnumerable<LotteryNumber> lotteryNumbers = null)
        {
            lotteryNumbers = lotteryNumbers != null ? lotteryNumbers : LotteryNumbers;
            int[] intervals = occurPostions.Select((x, i) => i == 0 ? x : x - occurPostions[i - 1] - 1).ToArray();
            intervals = intervals.Concat(new[] { lotteryNumbers.Count() - 1 - occurPostions[occurPostions.Length - 1] }).ToArray();
            return intervals;
        }

        private string Format(int[] filter)
        {
            string ret = string.Join(",", filter.OrderBy(x => CurrentLottery.Key == "pk10" && x == 0 ? 10 : x).Select(x => CurrentLottery.Key == "pk10" ? (x == 0 ? 10 : x).ToString("D2") : x.ToString()).ToArray());
            return ret;
        }

        private string Format(AnyFilter filter)
        {
            Dictionary<int, string> nameMapping = new Dictionary<int, string>
            {
                { 0, "万"},
                { 1, "千"},
                { 2, "百"},
                { 3, "十"},
                { 4, "个"}
            };
            return string.Concat(nameMapping[filter.Pos], " : ", string.Join(string.Empty, filter.Values)); ;
        }
    }
}
