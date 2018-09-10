using Lottery.Core;
using Lottery.Core.Algorithm;
using Lottery.Core.Data;
using System;
using System.Linq;
using System.Threading;

namespace Lottery.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("服务正在初始化.....");
            DateTime start = DateTime.Now;

            SimpleBet lastBet = null;
            int betIndex = 1;
            Timer timer = new Timer(delegate
            {
                SimpleBet currentBet = Invoke();
                foreach (OutputResult r in currentBet.Results)
                {
                    Console.WriteLine(r.ToReadString());
                }

                Action Reset = () =>
                {
                    Console.Title = $"【{currentBet.BetAward}】";
                    lastBet = currentBet;
                    betIndex = 1;

                    UpdateInfo(currentBet.BetAward, betIndex, 2);
                };

                if (lastBet == null)
                {
                    Reset();
                    return;
                }

                bool isHit = betIndex < 5 && currentBet.LastLotteryNumber.Contains(lastBet.BetAward);
                int status = isHit ? 1 : (betIndex == 4 ? 3 : 2);
                UpdateInfo(lastBet.BetAward, status == 1 || status == 3 ? betIndex : ++betIndex, status);

                if (status == 1 || status == 3)
                {
                    Reset();
                }
                Console.WriteLine();
            }, null, start.Second < 20 ? (20 - start.Second) * 1000 : (80 - start.Second) * 1000, 60000);

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
        static void UpdateInfo(string award, int betIndex, int status)
        {
            switch (status)
            {
                case 1:
                    Console.WriteLine($"当前计划投注号：{award}，已中奖，中奖轮次：{betIndex}");
                    break;
                case 2:
                    Console.WriteLine($"当前计划投注号：{award}，轮次：{betIndex}，计划中...");
                    break;
                case 3:
                    Console.WriteLine($"当前计划投注号：{award}，已失败");
                    break;
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
