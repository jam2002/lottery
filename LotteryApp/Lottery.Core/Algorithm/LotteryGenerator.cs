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
                    new CD.Lottery {  Key = "mdssc", DisplayName="美东两分彩",Source=4, StartIndex = 0, Length =5, MaxBetCount =200,  HasPair = false, HasDynamic = true },
                    new CD.Lottery {  Key = "tsssc", DisplayName="腾讯分分彩",Source=3, StartIndex = 0, Length =5, MaxBetCount =200,  HasPair = false, HasDynamic = true, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" } },
                    new CD.Lottery {  Key = "tsssc|front", DisplayName="腾讯分分彩 前三",Source=3, StartIndex = 0, Length =3, MaxBetCount =200,  HasPair = true, HasDynamic = true, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" } },
                    new CD.Lottery {  Key = "tsssc|middle", DisplayName="腾讯分分彩 中三",Source=3, StartIndex = 1, Length =3, MaxBetCount =200,  HasPair = true, HasDynamic = true, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" } },
                    new CD.Lottery {  Key = "tsssc|after", DisplayName="腾讯分分彩 后三",Source=3, StartIndex = 2, Length =3, MaxBetCount =200,  HasPair = true, HasDynamic = true, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" } },

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
                config = new LotteryMetaConfig { Lotteries = lotteries, Numbers = GetAllNumbers(3) };
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

            int[] array = type == 3 ? new int[] { x, y, z } : new int[] { x, y, z, p, q };
            int[] remainders = type == 3 ? new int[] { x % 3, y % 3, z % 3 } : new int[] { x % 3, y % 3, z % 3, p % 3, q % 3 };
            LotteryNumber number = new LotteryNumber
            {
                Key = string.Join(string.Empty, array),
                Max = array.Max(),
                Min = array.Min(),
                Sum = array.Sum(),
                Wan = type == 3 ? 0 : x,
                Thousand = type == 3 ? 0 : y,
                Hundred = type == 3 ? x : z,
                Decade = type == 3 ? y : p,
                Unit = type == 3 ? z : q,
                Odd = (OddEnum)Enum.Parse(oddType, string.Concat(odd.Contains(x) ? "Odd" : "Even", odd.Contains(y) ? "Odd" : "Even", odd.Contains(z) ? "Odd" : "Even")),
                Size = (SizeEnum)Enum.Parse(sizeType, string.Concat(large.Contains(x) ? "Large" : "Small", large.Contains(y) ? "Large" : "Small", large.Contains(z) ? "Large" : "Small")),
                Prime = (PrimeEnum)Enum.Parse(primeType, string.Concat(prime.Contains(x) ? "Prime" : "Composite", prime.Contains(y) ? "Prime" : "Composite", prime.Contains(z) ? "Prime" : "Composite"))
            };
            number.Span = number.Max - number.Min;
            number.ZeroCount = remainders.Where(t => t == 0).Count();
            number.OneCount = remainders.Where(t => t == 1).Count();
            number.TwoCount = remainders.Where(t => t == 2).Count();
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
                number.RepeatNumbers =threeArrays.Select(c => GetRepeats(c, null)).Where(c => c >= 0).Distinct().ToArray();

                number.LeftRepeats = new int[] { GetRepeats(left, 1) }.Distinct().Where(c => c >= 0).ToArray();
                number.MiddleRepeats = new int[] { GetRepeats(middle, 2) }.Distinct().Where(c => c >= 0).ToArray();
                number.RightRepeats = new int[] { GetRepeats(right, 3) }.Distinct().Where(c => c >= 0).ToArray();
                number.LeftAwards = left.Distinct().OrderBy(c => c).ToArray();
                number.MiddleAwards = middle.Distinct().OrderBy(c => c).ToArray();
                number.RightAwards = right.Distinct().OrderBy(c => c).ToArray();

                Combination combine = new Combination(number.DistinctNumbers.Length);
                var tmp = combine.GetRowsForAllPicks().Where(t => t.Picks == 2);
                number.AllPairs = tmp.Select(t => (from s in t select number.DistinctNumbers[s]).ToArray()).Select(t => 100 + t[0] * 10 + t[1]).ToArray();
            }
            else
            {
                number.LeftRepeats = number.RightRepeats = new int[][] { array }.Select(c => GetRepeats(c, null)).Where(c => c >= 0).Distinct().ToArray();
                number.LeftAwards = number.RightAwards = array.Distinct().OrderBy(c => c).ToArray();
                number.AdjacentNumbers = new int[] { };
                number.AllPairs = new int[] { };
            }
            return number;
        }

        private static int GetAdjacents(int[] array, int index)
        {
            int[] adjacents = array.GroupBy(c => c).Select(c => c.Key).OrderBy(c => c).ToArray();
            return adjacents.Length == 2 ? (100 + adjacents[0] * 10 + adjacents[1]) : -1;
        }

        private static int GetRepeats(int[] array, int? pos)
        {
            //int r = array[0] == array[2] || array[1] == array[2] || array[0] == array[1] ? array[1] : -1;

            int[] repeats = array.GroupBy(c => c).Where(c => c.Count() > 1).Select(c => c.Key).OrderBy(c => c).ToArray();
            int r = repeats.Any() ? repeats[0] : -1;

            if (pos == null || pos == 1)
                return r;
            else
                return r != -1 && array[0] != array[1] ? r : -1;
        }

        private static LotteryNumber[] GetAllNumbers(int type)
        {
            IEnumerable<LotteryNumber> query = null;
            int[] count = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            if (type == 3)
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
    }
}
