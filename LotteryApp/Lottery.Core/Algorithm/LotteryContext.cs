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
                { FactorTypeEnum.LeftRepeatNumber, new Dictionary<int, ReferenceFactor> { } },
                { FactorTypeEnum.RightRepeatNumber, new Dictionary<int, ReferenceFactor> { } },
                { FactorTypeEnum.LeftAward, new Dictionary<int, ReferenceFactor> { } },
                { FactorTypeEnum.RightAward, new Dictionary<int, ReferenceFactor> { } },
                { FactorTypeEnum.AdjacentNumber, new Dictionary<int, ReferenceFactor> { } },
                { FactorTypeEnum.AllPairs, new Dictionary<int, ReferenceFactor> { } }
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
            Dictionary<FactorTypeEnum, int[]> typeDic = new Dictionary<FactorTypeEnum, int[]>
            {
                { FactorTypeEnum.Award, number.DistinctNumbers},
                { FactorTypeEnum.LeftAward, number.LeftAwards},
                { FactorTypeEnum.RightAward, number.RightAwards},
                { FactorTypeEnum.RepeatNumber, number.RepeatNumbers},
                { FactorTypeEnum.LeftRepeatNumber, number.LeftRepeatNumbers},
                { FactorTypeEnum.RightRepeatNumber, number.RightRepeatNumbers},
                { FactorTypeEnum.AdjacentNumber, number.AdjacentNumbers},
                { FactorTypeEnum.AllPairs, number.AllPairs}
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

                factor.LastInterval = LotteryNumbers.Length - factor.OccurPositions[factor.OccurCount - 1] - 1;
                factor.MaxInterval = intervals.Max();
                factor.HitIntervals = intervals;
                factor.FailureCount = intervals.Select(t => (int)Math.Floor((decimal)t / InputOption.BetCycle)).Sum();
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
                case "history":
                    ret = GetHistoryResult();
                    break;
                case "symmetric":
                    ret = GetSymmetricResult();
                    break;
                case "single":
                    ret = GetSingleResult();
                    break;
                case "fivestar":
                    int[] fiveStarForms = new int[] { 3, 4, 5, 6 };
                    Dictionary<int, ReferenceFactor> factors = FactorDic[FactorTypeEnum.FiveStarForm];
                    ret = factors.Where(x => fiveStarForms.Contains(x.Key)).OrderBy(x => x.Key).Select(x => x.Value.ToResult()).ToArray();
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
                           on p.Key equals q.Key
                        where q.Value.LastInterval <= 7
                        orderby q.Value.FailureCount, q.Value.OccurCount descending, q.Value.LastInterval descending
                        select q.Key;
            return Build(query, FactorTypeEnum.AllPairs);
        }

        private LotteryResult[] GetHistoryResult()
        {
            var query = from p in FactorDic[FactorTypeEnum.AllPairs]
                        orderby p.Value.FailureCount, p.Value.OccurCount descending, p.Value.LastInterval descending
                        select p.Key;
            return Build(query, FactorTypeEnum.AllPairs);
        }

        private LotteryResult[] GetSingleResult()
        {
            FactorTypeEnum r = InputOption.GameArgs == "front" ? FactorTypeEnum.LeftAward : (InputOption.GameArgs == "after" ? FactorTypeEnum.RightAward : FactorTypeEnum.Award);

            var query = from p in FactorDic[r]
                        where p.Value.LastInterval >= 2 && p.Value.FailureCount <= 1
                        orderby p.Value.FailureCount <= 1 ? 1 : p.Value.FailureCount, p.Value.OccurCount descending, p.Value.LastInterval descending
                        select p.Key;
            if (InputOption.GameArgs == "second")
            {
                query = query.Skip(1);
            }
            return Build(query, r);
        }

        private LotteryResult[] GetSymmetricResult()
        {
            FactorTypeEnum r = InputOption.GameArgs == "front" ? FactorTypeEnum.LeftRepeatNumber : FactorTypeEnum.RightRepeatNumber;
            FactorTypeEnum s = InputOption.GameArgs == "front" ? FactorTypeEnum.LeftAward : FactorTypeEnum.RightAward;

            var query = from p in FactorDic[r]
                        join q in FactorDic[s]
                           on p.Key equals q.Key
                        where p.Value.LastInterval <= q.Value.LastInterval && q.Value.LastInterval >= 2
                        orderby q.Value.LastInterval descending, q.Value.OccurCount descending
                        select p.Key;
            return Build(query, r);
        }

        private LotteryResult[] Build(IEnumerable<int> awards, FactorTypeEnum type)
        {
            return awards.Take(3).Select(c =>
            {
                ReferenceFactor factor = FactorDic[type][c];
                int[] values = type == FactorTypeEnum.AllPairs || type == FactorTypeEnum.AdjacentNumber ? new int[] { (c - 100) / 10, (c - 100) % 10 } : new int[] { c };
                return new LotteryResult
                {
                    GameName = InputOption.GameName,
                    HitIntervals = factor.HitIntervals,
                    HitCount = factor.OccurCount,
                    LotteryName = InputOption.LotteryName,
                    LastInterval = factor.LastInterval,
                    MaxInterval = factor.MaxInterval,
                    AnyFilters = new AnyFilter[]
                    {
                        new AnyFilter{  Values =values }
                    },
                    Filter = $"不定位：{string.Join(",", values)}"
                };
            }).ToArray();
        }

        private bool CheckInterval(int[] intervals)
        {
            int[] unconIntervals = intervals.Where(c => c > 0).ToArray();
            return unconIntervals.Skip(unconIntervals.Length - 3).All(c => c <= 5);
        }

        private LotteryResult[] InferResults(IEnumerable<LotteryResult> list)
        {
            var query = from p in FactorDic[FactorTypeEnum.RepeatNumber]
                        join q in FactorDic[FactorTypeEnum.Award]
                           on p.Key equals q.Key
                        where p.Value.LastInterval <= q.Value.LastInterval && p.Value.LastInterval <= 7
                        orderby q.Value.LastInterval descending, q.Value.OccurCount descending
                        select p.Key;
            int[] pairs = query.ToArray();

            int[] repeats = FactorDic[FactorTypeEnum.Award].Keys.Where(t =>
             {
                 bool isValid = false;
                 if (InputOption.GameArgs == "13" || InputOption.GameArgs == "17")
                 {
                     int[] intervals = FactorDic[FactorTypeEnum.Award][t].HitIntervals;
                     bool currentLimit = intervals.Reverse().TakeWhile(c => c == 0).Count() <= 3;
                     List<int> heads = new List<int> { };
                     int head = 0;
                     for (int i = 0; i < intervals.Length; i++)
                     {
                         if (intervals[i] != 0)
                         {
                             head = i;
                         }
                         heads.Add(head);
                     }
                     var continuousHits = heads.GroupBy(c => c).Select(c => new { key = c.Key, count = c.Count() }).ToArray();
                     bool isNotOverHeat = continuousHits.Where(c => c.count >= 4).Count() < 3;
                     bool isNotCurrentOverHeat = continuousHits.Last().count <= 3;
                     bool isNotOrphan = continuousHits.Skip(continuousHits.Length - 3).Where(c => c.count == 1).Count() < 2;

                     isValid = isNotOverHeat && isNotOrphan && pairs.Contains(t);
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
