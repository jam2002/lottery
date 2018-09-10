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
            int betIndex = 0;
            Timer timer = new Timer(delegate
            {
                SimpleBet currentBet = Invoke();
                foreach (OutputResult r in currentBet.Results)
                {
                    Console.WriteLine(r.ToReadString());
                }

                if (lastBet == null)
                {
                    lastBet = currentBet;
                }

                bool isHit = currentBet.LastLotteryNumber.Contains(lastBet.BetAward);
                if (betIndex < 5 && betIndex > 0 && isHit)
                {
                    Console.WriteLine($"当前计划投注号：{lastBet.BetAward}，已中奖，中奖轮次：{betIndex}");

                    Console.Title = $"【{currentBet.BetAward}】";
                    lastBet = currentBet;
                    betIndex = 0;

                    Console.WriteLine($"当前计划投注号：{currentBet.BetAward}，轮次：{betIndex + 1}，计划中...");
                }
                else
                {
                    if (betIndex < 4)
                    {
                        Console.Title = $"【{lastBet.BetAward}】";
                        betIndex++;
                        Console.WriteLine($"当前计划投注号：{lastBet.BetAward}，轮次：{betIndex}，计划中...");
                    }
                    else
                    {
                        Console.WriteLine($"当前计划投注号：{lastBet.BetAward}，已失败");
                        Console.Title = $"【{currentBet.BetAward}】";
                        lastBet = currentBet;
                        betIndex = 0;

                        Console.WriteLine($"当前计划投注号：{currentBet.BetAward}，轮次：{betIndex + 1}，计划中...");
                    }
                }
            }, null, start.Second < 20 ? (20 - start.Second) * 1000 : (80 - start.Second) * 1000, 60000);

            Console.WriteLine("服务已运行");
            Console.ReadLine();
            timer.Dispose();
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
