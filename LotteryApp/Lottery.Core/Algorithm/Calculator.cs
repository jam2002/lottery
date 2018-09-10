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
        static HttpClient _httpClient;

        static Calculator()
        {
            InitClient();
            _httpClient.SendAsync(new HttpRequestMessage { Method = new HttpMethod("HEAD"), RequestUri = new Uri("http://tx-ssc.com") }).Result.EnsureSuccessStatusCode();
        }

        static void InitClient()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = new TimeSpan(0, 0, 10);
            _httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
        }

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
                Output = result
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
                else if (lottery.Source == 2)
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
                else
                {
                    try
                    {
                        string content = _httpClient.GetStringAsync("http://tx-ssc.com/api/getData").Result;
                        lotteries = JArray.Parse(content).Select(t => new { no = t.Value<string>("issue"), value = string.Join(string.Empty, t.Value<string>("code").Split(',')) }).OrderBy(t => t.no).Select(t => t.value).ToArray();
                    }
                    catch
                    {
                        try
                        {
                            InitClient();
                            string content = _httpClient.GetStringAsync("http://tx-ssc.com/api/getData").Result;
                            lotteries = JArray.Parse(content).Select(t => new { no = t.Value<string>("issue"), value = string.Join(string.Empty, t.Value<string>("code").Split(',')) }).OrderBy(t => t.no).Select(t => t.value).ToArray();
                        }
                        catch
                        {
                            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} 获取开奖数据异常！");
                        }
                    }
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
    }
}
