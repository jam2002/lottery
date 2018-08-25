using HtmlAgilityPack;
using Lottery.Core.Data;
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

namespace Lottery.Core.Algorithm
{
    public class Calculator
    {
        Data.Lottery lottery;
        LotteryMetaConfig config;
        InputOptions option;
        static Dictionary<string, string[]> lotteryCache = new Dictionary<string, string[]> { };

        /// <summary>
        /// 支持 cq, xj, tj
        /// </summary>
        /// <param name="name"></param>
        public Calculator(InputOptions input)
        {
            config = LotteryGenerator.GetConfig();
            lottery = config.Lotteries.Where(x => x.Key == input.LotteryName).First();
            option = input;
        }

        public static OutputResult[] GetResults(InputOptions[] options)
        {
            ClearCache();

            return options.Select(t =>
            {
                Calculator c = new Calculator(t);
                return c.Start();
            }).Where(t => t.Output != null).ToArray();
        }

        public static void ClearCache()
        {
            lotteryCache.Clear();
        }

        public OutputResult Start()
        {
            string[] lotteries = GetLotteries();
            lotteries = lotteries.Skip(lotteries.Length - option.Number).ToArray();

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

            LotteryContext context = new LotteryContext(config, selectedLottery, lottery.Key, option.GameArgs);
            LotteryResult[] result = context.GetGameResult(option.GameName);

            OutputResult ret = new OutputResult
            {
                DisplayName = lottery.DisplayName,
                LastLotteryNumber = lotteries[lotteries.Length - 1],
                Number = option.Number,
                Output = result
            };
            return ret;
        }

        private string[] GetLotteries(int retrieveNumber = 200)
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
                    string url = string.Concat("http://data.917500.cn/", mainKey, retrieveNumber == 200 ? string.Empty : ("_" + retrieveNumber.ToString()), ".txt");
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
            return lotteries.Select(x => x.Substring(lottery.StartIndex, lottery.Length)).ToArray();
        }

        public void Validate()
        {
            int count = 10000;
            int skipCount = option.Number;
            int betCycle = 0;
            int failureCount = 0;
            double minAmount = 0;
            double maxAmount = 0;
            double betAmount = 0;
            string[] baseLotteries = GetLotteries(count);

            Dictionary<int, int> cycleDic = CreateCycle(2, 7);

            Dictionary<int, int> hitDic = Enumerable.Range(0, cycleDic.Count).ToDictionary(x => x, x => 0);
            LotteryResult betResult = null;

            while (skipCount < count)
            {
                betResult = GetBetResult(skipCount, baseLotteries);

                if (betResult != null)
                {
                    betCycle = 0;
                    bool ret = false;
                    string lottery = null;

                    while (skipCount < count && (betCycle < cycleDic.Count || ret))
                    {
                        double cycleAmount = 4 * cycleDic[betCycle];
                        betAmount = betAmount - cycleAmount;
                        if (betAmount < minAmount)
                        {
                            minAmount = betAmount;
                        }

                        lottery = baseLotteries[skipCount];
                        skipCount++;

                        if (ret)
                        {
                            hitDic[betCycle] = hitDic[betCycle] + 1;
                            betAmount = betAmount + cycleDic[betCycle] * 26.666;
                            if (betAmount > maxAmount)
                            {
                                maxAmount = betAmount;
                            }
                            betResult = null;
                            break;
                        }
                        betCycle++;
                    }

                    if (!ret)
                    {
                        failureCount++;
                    }
                }
                else
                {
                    skipCount++;
                }
            }

            if (betResult == null)
            {
                betResult = GetBetResult(skipCount, baseLotteries);
            }
        }

        private LotteryResult GetBetResult(int skipCount, string[] baseLotteries)
        {
            string[] lotteries = baseLotteries.Skip(skipCount - option.Number).Take(option.Number).ToArray();
            LotteryNumber[] selectedLottery = LotteryGenerator.GetNumbers(lotteries); ;
            LotteryContext context = new LotteryContext(config, selectedLottery, lottery.Key, option.GameArgs);

            LotteryResult betResult = context.GetGameResult(option.GameName).FirstOrDefault();

            return betResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">1：任二；2：不定胆；3：三星直选</param>
        /// <param name="cycleCount">计划期数</param>
        /// <returns></returns>
        private Dictionary<int, int> CreateCycle(int type, int cycleCount)
        {
            Dictionary<int, int> cycleDic = null;
            int[] counter = Enumerable.Range(0, cycleCount).ToArray();
            switch (type)
            {
                case 1:
                    counter[0] = 1;
                    counter[1] = 1;
                    for (var i = 2; i < counter.Length; i++)
                    {
                        counter[i] = counter[i - 1] + counter[i - 2];
                    }
                    break;
                case 2:
                    counter[0] = counter[1] = counter[2] = counter[3] = 1;
                    counter[4] = counter[5] = 2;
                    counter[6] = 3;
                    //counter[7] = 4;
                    //counter[8] = 5;
                    //counter[9] = 6;
                    //counter[10] = 8;
                    //counter[11] = 10;
                    break;
            }

            cycleDic = counter.Select((x, i) => new { key = i, value = x }).ToDictionary(x => x.key, x => x.value);

            return cycleDic;
        }

        private string Format(LotteryNumber number)
        {
            return lottery.Key == "pk10" ? string.Join(" ", new int[] { number.Hundred, number.Decade, number.Unit }.Select(x => x == 0 ? "10" : x.ToString("D2")).ToArray()) : number.Key;
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
