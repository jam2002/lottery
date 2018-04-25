using Kw.Combinatorics;
using LotteryApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LotteryApp.Algorithm
{
    public class LotteryContext
    {
        public Lottery CurrentLottery { get; private set; }
        public LotteryMetaConfig Config { get; private set; }
        public LotteryNumber[] LotteryNumbers { get; private set; }
        public Dictionary<FactorTypeEnum, Dictionary<int, ReferenceFactor>> FactorDic { get; private set; }
        public Dictionary<FactorTypeEnum, int> MaxSkipDic { get; private set; }
        public int[] DistinctCounts { get; private set; }
        public string Args { get; private set; }

        public LotteryContext(LotteryMetaConfig config, LotteryNumber[] numbers, string name, string args)
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
                { FactorTypeEnum.FiveStarForm, new Dictionary<int, ReferenceFactor> { } }
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
            CurrentLottery = config.Lotteries.Where(x => x.Key == name).First();
            DistinctCounts = CurrentLottery.HasPair ? null : new int[] { 3 };
            Args = args;

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
            int[] awards = new[] { number.Hundred, number.Decade, number.Unit }.Distinct().ToArray();
            foreach (int key in awards)
            {
                BuildFactor(FactorTypeEnum.Award, key, pos);
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

            FactorTypeEnum[] posFactorTypes = new FactorTypeEnum[] { FactorTypeEnum.Wan, FactorTypeEnum.Thousand, FactorTypeEnum.Hundred, FactorTypeEnum.Decade, FactorTypeEnum.Unit, FactorTypeEnum.Award };
            int[] awards = Enumerable.Range(0, 10).ToArray();
            foreach (FactorTypeEnum factorType in posFactorTypes)
            {
                Dictionary<int, ReferenceFactor> referenceDic = FactorDic[factorType];
                foreach (int v in awards.Except(FactorDic[factorType].Keys))
                {
                    referenceDic.Add(v, new ReferenceFactor { Key = v, HitIntervals = new int[] { }, LastInterval = LotteryNumbers.Length, MaxInterval = LotteryNumbers.Length, OccurCount = 0, OccurPositions = new int[] { }, Type = factorType });
                }
            }
        }

        public CompositeLotteryResult GetCompositeResult()
        {
            CompositeLotteryResult ret = new CompositeLotteryResult { };
            ret.DynamicPosition = GetDynamicPosResult();

            if (CurrentLottery.Length == 3)
            {
                ret.Mix = GetMixResult();
                ret.Compound = GetCompoundResult();
                ret.Duplicated = GetDuplicatedResult();

                ReferenceFactor pair = FactorDic[FactorTypeEnum.Distinct].ContainsKey(2) ? FactorDic[FactorTypeEnum.Distinct][2] : null;
                if (pair != null && pair.MaxInterval <= 5)
                {
                    ret.GroupThree = GetFilteredResult(null, null, null, null, null, null, null, null, null, null, new int[] { 2 }, null);
                }
                else
                {
                    ret.GroupSix = GetGroupSixResult();
                }
            }

            if (CurrentLottery.Length == 5)
            {
                int[] fiveStarForms = Args == null ? new int[] { 1, 2, 3, 4, 5, 6 } : Args.Split(',')[0].Select(x => int.Parse(x.ToString())).ToArray();
                Dictionary<int, ReferenceFactor> factors = FactorDic[FactorTypeEnum.FiveStarForm];
                ret.FiveStar = factors.Where(x => fiveStarForms.Contains(x.Key)).ToDictionary(x => (FiveStarFormEnum)x.Key, x => new LotteryResult
                {
                    MaxInterval = x.Value.MaxInterval,
                    LastInterval = x.Value.LastInterval,
                    HitPositions = x.Value.OccurPositions,
                    HitIntervals = x.Value.HitIntervals,
                    HitCount = x.Value.OccurPositions.Length
                });

                int[] poses = new int[] { 0, 1, 2, 3, 4 };
                Combination combine = new Combination(poses.Length);
                int[][] posKeys = combine.GetRowsForAllPicks().Where(t => t.Picks == 2).Select(t => (from s in t select poses[s]).ToArray()).ToArray(); //获取健位置索引组合，比如万百，万千，万十

                int[][] valuePosKeys = Enumerable.Range(0, 9).Select(x => x + 4 < 9 ? Enumerable.Range(x, 5).ToArray() : Enumerable.Range(x, 9 - x).Concat(Enumerable.Range(0, x - 4)).ToArray()).ToArray(); //获取值位置索引组合，并且是连续的，比如01234,80123,总位置为9位，考虑杀一个冷码

                FactorTypeEnum[] posFactors = new FactorTypeEnum[] { FactorTypeEnum.Wan, FactorTypeEnum.Thousand, FactorTypeEnum.Hundred, FactorTypeEnum.Decade, FactorTypeEnum.Unit };
                Dictionary<FactorTypeEnum, int[][]> betValueDic = new Dictionary<FactorTypeEnum, int[][]> { };
                foreach (FactorTypeEnum posFactor in posFactors)
                {
                    Dictionary<int, ReferenceFactor> posReference = FactorDic[posFactor];
                    int[] values = posReference.Values.OrderBy(x => x.OccurCount).ThenByDescending(x => x.LastInterval).Skip(1).Select(x => x.Key).OrderBy(x => x).ToArray(); //获取值组合，此处杀了一个冷码

                    int[][] betValues = valuePosKeys.Select(x => x.Select(t => values[t]).ToArray()).ToArray(); //根据索引取值
                    betValueDic.Add(posFactor, betValues);
                }

                Dictionary<int, FactorTypeEnum> posMappings = new Dictionary<int, FactorTypeEnum>
                {
                    { 0, FactorTypeEnum.Wan},
                    { 1, FactorTypeEnum.Thousand},
                    { 2, FactorTypeEnum.Hundred},
                    { 3, FactorTypeEnum.Decade},
                    { 4, FactorTypeEnum.Unit}
                };

                ret.AnyTwo = posKeys.Select(x =>
                {
                    var betArray = x.Select(t => new { Pos = t, Values = betValueDic[posMappings[t]] }).ToArray();
                    var filters = from t in betArray[0].Values
                                  from s in betArray[1].Values
                                  select new AnyFilter[] { new AnyFilter { Pos = betArray[0].Pos, Values = t }, new AnyFilter { Pos = betArray[1].Pos, Values = s } };

                    IEnumerable<LotteryResult> list = filters.Select(t => GetFilteredResult(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, t)).ToArray();
                    return new { Key = string.Join(string.Empty, x), Result = InferResult(list, "any") };

                }).Where(x => x.Result != null).ToDictionary(x => x.Key, x => x.Result);
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

        private LotteryResult GetSimpleMixResult()
        {
            int[] orderSpans = FactorDic[FactorTypeEnum.Span].OrderBy(x => x.Value.OrderKey).Select(x => x.Key).ToArray();
            int[] orderMax = FactorDic[FactorTypeEnum.Max].Where(x => x.Key >= 4).OrderBy(x => x.Value.OrderKey).Select(x => x.Key).ToArray();
            int[] orderMin = FactorDic[FactorTypeEnum.Min].Where(x => x.Key <= 6).OrderBy(x => x.Value.OrderKey).Select(x => x.Key).ToArray();

            int[][] allSkips = (from x in Enumerable.Range(2, 4)
                                from y in Enumerable.Range(0, 5)
                                from z in Enumerable.Range(0, 5)
                                select new[] { x, y, z }).ToArray();

            IEnumerable<LotteryResult> list = allSkips.Select(x => GetFilteredResult(orderSpans.Skip(x[0]).ToArray(),
                                                                                                                                    null,
                                                                                                                                    null,
                                                                                                                                    null,
                                                                                                                                    null,
                                                                                                                                    null,
                                                                                                                                    null,
                                                                                                                                    null,
                                                                                                                                    orderMax.Skip(x[1]).ToArray(),
                                                                                                                                    orderMin.Skip(x[2]).ToArray(),
                                                                                                                                    DistinctCounts, null)).ToArray();

            return InferResult(list);
        }

        private LotteryResult GetCompoundResult()
        {
            int[] orderHundreds = FactorDic[FactorTypeEnum.Hundred].OrderBy(x => x.Value.OrderKey).Select(x => x.Key).ToArray();
            int[] orderDecades = FactorDic[FactorTypeEnum.Decade].OrderBy(x => x.Value.OrderKey).Select(x => x.Key).ToArray();
            int[] orderUnits = FactorDic[FactorTypeEnum.Unit].OrderBy(x => x.Value.OrderKey).Select(x => x.Key).ToArray();

            int[][] allSkips = (from x in GetSkipArray(FactorTypeEnum.Award, 10 - orderHundreds.Length)
                                from y in GetSkipArray(FactorTypeEnum.Award, 10 - orderDecades.Length)
                                from z in GetSkipArray(FactorTypeEnum.Award, 10 - orderUnits.Length)
                                select new[] { x, y, z }).ToArray();

            IEnumerable<LotteryResult> list = allSkips.Select(x => GetFilteredResult(null,
                                                                                                                                    null,
                                                                                                                                    null,
                                                                                                                                    null,
                                                                                                                                    null,
                                                                                                                                    orderHundreds.Skip(x[0]).ToArray(),
                                                                                                                                    orderDecades.Skip(x[1]).ToArray(),
                                                                                                                                    orderUnits.Skip(x[2]).ToArray(),
                                                                                                                                    null,
                                                                                                                                    null,
                                                                                                                                    DistinctCounts, null)).ToArray();

            return InferResult(list);
        }

        private LotteryResult GetGroupSixResult()
        {
            int[] awards = FactorDic[FactorTypeEnum.Award].OrderBy(x => x.Value.OrderKey).Select(x => x.Key).ToArray();

            int[][] allSkips = Enumerable.Range(3, 2).Select(x => new int[] { x, x, x }).ToArray();

            IEnumerable<LotteryResult> list = allSkips.Select(x => GetFilteredResult(null,
                                                                                                                                    null,
                                                                                                                                    null,
                                                                                                                                    null,
                                                                                                                                    null,
                                                                                                                                    awards.Skip(x[0]).ToArray(),
                                                                                                                                    awards.Skip(x[1]).ToArray(),
                                                                                                                                    awards.Skip(x[2]).ToArray(),
                                                                                                                                    null,
                                                                                                                                    null,
                                                                                                                                    new int[] { 3 }, null)).ToArray();

            return InferResult(list, "six");
        }

        private LotteryResult GetDuplicatedResult()
        {
            int[] sequenceKeys = FactorDic[FactorTypeEnum.SequenceKey].Where(x => x.Value.OccurCount > 1 && x.Value.LastInterval <= 15).OrderByDescending(x => x.Value.OccurCount).Select(x => x.Key).ToArray();

            LotteryResult ret = GetFilteredResult(null, null, null, null, null, null, null, null, null, null, null, null, sequenceKeys);
            return ret;
        }

        private LotteryResult GetDynamicPosResult()
        {
            string[] arguments = (Args ?? "34").Split(',');
            int[] number = arguments[0].Select(x => int.Parse(x.ToString())).ToArray();
            int keyCount = number[0];//码数，任选二，任选三，任选四
            int betCount = number.Length > 1 ? number[1] : keyCount;//投注数，任选二选二码，任选三选三码，四码，任选四选四码，五码

            int[][] posKeys = null;
            int[] numbers = arguments.Length > 1 ? arguments[1].Select(x => int.Parse(x.ToString())).OrderBy(x => x).ToArray() : (CurrentLottery.Length <= 5 ? new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 } : new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 });

            Combination combine = new Combination(numbers.Length);

            posKeys = combine.GetRowsForAllPicks().Where(t => t.Picks == betCount).Select(t => (from s in t select numbers[s]).ToArray()).ToArray();
            combine = new Combination(betCount);
            Func<int[], int[][]> getBetPosKeys = p => combine.GetRowsForAllPicks().Where(t => t.Picks == keyCount).Select(t => (from s in t select p[s]).ToArray()).ToArray();

            IEnumerable<LotteryResult> list = posKeys.Select(x => GetFilteredResult(null, null, null, null, null, null, null, null, null, null, null, null, null, getBetPosKeys(x))).ToArray();
            LotteryResult ret = arguments.Length > 1 ? list.FirstOrDefault() : InferResult(list, "any");

            return ret;
        }

        private LotteryResult InferResult(IEnumerable<LotteryResult> list, string type = null)
        {
            int maxBetCount = type == "six" ? 220 : CurrentLottery.MaxBetCount;
            int maxIntervalCount = type == "any" ? 5 : (type == "dynamic" ? 9 : 9);
            LotteryResult[] availableList = list.Where(x => x.MaxInterval < maxIntervalCount && (type == "dynamic" || type == "any" ? true : x.BetCount < maxBetCount))
                                                                 .OrderBy(t => t.MaxInterval)
                                                                 .ThenByDescending(t => t.LastContinuous)
                                                                 .ThenByDescending(t => t.HitCount)
                                                                 .ThenBy(t => t.LastInterval)
                                                                 .ToArray();

            LotteryResult result = availableList.FirstOrDefault();
            return result;
        }

        private LotteryResult GetFilteredResult(int[] spans, OddEnum[] odds, SizeEnum[] sizes, PrimeEnum[] primes, int[] sums, int[] hundreds, int[] decades, int[] units, int[] maxes, int[] mines, int[] distincts, bool? excludeThree, int[] sequenceKeys = null, int[][] dynamicPosKeys = null, int[] fiveStarForms = null, AnyFilter[] anyFilters = null)
        {
            IEnumerable<LotteryNumber> query = CurrentLottery.Length == 3 ? Config.Numbers : LotteryNumbers;

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
            LotteryResult[] hitQuery = LotteryNumbers.Select((x, i) => new LotteryResult { HitCount = numberDic.ContainsKey(x.Key) ? 1 : 0, HitPositions = new[] { i } }).Where(x => x.HitCount > 0).ToArray();

            ret.HitCount = hitQuery.Length;
            ret.HitPositions = hitQuery.SelectMany(x => x.HitPositions).ToArray();
            IEnumerable<string> hitKeys = LotteryNumbers.Where(x => numberDic.ContainsKey(x.Key)).Select(x => x.Key).Distinct().ToArray();
            ret.PosHitCount = posCounter.Where(x => hitKeys.Contains(x.Key)).Select(x => x.Value).Sum();

            if (ret.HitCount > 0)
            {
                int[] intervals = GetIntervals(ret.HitPositions);
                ret.MaxInterval = intervals.Max();
                ret.LastInterval = intervals[intervals.Length - 1];
                ret.HitIntervals = intervals;

                List<Tuple<int, int>> tuples = new List<Tuple<int, int>> { };
                for (int i = 0; i < intervals.Length; i++)
                {
                    int key = intervals[i];
                    if (key == 0)
                    {
                        int j = i;
                        while (j >= 0 && intervals[j] == 0)
                        {
                            j--;
                        }
                        tuples.Add(new Tuple<int, int>(j, i - j + 1));
                    }
                }
                var continuousArray = tuples.GroupBy(x => x.Item1).Select(x => new { Start = x.Key, MaxCount = x.Select(t => t.Item2).Max() }).ToArray();
                ret.MaxContinuous = continuousArray.Any() ? continuousArray.Max(x => x.MaxCount) : 0;
                ret.LastContinuous = continuousArray.Any() ? continuousArray.Last().MaxCount : 0;
            }
            else
            {
                ret.MaxInterval = ret.LastInterval = LotteryNumbers.Length;
                ret.MaxContinuous = ret.LastContinuous = 0;
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
                { "不定胆", dynamicPosKeys},
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
                    case "不定胆":
                        filterDisplay = string.Join(";", ((int[][])x.Value).Select(t => Format(t)));
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
                ret.Title = string.Join(string.Empty, anyFilters.Select(t => nameMapping[t.Pos]));
            }

            return ret;
        }

        private int[] GetSkipArray(FactorTypeEnum type, int alreadySubtraced)
        {
            int remain = MaxSkipDic[type] - alreadySubtraced;
            remain = remain <= 0 ? 1 : (remain + 1);
            return Enumerable.Range(0, remain).ToArray();
        }

        private int[] GetIntervals(int[] occurPostions)
        {
            int[] intervals = occurPostions.Select((x, i) => i == 0 ? x : x - occurPostions[i - 1] - 1).ToArray();
            intervals = intervals.Concat(new[] { LotteryNumbers.Length - 1 - occurPostions[occurPostions.Length - 1] }).ToArray();
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
