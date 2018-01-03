using LotteryApp.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace LotteryApp.Algorithm
{
    public class Calculator
    {
        Lottery lottery;
        LotteryMetaConfig config;
        string type;
        string algorithmArgs;

        public int TakeNumber { get; private set; }

        /// <summary>
        /// 支持 cq, xj, tj
        /// </summary>
        /// <param name="name"></param>
        public Calculator(string name, string inputType, int takeNumber, string args)
        {
            config = LotteryGenerator.GetConfig();
            lottery = config.Lotteries.Where(x => x.Key == name).First();
            TakeNumber = takeNumber;
            type = inputType;
            algorithmArgs = args;
        }

        public bool Start()
        {
            if (!CanRun())
            {
                return false;
            }

            string[] lotteries = GetLotteries();
            LotteryNumber[] selectedLottery = null;
            if (lottery.Length == 5)
            {
                selectedLottery = LotteryGenerator.GetNumbers(lotteries);
            }
            else
            {
                Dictionary<string, LotteryNumber> lotteryDic = config.Numbers.ToDictionary(x => x.Key, x => x);
                selectedLottery = lotteries.Select(x => lotteryDic[x]).ToArray();
            }

            LotteryContext context = new LotteryContext(config, selectedLottery, lottery.Key, algorithmArgs);
            CompositeLotteryResult ret = context.GetCompositeResult();

            string[] types = type.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            IEnumerable<Tuple<string, string, LotteryResult>> list = new Tuple<string, string, LotteryResult>[]
            {
                new Tuple<string,string, LotteryResult>("three","组三投注策略",ret.GroupThree),
                new Tuple<string, string,LotteryResult>("six","组三投注策略",ret.GroupSix),
                new Tuple<string,string, LotteryResult>("compound","复式投注策略",ret.Compound),
                new Tuple<string,string, LotteryResult>("mix","单式投注策略",ret.Mix),
                new Tuple<string,string, LotteryResult>("duplicated","直选投注策略",ret.Duplicated),
                new Tuple<string, string,LotteryResult>("dynamic","不定胆投注策略",ret.DynamicPosition)
            };
            list = list.Where(x => x.Item3 != null && (x.Item1 == "dynamic" ? lottery.HasDynamic : true));
            if (lottery.Key == "pk10")
            {
                string[] unaviableCodes = new string[] { "three", "six", "duplicated" };
                list = list.Where(x => !unaviableCodes.Contains(x.Item1));
            }
            Dictionary<string, LotteryResult> resultDic = list.Where(x => types.Contains(x.Item1)).ToDictionary(x => x.Item2, x => x.Item3);

            if (resultDic.Values.Any())
            {
                Console.WriteLine(string.Format("{0} 最后一期分析奖号 {1}，分析期数：{2}，分析结果：", lottery.DisplayName, lotteries[lotteries.Length - 1], lotteries.Length));

                foreach (var pair in resultDic)
                {
                    Console.WriteLine(string.Format("{0}：一共 {1} 注，最大中奖次数：{2} ，最大间隔：{3}，最近间隔：{4}", pair.Key, pair.Value.BetCount, pair.Value.HitCount, pair.Value.MaxIntervalCount, pair.Value.LastIntervalCount));
                    Console.WriteLine(pair.Value.Filter);
                    Console.WriteLine(string.Format("中奖号码：{0}", string.Join(",", pair.Value.HitPositions.Select(x => Format(context.LotteryNumbers[x])).ToArray())));
                    if (pair.Key == "单式投注策略")
                    {
                        Console.WriteLine(string.Format("投注号码：{0}", string.Join(",", pair.Value.Numbers.Select(x => Format(x)).ToArray())));
                    }
                }
            }

            return resultDic.Values.Any();
        }

        private string[] GetLotteries()
        {
            string mainKey = lottery.Key.Split('|')[0];
            string url = string.Concat("http://data.917500.cn/", mainKey, ".txt");
            HttpClient client = new HttpClient();
            string content = client.GetStringAsync(url).Result;

            string ret = null;
            string[] lotteries = new Regex(lottery.RegexPattern).Matches(content).OfType<Match>().Select(x =>
            {
                if (lottery.Key == "pk10")
                {
                    ret = string.Join(string.Empty, x.Value.Split(' ').Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t == "10" ? "0" : t.Replace("0", string.Empty)));
                }
                else
                {
                    ret = x.Value.Replace(" ", string.Empty);
                }
                return ret.Substring(lottery.StartIndex, lottery.Length);
            }).ToArray();
            return lotteries.Skip(lotteries.Length - TakeNumber).ToArray();
        }

        private void Save(Dictionary<string, LotteryResult> results, string name)
        {
            string path = @"..\..\Result\" + name;
            var query = results.Where(x => x.Value != null).Select(ret =>
            {
                string numbers = string.Join(",", ret.Value.Numbers.Select(x => Format(x)).ToArray());
                return new { name = ret.Key, filter = ret.Value.Filter, numbers = numbers };
            }).ToArray();

            string str = JsonConvert.SerializeObject(query);
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.Write(str);
                sw.Flush();
                sw.Close();
            }
        }

        private string Format(LotteryNumber number)
        {
            return lottery.Key == "pk10" ? string.Join(" ", new int[] { number.Hundred, number.Decade, number.Unit }.Select(x => x == 0 ? "10" : x.ToString("D2")).ToArray()) : number.Key;
        }

        private bool CanRun()
        {
            DateTime[][] times = lottery.TradingHours.Select(x => x.Split('-').Select(t => DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " " + t)).ToArray()).ToArray();
            return times.Any(x => DateTime.Now >= x[0] && DateTime.Now <= x[1]);
        }
    }
}
