using Lottery.Core.Data;
using Newtonsoft.Json.Linq;
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
            LotteryGenerator.TupleLength = option.TupleLength;
            LotteryGenerator.Number = option.Number;
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
            lotteries = lotteries.Skip(lotteries.Length > option.TakeNumber? lotteries.Length - option.TakeNumber :0).Take(option.TakeNumber).ToArray();

            LotteryNumber[] selectedLottery = null;
            if (lottery.Length >= 5)
            {
                selectedLottery = LotteryGenerator.GetNumbers(lotteries);
            }
            else
            {
                Dictionary<string, LotteryNumber> lotteryDic = lottery.Length == 3 ? config.ThreeNumbers.ToDictionary(x => x.Key, x => x) : config.TwoNumbers.ToDictionary(x => x.Key, x => x);
                selectedLottery = lotteries.Select(x => lotteryDic[x]).ToArray();
            }

            LotteryContext context = new LotteryContext(config, selectedLottery, option);
            LotteryResult[] result = context.GetGameResult();

            OutputResult ret = new OutputResult
            {
                DisplayName = lottery.DisplayName,
                LastLotteryNumber = lotteries[lotteries.Length - 1],
                Number = option.TakeNumber,
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
                lotteries = lotteryCache[mainKey].Select(x => lottery.IndexKeys?.Any() == true ? string.Join(string.Empty, lottery.IndexKeys.Select(t => x[t])) : x.Substring(lottery.StartIndex, lottery.Length)).ToArray();
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
                else if (lottery.Source == 4)
                {
                    lotteries = GetTsNumbersV2(lottery.Key);
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

        private string[] GetTsNumbers(string key)
        {
            int type = 16;
            switch(key)
            {
                case "qqssc":
                type = 15;
                break;
                case "tsssc":
                type = 16;
                break;
                case "mdssc":
                type = 18;
                break;
                case "djssc":
                type = 12;
                break;
            }
            string[] lotteries;
            string param = $"id={type}&pnum=50";
            byte[] bs = Encoding.UTF8.GetBytes(param);
            string url = "http://pay5.hbcchy.com/lotterytrend/getsscchart";
            HttpWebRequest webRequest = WebRequest.CreateHttp(url);
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

                    JArray array = JObject.Parse(content).SelectToken("data") as JArray;
                    lotteries = array.Select(t =>
                    {
                        JArray c = t as JArray;
                        return c.ElementAt(1).Value<string>();
                    }).ToArray();
                }
            }
            return lotteries;
        }

        private string[] GetTsNumbersV2(string key)
        {
            string[] lotteries;

            string url = "http://www.sygyy.com/plans/kj.php?type=kj";
            HttpWebRequest webRequest = WebRequest.CreateHttp(url);
            webRequest.Method = "GET";
            webRequest.ContentType = "text/html; charset=UTF-8";
            webRequest.Timeout = 50000;
            

            using (HttpWebResponse response = webRequest.GetResponse() as HttpWebResponse)
            {
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    string content = sr.ReadToEnd();

                    JArray array = JObject.Parse(content).SelectToken("result.data") as JArray;
                    lotteries = array.Select(t =>
                    {
                        JObject c = t as JObject;
                        return c.SelectToken("preDrawCode").ToString().Replace(",", string.Empty);
                    }).Reverse().ToArray();
                }
            }
            return lotteries;
        }
    }
}
