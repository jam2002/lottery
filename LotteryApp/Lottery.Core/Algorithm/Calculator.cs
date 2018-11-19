using HtmlAgilityPack;
using Lottery.Core.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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

        public static OutputResult[] GetResults(InputOptions[] options, bool clearCache = true)
        {
            if (clearCache)
            {
                ClearCache();
            }

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

        public static Dictionary<string, string[]> GetCache()
        {
            return lotteryCache;
        }

        public OutputResult Start()
        {
            string[] lotteries = GetLotteries();
            lotteries = lotteries.Skip((option.SkipCount.HasValue ? option.SkipCount.Value : lotteries.Length) - option.Number).Take(option.Number).ToArray();

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

            LotteryContext context = new LotteryContext(config, selectedLottery, option);
            LotteryResult[] result = context.GetGameResult();

            OutputResult ret = new OutputResult
            {
                DisplayName = lottery.DisplayName,
                LastLotteryNumber = lotteries[lotteries.Length - 1],
                Number = option.Number,
                Output = result,
                Input = option
            };
            return ret;
        }

        private string[] GetLotteries()
        {
            string mainKey = lottery.Key.Split('|')[0];

            string[] lotteries = null;
            if (lotteryCache.ContainsKey(lottery.Key))
            {
                lotteries = lotteryCache[lottery.Key];
            }
            else if (lotteryCache.ContainsKey(mainKey))
            {
                lotteries = lotteryCache[mainKey].Select(x => x.Substring(lottery.StartIndex, lottery.Length)).ToArray();
                lotteryCache[lottery.Key] = lotteries;
            }
            else
            {
                if (lottery.Source == 1)
                {
                    string url = string.Concat("http://data.917500.cn/", mainKey, option.RetrieveNumber == 200 ? string.Empty : ("_" + option.RetrieveNumber.ToString()), ".txt");
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
                else if (lottery.Source == 3)
                {
                    try
                    {
                        //lotteries = GetTsNumbers("http://tx-ssc.com/api/getData");
                        lotteries = GetTsNumbers("https://www.150106.com/api/lastOpenedIssues.php?id=1&issueCount=100");
                    }
                    catch
                    {
                        lotteries = GetTsNumbers("https://www.pp926.com/api/lastOpenedIssues.php?id=1&issueCount=100");
                    }
                }
                else if (lottery.Source == 4)
                {
                    lotteries = GetMdNumbers();
                }

                lotteryCache[mainKey] = lotteries;
                if (mainKey != lottery.Key)
                {
                    lotteryCache[lottery.Key] = lotteries.Select(x => x.Substring(lottery.StartIndex, lottery.Length)).ToArray();
                    lotteries = lotteryCache[lottery.Key];
                }
            }
            return lotteries;
        }

        private string[] GetTsNumbers(string apiPath)
        {
            string[] lotteries;
            bool isOffical = apiPath.StartsWith("http://tx-ssc.com/api");

            HttpWebRequest webRequest = WebRequest.CreateHttp(apiPath);
            webRequest.Timeout = 5000;
            using (HttpWebResponse response = webRequest.GetResponse() as HttpWebResponse)
            {
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    string content = sr.ReadToEnd();
                    if (isOffical)
                    {
                        lotteries = JArray.Parse(content).Select(t => new { no = t.Value<string>("issue"), value = string.Join(string.Empty, t.Value<string>("code").Split(',')) }).OrderBy(t => t.no).Select(t => t.value).ToArray();
                    }
                    else
                    {
                        lotteries = JObject.Parse(content).Value<string>("result").Split(',').Select(t => new { no = t.Split('|')[0], value = t.Split('|')[1] }).OrderBy(t => t.no).Select(t => t.value).ToArray();
                    }
                }
            }
            return lotteries;
        }

        private string[] GetMdNumbers()
        {
            string[] lotteries;
            string param = "id=18&pnum=50";
            byte[] bs = Encoding.UTF8.GetBytes(param);
            HttpWebRequest webRequest = WebRequest.CreateHttp("http://wdpay.9vcpp.cn/lotterytrend/getsscchart");
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            webRequest.ContentLength = bs.Length;
            using (Stream reqStream = webRequest.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
            }

            webRequest.Timeout = 5000;
            using (HttpWebResponse response = webRequest.GetResponse() as HttpWebResponse)
            {
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    string content = sr.ReadToEnd();
                    lotteries = JObject.Parse(content)["data"].Select(c => ((JArray)c)[1].ToString()).ToArray();
                }
            }

            return lotteries;
        }
    }
}
