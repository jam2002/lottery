using Lottery.Core;
using Lottery.Core.Algorithm;
using Lottery.Core.Data;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Configuration;

namespace Lottery.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var sp = ServicePointManager.FindServicePoint(new Uri("http://tx-ssc.com"));
            sp.ConnectionLimit = 10;

            Console.WriteLine("服务正在初始化.....");

            DateTime start = DateTime.Now;
            SimpleBetParameters p = new SimpleBetParameters
            {
                LastBet = null,
                BetIndex = 0,
                BetCycle = int.Parse(ConfigurationManager.AppSettings["BetCycle"]),
                BetRepeat = bool.Parse(ConfigurationManager.AppSettings["BetRepeat"]),
                ChangeBetNumberOnceHit = bool.Parse(ConfigurationManager.AppSettings["ChangeBetNumberOnceHit"]),
                GameArgs = ConfigurationManager.AppSettings["GameArgs"],
                GameNumber = int.Parse(ConfigurationManager.AppSettings["GameNumber"]),
                Dispatcher = (t, v) =>
                 {
                     Console.WriteLine(t);
                     if (v != null)
                     {
                         Console.Title = v;
                     }
                 }
            };
            Timer timer = new Timer(StartBet, p, start.Second < 20 ? (20 - start.Second) * 1000 : (80 - start.Second) * 1000, 60000);

            Console.WriteLine("服务已运行");
            Console.ReadLine();
            timer.Dispose();
        }

        /// <summary>
        /// 1: 已中奖；2：计划中；3：已失败
        /// </summary>
        /// <param name="award"></param>
        /// <param name="betIndex"></param>
        /// <param name="status"></param>
        static string BuildInfo(int[] award, int betIndex, int status)
        {
            string ret = null;
            string betTime = DateTime.Now.ToString("HH:mm:ss");
            string betAwards = string.Join(",", award);
            switch (status)
            {
                case 1:
                    ret = $"{betTime}，当前计划投注号：{betAwards}，已中奖，中奖轮次：{betIndex}";
                    break;
                case 2:
                    ret = $"{betTime}，当前计划投注号：{betAwards}，轮次：{betIndex}，计划中...";
                    break;
                case 3:
                    ret = $"{betTime}，当前计划投注号：{betAwards}，已失败";
                    break;
                case 4:
                    ret = $"{betTime}，当前计划没有投注号，等待中";
                    break;
            }
            return ret;
        }

        static void StartBet(object state)
        {
            SimpleBetParameters p = state as SimpleBetParameters;
            SimpleBet currentBet = Invoke(p);
            foreach (OutputResult r in currentBet.Results)
            {
                p.Dispatcher(r.ToReadString(), null);
            }

            int number = int.Parse(p.GameArgs[0].ToString());

            Action<int?> Reset = (s) =>
            {
                string bet = null;
                bool changed = currentBet.BetAward.Any() && (p.BetIndex == 0 || s == 3 || (p.ChangeBetNumberOnceHit && s == 1));
                if (changed)
                {
                    if (p.GameArgs == "22")
                    {
                        bet = $"【{currentBet.BetAward[0]}{currentBet.BetAward[1]} {currentBet.BetAward[0]}{currentBet.BetAward[2]} {currentBet.BetAward[1]}{currentBet.BetAward[0]} {currentBet.BetAward[2]}{currentBet.BetAward[0]}】";
                    }
                    else
                    {
                        bet = $"【{string.Join(",", currentBet.BetAward)}】";
                    }

                    p.BetIndex = 1;
                    p.LastBet = currentBet;
                    p.Dispatcher(BuildInfo(p.LastBet.BetAward, p.BetIndex, 2), bet);
                }
                else
                {
                    int[] betAwards = p.LastBet?.BetAward ?? new int[] { };
                    p.BetIndex = 0;
                    p.Dispatcher(BuildInfo(betAwards, p.BetIndex, 4), string.Empty);
                }
            };

            if (p.BetIndex == 0)
            {
                Reset(4);
                return;
            }
            int[] current = currentBet.LastLotteryNumber.Select(t => int.Parse(t.ToString())).ToArray();
            int[][] betValues = p.GameArgs == "22" ? new int[][] { new int[] { p.LastBet.BetAward[0], p.LastBet.BetAward[1] }, new int[] { p.LastBet.BetAward[0], p.LastBet.BetAward[2] } } : new int[][] { p.LastBet.BetAward };
            bool isHit = p.BetIndex > 0 && p.BetIndex <= p.BetCycle && betValues.Any(t => t.Intersect(current).Count() >= t.Length);
            int status = isHit ? 1 : (p.BetIndex == p.BetCycle ? 3 : 2);
            if (p.BetIndex > 0)
            {
                p.Dispatcher(BuildInfo(p.LastBet.BetAward, status == 1 || status == 3 ? p.BetIndex : ++p.BetIndex, status), null);
            }

            if (status == 1 || status == 3)
            {
                Reset(status);
            }
        }

        static SimpleBet Invoke(SimpleBetParameters p)
        {
            InputOptions[] options = new InputOptions[]
            {
                 new InputOptions {  Number =p.GameNumber, LotteryName = "tsssc", GameName = "dynamic",  GameArgs = p.GameArgs, BetCycle = p.BetCycle, BetRepeat = p.BetRepeat  }
            };
            OutputResult[] outputs = Calculator.GetResults(options);
            SimpleBet bet = null;
            if (outputs.Any() && outputs[0].Output.Any())
            {
                int[] awards = outputs[0].Output[0].AnyFilters.SelectMany(t => t.Values).Distinct().ToArray();
                if (p.LastBet != null && p.GameArgs == "11")
                {
                    awards = outputs.SelectMany(t => t.Output.SelectMany(c => c.AnyFilters.SelectMany(q => q.Values)).ToArray()).ToArray();
                    int current = p.LastBet.BetAward[0];
                    awards = awards.Where(c => c != current).Take(1).ToArray();
                }

                if (p.GameArgs == "22")
                {
                    int[][] values = outputs[0].Output.Select(c => c.AnyFilters[0].Values).ToArray();
                    int[] first = values[0];
                    bool found = false;
                    foreach (int[] temp in values.Skip(1))
                    {
                        if (temp.Intersect(first).Any())
                        {
                            int u = temp.Intersect(first).First();
                            first = new[] { u }.Concat(first).Concat(temp).Distinct().ToArray();
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        first = first.Concat(values[1]).ToArray();
                    }
                    awards = first;
                }

                bet = new SimpleBet
                {
                    LastLotteryNumber = outputs[0].LastLotteryNumber,
                    BetAward = awards,
                    Results = outputs
                };
            }
            return bet;
        }

        static void Validate()
        {
            InputOptions[] options = new InputOptions[]
            {
                   new InputOptions {  Number =30, LotteryName = "xjssc", GameName = "dynamic",  GameArgs = "22", RetrieveNumber =10000 }
            };
            ValidationResult r = Validator.Validate(options);
            Console.WriteLine(r.ToReadString());
        }
    }
}
