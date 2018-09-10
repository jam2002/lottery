using Lottery.Core;
using Lottery.Core.Algorithm;
using Lottery.Core.Data;
using System;
using System.Linq;
using System.Net;
using System.Threading;

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
                BetIndex = 1,
                Dispatcher =(t,v) =>
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
        static string BuildInfo(string award, int betIndex, int status)
        {
            string ret = null;
            string betTime = DateTime.Now.ToString("HH:mm:ss");
            switch (status)
            {
                case 1:
                    ret = $"{betTime}，当前计划投注号：{award}，已中奖，中奖轮次：{betIndex}";
                    break;
                case 2:
                    ret = $"{betTime}，当前计划投注号：{award}，轮次：{betIndex}，计划中...";
                    break;
                case 3:
                    ret = $"{betTime}，当前计划投注号：{award}，已失败";
                    break;
            }
            return ret;
        }

        static void StartBet(object state)
        {
            SimpleBetParameters p = state as SimpleBetParameters;
            SimpleBet currentBet = Invoke();
            foreach (OutputResult r in currentBet.Results)
            {
                p.Dispatcher(r.ToReadString(), null);
            }

            Action Reset = () =>
            {
                p.LastBet = currentBet;
                p.BetIndex = 1;

                p.Dispatcher(BuildInfo(currentBet.BetAward, p.BetIndex, 2), $"【{currentBet.BetAward}】");
            };

            if (p.LastBet == null)
            {
                Reset();
                return;
            }

            bool isHit = p.BetIndex < 6 && currentBet.LastLotteryNumber.Contains(p.LastBet.BetAward);
            int status = isHit ? 1 : (p.BetIndex == 5 ? 3 : 2);
            p.Dispatcher(BuildInfo(p.LastBet.BetAward, status == 1 || status == 3 ? p.BetIndex : ++p.BetIndex, status), null);

            if (status == 1 || status == 3)
            {
                Reset();
            }
        }

        static SimpleBet Invoke()
        {
            InputOptions[] options = new InputOptions[]
            {
                 new InputOptions {  Number =50, LotteryName = "tsssc", GameName = "dynamic",  GameArgs = "11" }
            };
            OutputResult[] outputs = Calculator.GetResults(options);
            SimpleBet bet = null;
            if (outputs.Any())
            {
                bet = new SimpleBet
                {
                    LastLotteryNumber = outputs[0].LastLotteryNumber,
                    BetAward = outputs[0].Output[0].AnyFilters[0].Values[0].ToString(),
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
