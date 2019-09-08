using Kw.Combinatorics;
using Lottery.Core.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CD = Lottery.Core.Data;

namespace Lottery.Core.Algorithm
{
    public class LotteryGenerator
    {
        public static LotteryMetaConfig GetConfig()
        {
            LotteryMetaConfig config = null;
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            if (File.Exists(path))
            {
                string content = null;
                using (StreamReader sr = new StreamReader(path))
                {
                    content = sr.ReadToEnd();
                }
                config = JsonConvert.DeserializeObject<LotteryMetaConfig>(content);
            }
            else
            {
                CD.Lottery[] lotteries = new CD.Lottery[]
                {
                    new CD.Lottery {  Key = "qqssc", DisplayName="QQ分分彩",Source=4, StartIndex = 0, Length =5, MaxBetCount =200,  HasPair = false, HasDynamic = true },
                    new CD.Lottery {  Key = "tsssc", DisplayName="腾讯分分彩",Source=4, StartIndex = 0, Length =5, MaxBetCount =200,  HasPair = false, HasDynamic = true },
                    new CD.Lottery {  Key = "mdssc", DisplayName="美东分分彩", Source=4, StartIndex = 0, Length =5, MaxBetCount =200,  HasPair = false, HasDynamic = true }, 
                    new CD.Lottery {  Key = "djssc", DisplayName="东京1.5分彩",Source=4, StartIndex = 0, Length =5, MaxBetCount =200,  HasPair = false, HasDynamic = true }, 
                    new CD.Lottery {  Key = "tsssc|front", DisplayName="腾讯分分彩 前三",Source=3,  StartIndex = 0, Length =3, MaxBetCount =200,  HasPair = true, HasDynamic = true, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" } },
                    new CD.Lottery {  Key = "tsssc|middle", DisplayName="腾讯分分彩 中三",Source=3, StartIndex = 1, Length =3, MaxBetCount =200,  HasPair = true, HasDynamic = true, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" } },
                    new CD.Lottery {  Key = "tsssc|after", DisplayName="腾讯分分彩 后三",Source=3, StartIndex = 2, Length =3, MaxBetCount =200,  HasPair = true, HasDynamic = true, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" } },
                    new CD.Lottery {  Key = "tsssc|tuplea", DisplayName="腾讯分分彩 万千十",Source=3 , IndexKeys = new int[] { 0,1,3},Length =3,  MaxBetCount =200,  HasPair = true, HasDynamic = true, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" } },
                    new CD.Lottery {  Key = "tsssc|tupleb", DisplayName="腾讯分分彩 万千个",Source=3, IndexKeys = new int[] { 0,1,4}, Length =3, MaxBetCount =200,  HasPair = true, HasDynamic = true, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" } },
                    new CD.Lottery {  Key = "tsssc|tuplec", DisplayName="腾讯分分彩 万百十",Source=3, IndexKeys = new int[] { 0,2,3},Length =3,  MaxBetCount =200,  HasPair = true, HasDynamic = true, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" } },
                    new CD.Lottery {  Key = "tsssc|tupled", DisplayName="腾讯分分彩 万百个",Source=3, IndexKeys = new int[] { 0,2,4},Length =3,  MaxBetCount =200,  HasPair = true, HasDynamic = true, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" } },
                    new CD.Lottery {  Key = "tsssc|tuplee", DisplayName="腾讯分分彩 万十个",Source=3, IndexKeys = new int[] { 0,3,4}, Length =3, MaxBetCount =200,  HasPair = true, HasDynamic = true, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" } },
                    new CD.Lottery {  Key = "tsssc|tuplef", DisplayName="腾讯分分彩 千百个",Source=3, IndexKeys = new int[] { 1,2,4}, Length =3, MaxBetCount =200,  HasPair = true, HasDynamic = true, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" } },
                    new CD.Lottery {  Key = "tsssc|tupleg", DisplayName="腾讯分分彩 千十个",Source=3, IndexKeys = new int[] { 1,3,4},Length =3,  MaxBetCount =200,  HasPair = true, HasDynamic = true, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" } },

                    new CD.Lottery {  Key = "cqssc", DisplayName="重庆时时彩",Source=1, RegexPattern = @"(?<=\d{11}\s)(\d\s){4}\d", StartIndex = 0, Length =5, MaxBetCount =200,  HasPair = false, HasDynamic = true, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" } },
                    new CD.Lottery {  Key = "cqssc|front", DisplayName="重庆时时彩 前三",Source=1, RegexPattern = @"(?<=\d{11}\s)(\d\s){4}\d", StartIndex = 0, Length =3, MaxBetCount =200,  HasPair = true, HasDynamic = true, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" } },
                    new CD.Lottery {  Key = "cqssc|middle", DisplayName="重庆时时彩 中三",Source=1, RegexPattern = @"(?<=\d{11}\s)(\d\s){4}\d", StartIndex = 1, Length =3, MaxBetCount =200,  HasPair = true, HasDynamic = true, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" } },
                    new CD.Lottery {  Key = "cqssc|after", DisplayName="重庆时时彩 后三",Source=1, RegexPattern = @"(?<=\d{11}\s)(\d\s){4}\d", StartIndex = 2, Length =3, MaxBetCount =200,  HasPair = true, HasDynamic = true, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" } },

                    new CD.Lottery {  Key = "xjssc", DisplayName="新疆时时彩",Source=1, RegexPattern = @"(?<=\d{10}\s)(\d\s){4}\d", StartIndex = 0, Length =5, MaxBetCount =200,  HasPair = false,HasDynamic = true, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" }},
                    new CD.Lottery {  Key = "xjssc|front", DisplayName="新疆时时彩 前三",Source=1, RegexPattern = @"(?<=\d{10}\s)(\d\s){4}\d", StartIndex = 0, Length =3, MaxBetCount =200,  HasPair = true,HasDynamic = true, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" }},
                    new CD.Lottery {  Key = "xjssc|middle", DisplayName="新疆时时彩 中三",Source=1, RegexPattern = @"(?<=\d{10}\s)(\d\s){4}\d", StartIndex = 1, Length =3, MaxBetCount =200,  HasPair = true,HasDynamic = true, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" }},
                    new CD.Lottery {  Key = "xjssc|after", DisplayName="新疆时时彩 后三",Source=1, RegexPattern = @"(?<=\d{10}\s)(\d\s){4}\d", StartIndex = 2, Length =3, MaxBetCount =200,  HasPair = true,HasDynamic = true, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" }},

                    new CD.Lottery {  Key = "tjssc", DisplayName="天津时时彩", Source=1, RegexPattern = @"(?<=\d{10}\s)(\d\s){4}\d", StartIndex = 0, Length =5, MaxBetCount =200,  HasPair = false, HasDynamic = true, TradingHours = new string[] { "09:00:00-23:00:00" } },
                    new CD.Lottery {  Key = "tjssc|front", DisplayName="天津时时彩 前三",Source=1, RegexPattern = @"(?<=\d{10}\s)(\d\s){4}\d", StartIndex = 0, Length =3,MaxBetCount =200,   HasPair = true,HasDynamic = true, TradingHours = new string[] {"09:00:00-23:00:00" }},
                    new CD.Lottery {  Key = "tjssc|middle", DisplayName="天津时时彩 中三",Source=1, RegexPattern = @"(?<=\d{10}\s)(\d\s){4}\d", StartIndex = 1, Length =3,MaxBetCount =200,   HasPair = true,HasDynamic = true, TradingHours = new string[] {"09:00:00-23:00:00" }},
                    new CD.Lottery {  Key = "tjssc|after", DisplayName="天津时时彩 后三",Source=1, RegexPattern = @"(?<=\d{10}\s)(\d\s){4}\d", StartIndex = 2, Length =3,MaxBetCount =200,   HasPair = true,HasDynamic = true, TradingHours = new string[] {"09:00:00-23:00:00" }},

                    new CD.Lottery {  Key = "tjssc", DisplayName="云南时时彩", Source=1, RegexPattern = @"(?<=\d{10}\s)(\d\s){4}\d", StartIndex = 0, Length =5, MaxBetCount =200,  HasPair = false, HasDynamic = true, TradingHours = new string[] { "09:00:00-23:00:00" } },
                    new CD.Lottery {  Key = "tjssc|front", DisplayName="云南时时彩 前三",Source=1, RegexPattern = @"(?<=\d{10}\s)(\d\s){4}\d", StartIndex = 0, Length =3,MaxBetCount =200,   HasPair = true,HasDynamic = true, TradingHours = new string[] {"09:00:00-23:00:00" }},
                    new CD.Lottery {  Key = "tjssc|middle", DisplayName="云南时时彩 中三",Source=1, RegexPattern = @"(?<=\d{10}\s)(\d\s){4}\d", StartIndex = 1, Length =3,MaxBetCount =200,   HasPair = true,HasDynamic = true, TradingHours = new string[] {"09:00:00-23:00:00" }},
                    new CD.Lottery {  Key = "tjssc|after", DisplayName="云南时时彩 后三",Source=1, RegexPattern = @"(?<=\d{10}\s)(\d\s){4}\d", StartIndex = 2, Length =3,MaxBetCount =200,   HasPair = true,HasDynamic = true, TradingHours = new string[] {"09:00:00-23:00:00" }},

                    new CD.Lottery {  Key = "pk10",  DisplayName="北京PK10",  RegexPattern = @"(?<=\d{6}\s)(\d\d\s){9}\d\d", StartIndex = 0, Length =3, MaxBetCount =200,  HasPair = false,HasDynamic = false, TradingHours = new string[] { "07:30:00-23:00:00"}},

                    new CD.Lottery {  Key = "jx115",  DisplayName="江西11选5",  RegexPattern = @"(?<=\d{10}\s)(\d\d\s){4}\d\d", StartIndex = 0, Length =10, MaxBetCount =200,  HasPair = false,HasDynamic = true, TradingHours = new string[] { "09:00:00-23:00:00"}},
                    new CD.Lottery {  Key = "sd115",  DisplayName="山东11选5",  RegexPattern = @"(?<=\d{10}\s)(\d\d\s){4}\d\d", StartIndex = 0, Length =10, MaxBetCount =200,  HasPair = false,HasDynamic = true, TradingHours = new string[] { "09:00:00-23:00:00"}},
                    new CD.Lottery {  Key = "gd115",  DisplayName="广东11选5",  RegexPattern = @"(?<=\d{10}\s)(\d\d\s){4}\d\d", StartIndex = 0, Length =10, MaxBetCount =200,  HasPair = false,HasDynamic = true, TradingHours = new string[] { "09:00:00-23:00:00"}},
                    new CD.Lottery {  Key = "gs115",  DisplayName="甘肃11选5",  RegexPattern = @"(?<=\d{10}\s)(\d\d\s){4}\d\d", StartIndex = 0, Length =10, MaxBetCount =200,  HasPair = false,HasDynamic = true, TradingHours = new string[] { "09:00:00-23:00:00"}}
                };
                config = new LotteryMetaConfig { Lotteries = lotteries, ThreeNumbers = GetAllNumbers(3), TwoNumbers = GetAllNumbers(2) };
                string str = JsonConvert.SerializeObject(config);
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.Write(str);
                    sw.Flush();
                    sw.Close();
                }
            }
            return config;
        }

        private static LotteryNumber Build(int x, int y, int z, int p, int q, int type)
        {
            int[] large = new[] { 5, 6, 7, 8, 9 };
            int[] small = new[] { 0, 1, 2, 3, 4 };
            int[] odd = new[] { 1, 3, 5, 7, 9 };
            int[] even = new[] { 0, 2, 4, 6, 8 };
            int[] prime = new[] { 1, 2, 3, 5, 7 };
            int[] composite = new[] { 0, 4, 6, 8, 9 };

            Type oddType = typeof(OddEnum);
            Type sizeType = typeof(SizeEnum);
            Type primeType = typeof(PrimeEnum);

            int[] array = type == 2 ? new int[] { x, y } : (type == 3 ? new int[] { x, y, z } : new int[] { x, y, z, p, q });
            int[] remainders = type == 2 ? new int[] { x % 3, y % 3 } : (type == 3 ? new int[] { x % 3, y % 3, z % 3 } : new int[] { x % 3, y % 3, z % 3, p % 3, q % 3 });
            LotteryNumber number = new LotteryNumber
            {
                Key = string.Join(string.Empty, array),
                Max = array.Max(),
                Min = array.Min(),
                Sum = array.Sum(),
                Wan = type == 5 ? x : 0,
                Thousand = type == 5 ? y : 0,
                Hundred = type == 2 ? 0 : (type == 3 ? x : z),
                Decade = type == 2 ? x : (type == 3 ? y : p),
                Unit = type == 2 ? y : (type == 3 ? z : q),
                Odd = (OddEnum)Enum.Parse(oddType, string.Concat(odd.Contains(x) ? "Odd" : "Even", odd.Contains(y) ? "Odd" : "Even", odd.Contains(z) ? "Odd" : "Even")),
                Size = (SizeEnum)Enum.Parse(sizeType, string.Concat(large.Contains(x) ? "Large" : "Small", large.Contains(y) ? "Large" : "Small", large.Contains(z) ? "Large" : "Small")),
                Prime = (PrimeEnum)Enum.Parse(primeType, string.Concat(prime.Contains(x) ? "Prime" : "Composite", prime.Contains(y) ? "Prime" : "Composite", prime.Contains(z) ? "Prime" : "Composite"))
            };

            //number.OddType = number.Odd
            number.Span = number.Max - number.Min;
            number.ZeroCount = remainders.Where(t => t == 0).Count();
            number.OneCount = remainders.Where(t => t == 1).Count();
            number.TwoCount = remainders.Where(t => t == 2).Count();
            number.RawNumbers = array;
            number.DistinctNumbers = array.Distinct().OrderBy(t => t).ToArray();
            number.Distinct = number.DistinctNumbers.Length;
            number.SequenceKey = int.Parse("1" + string.Join(string.Empty, number.DistinctNumbers));
            number.BetKeyPairs = new int[][] { array };

            if (type == 5)
            {
                int[] repeats = array.GroupBy(t => t).Where(t => t.Count() > 1).Select(t => t.Key).OrderBy(t => t).ToArray();
                switch (number.Distinct)
                {
                    case 5:
                        number.FiveStarForm = 1;
                        break;
                    case 4:
                        number.FiveStarForm = 2;
                        break;
                    case 3:
                        number.FiveStarForm = repeats.Length == 2 ? 3 : 4;
                        break;
                    case 2:
                        number.FiveStarForm = repeats.Length == 2 ? 5 : 6;
                        break;
                }

                int[] left = array.Take(3).ToArray();
                int[] middle = array.Skip(1).Take(3).ToArray();
                int[] right = array.Skip(2).Take(3).ToArray();
                int[][] threeArrays = new int[][] { left, middle, right };

                number.AdjacentNumbers = threeArrays.Select((c, i) => GetAdjacents(c, i)).Where(c => c > 100).Distinct().ToArray();
                number.RepeatNumbers = threeArrays.Select(c => GetRepeats(c, null)).Where(c => c >= 0).Distinct().ToArray();

                number.LeftRepeats = new int[] { GetRepeats(left, 1) }.Distinct().Where(c => c >= 0).ToArray();
                number.LeftAwards = left.Distinct().OrderBy(c => c).ToArray();
                number.LeftRawAwards = left;
                number.LeftDistinct = number.LeftAwards.Length <= 2 ? 2 : 3;
                number.LeftTuples = GetTuples(left);
                number.Left4Tuples = GetTuples(array.Take(4).ToArray());
                number.LeftSpan = number.LeftAwards[number.LeftAwards.Length - 1] - number.LeftAwards[0];
                number.Left4Awards = array.Take(4).Distinct().OrderBy(c => c).ToArray();

                number.MiddleRepeats = new int[] { GetRepeats(middle, 2) }.Distinct().Where(c => c >= 0).ToArray();
                number.MiddleAwards = middle.Distinct().OrderBy(c => c).ToArray();
                number.MiddleRawAwards = middle;
                number.MiddleDistinct = number.MiddleAwards.Length <= 2 ? 2 : 3;
                number.MiddleTuples = GetTuples(middle);
                number.MiddleSpan = number.MiddleAwards[number.MiddleAwards.Length - 1] - number.MiddleAwards[0];

                number.RightRepeats = new int[] { GetRepeats(right, 3) }.Distinct().Where(c => c >= 0).ToArray();
                number.RightAwards = right.Distinct().OrderBy(c => c).ToArray();
                number.RightRawAwards = right;
                number.RightDistinct = number.RightAwards.Length <= 2 ? 2 : 3;
                number.RightTuples = GetTuples(right);
                number.Right4Tuples = GetTuples(array.Skip(1).ToArray());
                number.RightSpan = number.RightAwards[number.RightAwards.Length - 1] - number.RightAwards[0];
                number.Right4Awards = array.Skip(1).Distinct().OrderBy(c => c).ToArray();

                number.AllTuples = GetTuples(array);
                number.ATuples = GetTuples(new int[] { array[0], array[1], array[3] });
                number.BTuples = GetTuples(new int[] { array[0], array[1], array[4] });
                number.CTuples = GetTuples(new int[] { array[0], array[2], array[3] });
                number.DTuples = GetTuples(new int[] { array[0], array[2], array[4] });
                number.ETuples = GetTuples(new int[] { array[0], array[3], array[4] });
                number.FTuples = GetTuples(new int[] { array[1], array[2], array[4] });
                number.GTuples = GetTuples(new int[] { array[1], array[3], array[4] });

                number.AAwards = GetAwards(new int[] { array[0], array[1], array[3] });
                number.BAwards = GetAwards(new int[] { array[0], array[1], array[4] });
                number.CAwards = GetAwards(new int[] { array[0], array[2], array[3] });
                number.DAwards = GetAwards(new int[] { array[0], array[2], array[4] });
                number.EAwards = GetAwards(new int[] { array[0], array[3], array[4] });
                number.FAwards = GetAwards(new int[] { array[1], array[2], array[4] });
                number.GAwards = GetAwards(new int[] { array[1], array[3], array[4] });

                int[] a4 = new int[] { array[0], array[1], array[2], array[4] };
                int[] b4 = new int[] { array[0], array[1], array[3], array[4] };
                int[] c4 = new int[] { array[0], array[2], array[3], array[4] };
                number.Tuple4As = GetTuples(a4);
                number.Tuple4Bs = GetTuples(b4);
                number.Tuple4Cs = GetTuples(c4);
                number.Tuple4AAwards = GetAwards(a4);
                number.Tuple4BAwards = GetAwards(b4);
                number.Tuple4CAwards = GetAwards(c4);

                number.LeftPairs = GetAwards(new int[] { array[0], array[1] });
                number.RightPairs = GetAwards(new int[] { array[3], array[4] });
                number.APairs = GetAwards(new int[] { array[0], array[2] });
                number.BPairs = GetAwards(new int[] { array[0], array[3] });
                number.CPairs = GetAwards(new int[] { array[0], array[4] });
                number.DPairs = GetAwards(new int[] { array[1], array[2] });
                number.EPairs = GetAwards(new int[] { array[1], array[3] });
                number.FPairs = GetAwards(new int[] { array[1], array[4] });
                number.GPairs = GetAwards(new int[] { array[2], array[3] });
                number.HPairs = GetAwards(new int[] { array[2], array[4] });

                number.LeftPairTuples = GetTuples(new int[] { array[0], array[1] });
                number.RightPairTuples = GetTuples(new int[] { array[3], array[4] });
                number.APairTuples = GetTuples(new int[] { array[0], array[2] });
                number.BPairTuples = GetTuples(new int[] { array[0], array[3] });
                number.CPairTuples = GetTuples(new int[] { array[0], array[4] });
                number.DPairTuples = GetTuples(new int[] { array[1], array[2] });
                number.EPairTuples = GetTuples(new int[] { array[1], array[3] });
                number.FPairTuples = GetTuples(new int[] { array[1], array[4] });
                number.GPairTuples = GetTuples(new int[] { array[2], array[3] });
                number.HPairTuples = GetTuples(new int[] { array[2], array[4] });
            }
            else
            {
                number.RepeatNumbers = number.LeftRepeats = number.RightRepeats = number.MiddleRepeats = new int[][] { array }.Select(c => GetRepeats(c, null)).Where(c => c >= 0).Distinct().ToArray();
                number.LeftAwards = number.RightAwards = number.MiddleAwards = array.Distinct().OrderBy(c => c).ToArray();
                number.LeftDistinct = number.RightDistinct = number.MiddleDistinct = number.LeftAwards.Length <= 2 ? 2 : 3;
                number.LeftTuples = number.RightTuples = number.MiddleTuples = number.Left4Tuples = number.Right4Tuples = number.AllTuples = GetTuples(array);
                number.LeftSpan = number.RightSpan = number.MiddleSpan = number.Span;
                number.AdjacentNumbers = new int[] { };
            }

            number.AllPairs = GetPairs(number.DistinctNumbers);

            return number;
        }

        private static readonly int[] AllKeys = Enumerable.Range(0, 10).ToArray();

        private static int[] GetPairs(int[] array)
        {
            int[] sort = array.Distinct().OrderBy(c => c).ToArray();
            Combination combine = new Combination(sort.Length);
            return combine.GetRowsForAllPicks().Where(t => t.Picks == 2).Select(t => (from s in t select sort[s]).ToArray()).Select(t => 100 + t[0] * 10 + t[1]).ToArray();
        }

        private static int[] GetTuples(int[] array)
        {
            int[] sort = array.Distinct().OrderBy(c => c).ToArray();
            Combination combine = new Combination(sort.Length);
            int[] ret = new int[] { };

            if (sort.Length >= 2 && Number > 0 && TupleLength > Number && sort.Length >= Number)
            {
                int[][] awards = combine.GetRowsForAllPicks().Where(t => t.Picks == Number).Select(t => (from s in t select sort[s]).ToArray()).ToArray();

                int remainCount = TupleLength - Number;
                ret = awards.SelectMany(c =>
                {
                    int[] remains = AllKeys.Where(t => !c.Contains(t)).ToArray();
                    Combination subCombine = new Combination(remains.Length);
                    int[] selectedAwards = subCombine.GetRowsForAllPicks().Where(t => t.Picks == remainCount).Select(t =>
                    {
                        int[] temp = c.Concat(from s in t select remains[s]).OrderBy(s => s).ToArray();
                        int k = (int)Math.Pow(10, TupleLength) + temp.Aggregate((x, y) => x * 10 + y);
                        return k;
                    }).ToArray();
                    return selectedAwards;
                }).Distinct().ToArray();
            }
            return ret;
        }

        private static int[] GetAwards(int[] array)
        {
            return array.Distinct().OrderBy(c => c).ToArray();
        }

        private static int GetAdjacents(int[] array, int index)
        {
            int[] adjacents = array.GroupBy(c => c).Select(c => c.Key).OrderBy(c => c).ToArray();
            return adjacents.Length == 2 ? (100 + adjacents[0] * 10 + adjacents[1]) : -1;
        }

        private static int GetRepeats(int[] array, int? pos)
        {
            int[] repeats = array.GroupBy(c => c).Where(c => c.Count() > 1).Select(c => c.Key).OrderBy(c => c).ToArray();
            int r = repeats.Any() ? repeats[0] : -1;
            return r;
        }

        private static LotteryNumber[] GetAllNumbers(int type)
        {
            IEnumerable<LotteryNumber> query = null;
            int[] count = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            if (type == 2)
            {
                query = from x in count
                        from y in count
                        select Build(x, y, 0, 0, 0, 2);
            }
            else if (type == 3)
            {
                query = from x in count
                        from y in count
                        from z in count
                        select Build(x, y, z, 0, 0, 3);
            }
            else
            {
                query = from x in count
                        from y in count
                        from z in count
                        from p in count
                        from q in count
                        select Build(x, y, z, p, q, 5);
            }
            return query.ToArray();
        }

        public static LotteryNumber[] GetNumbers(string[] numbers)
        {
            int[] splitIndex = new int[] { 0, 2, 4, 6, 8 };
            int type = numbers[0].Length;

            var query = numbers.Select(x =>
            {
                int[] values = type <= 5 ? x.Select(t => int.Parse(t.ToString())).ToArray() : splitIndex.Select(t => int.Parse(x.Substring(t, 2))).ToArray();
                return type == 3 ? Build(values[0], values[1], values[2], 0, 0, type) : Build(values[0], values[1], values[2], values[3], values[4], type);
            }).ToArray();

            return query;
        }

        public static int TupleLength { get; set; }

        public static int Number { get; set; }
    }
}
