using Lottery.Core;
using Lottery.Core.Algorithm;
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

            Timer timer = new Timer(delegate
            {
                string ret = Invoke();
                Console.Title = $"【{ret}】";
            }, null, start.Second < 20 ? (20 - start.Second) * 1000 : (80 - start.Second) * 1000, 60000 * 4);

            Console.WriteLine("服务已运行");
            Console.ReadLine();
            timer.Dispose();
        }

        static string Invoke()
        {
            InputOptions[] options = new InputOptions[]
            {
                    new InputOptions {  Number =50, LotteryName = "tsssc", GameName = "dynamic",  GameArgs = "11" }
            };
            OutputResult[] outputs = Calculator.GetResults(options);
            foreach (OutputResult r in outputs)
            {
                Console.WriteLine(r.ToReadString());
            }
            return outputs.Any() ? outputs[0].Output[0].AnyFilters[0].Values[0].ToString() : string.Empty;
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
