using LotteryApp.Data;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using Kw.Combinatorics;
using System.Collections.Generic;

namespace LotteryApp.Algorithm
{
    public class LotteryGenerator
    {
        public static LotteryMetaConfig GetConfig()
        {
            LotteryMetaConfig config = null;
            string path = @"..\..\config.json";
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
                Lottery[] lotteries = new Lottery[]
                {
                new Lottery {  Key = "cqssc", DisplayName="重庆时时彩", RegexPattern = @"(?<=\d{11}\s)(\d\s){4}\d", StartIndex = 0, Length =5, MaxBetCount =200,  HasPair = true, HasDynamic = false, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" } },
                new Lottery {  Key = "cqssc|front", DisplayName="重庆时时彩 前三", RegexPattern = @"(?<=\d{11}\s)(\d\s){4}\d", StartIndex = 0, Length =3, MaxBetCount =200,  HasPair = true, HasDynamic = true, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" } },
                new Lottery {  Key = "cqssc|middle", DisplayName="重庆时时彩 中三", RegexPattern = @"(?<=\d{11}\s)(\d\s){4}\d", StartIndex = 1, Length =3, MaxBetCount =200,  HasPair = true, HasDynamic = false, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" } },
                new Lottery {  Key = "cqssc|after", DisplayName="重庆时时彩 后三", RegexPattern = @"(?<=\d{11}\s)(\d\s){4}\d", StartIndex = 2, Length =3, MaxBetCount =200,  HasPair = true, HasDynamic = true, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" } },

                new Lottery {  Key = "xjssc|front", DisplayName="新疆时时彩 前三", RegexPattern = @"(?<=\d{10}\s)(\d\s){4}\d", StartIndex = 0, Length =3, MaxBetCount =200,  HasPair = true,HasDynamic = false, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" }},
                new Lottery {  Key = "xjssc|middle", DisplayName="新疆时时彩 中三", RegexPattern = @"(?<=\d{10}\s)(\d\s){4}\d", StartIndex = 1, Length =3, MaxBetCount =200,  HasPair = true,HasDynamic = false, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" }},
                new Lottery {  Key = "xjssc|after", DisplayName="新疆时时彩 后三", RegexPattern = @"(?<=\d{10}\s)(\d\s){4}\d", StartIndex = 2, Length =3, MaxBetCount =200,  HasPair = true,HasDynamic = true, TradingHours = new string[] { "00:00:00-02:00:00", "09:50:00-23:59:59" }},

                new Lottery {  Key = "tjssc|front", DisplayName="天津时时彩 前三", RegexPattern = @"(?<=\d{10}\s)(\d\s){4}\d", StartIndex = 0, Length =3,MaxBetCount =200,   HasPair = true,HasDynamic = true, TradingHours = new string[] {"09:00:00-23:00:00" }},
                new Lottery {  Key = "tjssc|middle", DisplayName="天津时时彩 中三", RegexPattern = @"(?<=\d{10}\s)(\d\s){4}\d", StartIndex = 1, Length =3,MaxBetCount =200,   HasPair = true,HasDynamic = false, TradingHours = new string[] {"09:00:00-23:00:00" }},
                new Lottery {  Key = "tjssc|after", DisplayName="天津时时彩 后三", RegexPattern = @"(?<=\d{10}\s)(\d\s){4}\d", StartIndex = 2, Length =3,MaxBetCount =200,   HasPair = true,HasDynamic = true, TradingHours = new string[] {"09:00:00-23:00:00" }},

                new Lottery {  Key = "pk10",  DisplayName="北京PK10 前三",  RegexPattern = @"(?<=\d{6}\s)(\d\d\s){9}\d\d", StartIndex = 0, Length =3, MaxBetCount =200,  HasPair = false,HasDynamic = false, TradingHours = new string[] { "07:30:00-23:00:00"}}
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

            int[][] sourceKeys = type == 3 ? new int[][] { array } : new int[][] { new[] { x, y, z }, new[] { z, p, q } };
            number.BetKeyPairs = sourceKeys.Select(t => t.Distinct().OrderBy(r => r).ToArray()).SelectMany(t => GetPosKeys(t)).Distinct().ToArray();
            return number;
        }

        private static int[] GetPosKeys(int[] source)
        {
            if (source.Length > 1)
            {
                Combination combine = new Combination(source.Length);
                int[] keypairs = combine.GetRowsForAllPicks().Where(t => t.Picks == 2).Select(t => string.Join(string.Empty, from s in t select source[s])).Select(t => int.Parse("1" + t)).ToArray();
                return keypairs;
            }
            return new int[] { };
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
            int type = numbers[0].Length;
            var query = numbers.Select(x =>
            {
                int[] values = x.Select(t => int.Parse(t.ToString())).ToArray();
                return type == 3 ? Build(values[0], values[1], values[2], 0, 0, type) : Build(values[0], values[1], values[2], values[3], values[4], type);
            }).ToArray();

            return query;
        }
    }
}
