using Lottery.Core;
using Lottery.Core.Algorithm;
using System;

namespace Lottery.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //Invoke();
            Validate();
            Console.ReadLine();
        }

        static void Invoke()
        {
            InputOptions[] options = new InputOptions[]
            {
                    new InputOptions {  Number =30, LotteryName = "cqssc", GameName = "dynamic",  GameArgs = "33" },
                    new InputOptions {  Number =30, LotteryName = "cqssc", GameName = "dynamic",  GameArgs = "34" },
                    //new InputOptions {  Number =20, LotteryName = "cqssc", GameName = "dynamic",  GameArgs = "22" },
                    //new InputOptions {  Number =30, LotteryName = "cqssc|after", GameName = "groupThree" },
                    //new InputOptions {  Number =30, LotteryName = "cqssc|middle", GameName = "groupThree" },
                    //new InputOptions {  Number =30, LotteryName = "cqssc|front", GameName = "groupThree" },

                    new InputOptions {  Number =30, LotteryName = "xjssc", GameName = "dynamic",  GameArgs = "33" },
                    new InputOptions {  Number =30, LotteryName = "xjssc", GameName = "dynamic",  GameArgs = "34" },
                    //new InputOptions {  Number =20, LotteryName = "xjssc", GameName = "dynamic",  GameArgs = "22" },
                    //new InputOptions {  Number =30, LotteryName = "xjssc|after", GameName = "groupThree" },
                    //new InputOptions {  Number =30, LotteryName = "xjssc|middle", GameName = "groupThree" },
                    //new InputOptions {  Number =30, LotteryName = "xjssc|front", GameName = "groupThree" }
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
                   new InputOptions {  Number =60, LotteryName = "xjssc", GameName = "dynamic",  GameArgs = "22", RetrieveNumber =1000 }
            };
            ValidationResult r = Validator.Validate(options);
            Console.WriteLine(r.ToReadString());
        }
    }
}
