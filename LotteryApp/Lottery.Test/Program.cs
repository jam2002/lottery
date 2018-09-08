using Lottery.Core;
using Lottery.Core.Algorithm;
using System;

namespace Lottery.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Invoke();
            //Validate();
            while (Console.ReadLine() != "exit")
            {
                Invoke();
            }
        }

        static void Invoke()
        {
            InputOptions[] options = new InputOptions[]
            {
                    new InputOptions {  Number =60, LotteryName = "cqssc", GameName = "dynamic",  GameArgs = "22" },
                    new InputOptions {  Number =30, LotteryName = "cqssc", GameName = "dynamic",  GameArgs = "22" },
                    new InputOptions {  Number =60, LotteryName = "cqssc", GameName = "dynamic",  GameArgs = "33" }

                    //new InputOptions {  Number =60, LotteryName = "xjssc", GameName = "dynamic",  GameArgs = "22" },
                    //new InputOptions {  Number =30, LotteryName = "xjssc", GameName = "dynamic",  GameArgs = "22" },
                    //new InputOptions {  Number =60, LotteryName = "xjssc", GameName = "dynamic",  GameArgs = "33" }
            };
            OutputResult[] outputs = Calculator.GetResults(options);
            foreach (OutputResult r in outputs)
            {
                Console.WriteLine(r.ToReadString());
            }

            Console.WriteLine("策略生成结束");
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
