using HtmlAgilityPack;
using LotteryApp.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace LotteryApp.Algorithm
{
    public class Calculator
    {
        Lottery lottery;
        LotteryMetaConfig config;
        string type;
        string algorithmArgs;
        static Dictionary<string, string[]> lotteryCache = new Dictionary<string, string[]> { };

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

        public static void ClearCache()
        {
            lotteryCache.Clear();
        }

        public bool Start()
        {
            string[] lotteries = GetLotteries();
            LotteryNumber[] selectedLottery = null;
            if (lottery.Length >= 5)
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
            list = list.Where(x => x.Item3 != null && (x.Item1 == "dynamic" ? lottery.HasDynamic : true) && (x.Item1 == "three" ? lottery.HasPair : true));
            if (lottery.Key == "pk10")
            {
                string[] unaviableCodes = new string[] { "three", "six", "duplicated" };
                list = list.Where(x => !unaviableCodes.Contains(x.Item1));
            }
            Dictionary<string, LotteryResult> resultDic = list.Where(x => types.Contains(x.Item1)).ToDictionary(x => x.Item2, x => x.Item3);
            Console.WriteLine(string.Format("{0} 最后一期分析奖号 {1}，分析期数：{2}，分析结果：", lottery.DisplayName, lotteries[lotteries.Length - 1], lotteries.Length));
            Console.WriteLine();

            if (resultDic.Values.Any())
            {
                foreach (var pair in resultDic)
                {
                    Console.WriteLine(string.Format("{0}：一共 {1} 注，最大中奖次数：{2} ，最大间隔：{3}，最近间隔：{4}", pair.Key, pair.Value.BetCount, pair.Value.HitCount, pair.Value.MaxInterval, pair.Value.LastInterval));
                    Console.WriteLine(string.Format("间隔列表：{0}", string.Join(",", pair.Value.HitIntervals)));
                    Console.WriteLine(pair.Value.Filter);
                    Console.WriteLine(string.Format("中奖号码：{0}", string.Join(",", pair.Value.HitPositions.Select(x => Format(context.LotteryNumbers[x])).ToArray())));
                    if (pair.Key == "单式投注策略")
                    {
                        Console.WriteLine(string.Format("投注号码：{0}", string.Join(",", pair.Value.Numbers.Select(x => Format(x)).ToArray())));
                    }
                }
            }

            if (types.Contains("fivestar") && ret.FiveStar != null && ret.FiveStar.Any())
            {
                LotteryResult formRet = null;
                Dictionary<FiveStarFormEnum, string> forms = GetEnumDescriptions<FiveStarFormEnum>();
                foreach (var p in forms)
                {
                    if (ret.FiveStar.ContainsKey(p.Key))
                    {
                        formRet = ret.FiveStar[p.Key];
                        Console.WriteLine(string.Format("{0}：最大中奖次数：{1} ，最大间隔：{2}，最近间隔：{3}", p.Value, formRet.HitCount, formRet.MaxInterval, formRet.LastInterval));
                        Console.WriteLine(string.Format("间隔列表：{0}", string.Join(",", formRet.HitIntervals)));
                        Console.WriteLine();
                    }
                }
            }

            if (types.Contains("anytwo") && ret.AnyTwo != null && ret.AnyTwo.Any())
            {
                var q = ret.AnyTwo.OrderByDescending(t => t.Value.HitCount)
                                              .ThenByDescending(t => t.Value.MaxContinuous)
                                              .ThenByDescending(t => t.Value.LastContinuous)
                                              .ThenBy(t => t.Value.LastInterval);
                foreach (var p in q)
                {
                    Console.WriteLine(string.Format("{0}：中奖：{1}，最大连中：{2}，最近连中：{3}，最大间隔：{4}，最近间隔：{5}", p.Value.Title, p.Value.HitCount, p.Value.MaxContinuous, p.Value.LastContinuous, p.Value.MaxInterval, p.Value.LastInterval));
                    Console.WriteLine(p.Value.Filter);
                    Console.WriteLine(string.Format("中奖号码：{0}", string.Join(",", p.Value.HitPositions.Select(x => Format(context.LotteryNumbers[x])).ToArray())));
                    Console.WriteLine();
                }
            }

            return resultDic.Values.Any();
        }

        private string[] GetLotteries()
        {
            string mainKey = lottery.Key.Split('|')[0];

            string[] lotteries = null;
            if (lotteryCache.ContainsKey(mainKey))
            {
                lotteries = lotteryCache[mainKey];
            }
            else
            {
                if (lottery.Source == 1)
                {
                    string url = string.Concat("http://data.917500.cn/", mainKey, ".txt");
                    HttpClient client = new HttpClient();
                    string content = client.GetStringAsync(url).Result;
                    client.Dispose();

                    string ret = null;
                    lotteries = new Regex(lottery.RegexPattern).Matches(content).OfType<Match>().Select(x =>
                    {
                        if (lottery.Key == "pk10")
                        {
                            ret = string.Join(string.Empty, x.Value.Split(' ').Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t == "10" ? "0" : t.Replace("0", string.Empty)));
                        }
                        else
                        {
                            ret = x.Value.Replace(" ", string.Empty);
                        }
                        return ret;
                    }).ToArray();
                }
                else
                {
                    CookieContainer cookieContainer = new CookieContainer();

                    HttpWebRequest request = WebRequest.Create("https://www.caipiaokong.com/member.php?mod=logging&action=login&loginsubmit=yes&loginhash=LW3pV&inajax=1") as HttpWebRequest;
                    request.Method = "POST";
                    request.KeepAlive = false;
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.CookieContainer = cookieContainer;
                    request.Referer = "https://www.caipiaokong.com/member.php?mod=logging&action=login";
                    string postString = "formhash=edac6fc2&referer=https%3A%2F%2Fwww.caipiaokong.com%2Flottery%2Fhljssc.html&username=jam2002&password=240a865854971ac529a947c889352688&questionid=0&answer=&cookietime=2592000";
                    byte[] postData = Encoding.ASCII.GetBytes(postString);
                    request.ContentLength = postData.Length;
                    request.AllowAutoRedirect = false;
                    Stream outputStream = request.GetRequestStream();
                    outputStream.Write(postData, 0, postData.Length);
                    outputStream.Close();

                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                    Stream responseStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    string srcString = reader.ReadToEnd();

                    string key = mainKey.EndsWith("115") ? (mainKey.Substring(0, mainKey.Length - 3) + "syxw") : mainKey;
                    request = WebRequest.Create(string.Concat("https://www.caipiaokong.com/lottery/", key, ".html")) as HttpWebRequest;
                    request.Method = "GET";
                    request.KeepAlive = false;
                    request.CookieContainer = cookieContainer;

                    response = request.GetResponse() as HttpWebResponse;
                    responseStream = response.GetResponseStream();
                    reader = new StreamReader(responseStream, Encoding.UTF8);
                    srcString = reader.ReadToEnd();

                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(srcString);
                    HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//*[@id=\"wp\"]/div[3]/div[1]/div[2]/table/tbody/tr");
                    lotteries = nodes.Skip(1)
                                                 .Select(x => string.Join(string.Empty, x.Elements("td").Skip(1).Take(5).Select(t => t.FirstChild.InnerText)))
                                                 .Reverse()
                                                 .ToArray();
                }
                lotteryCache[mainKey] = lotteries;
            }
            return lotteries.Select(x => x.Substring(lottery.StartIndex, lottery.Length)).Skip(lotteries.Length - TakeNumber).ToArray();
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

        private Dictionary<T, string> GetEnumDescriptions<T>() where T : struct
        {
            Type type = typeof(T);
            var fields = type.GetFields().ToDictionary(x => x.Name, x => x);
            return Enum.GetNames(type).Select(x => new
            {
                key = (T)Enum.Parse(type, x),
                description = fields[x].GetCustomAttribute<DescriptionAttribute>().Description
            }).ToDictionary(x => x.key, x => x.description);
        }
    }
}
