using LotteryApp.Algorithm;
using System;
using System.Linq;

namespace LotteryApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Run(30, "all", "three");
            Run(30, "all", "dynamic");

            string commands = Console.ReadLine();
            while (commands != "exit")
            {
                string[] inputArgs = commands.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToArray();
                int number = inputArgs.Length > 1 ? int.Parse(inputArgs[1]) : 30;
                string name = inputArgs.Length > 2 ? inputArgs[2] : null;
                string type = inputArgs.Length > 3 ? inputArgs[3] : null;
                string algorArgs = inputArgs.Length > 4 ? inputArgs[4] : null;
                Run(number, name, type, algorArgs);
                commands = Console.ReadLine();
            }
        }

        static void Run(int? number = null, string lotterNames = null, string type = null, string algorArgs = null)
        {
            number = number.HasValue ? number.Value : 30;
            string[] names = lotterNames != null && lotterNames != "all" ? lotterNames.Split(',') : LotteryGenerator.GetConfig().Lotteries.Select(x => x.Key).ToArray();
            if (type == null)
            {
                type = "three,dynamic";
            }

            foreach (string name in names)
            {
                Calculator calclator = new Calculator(name, type, number.Value, algorArgs);
                bool successStarted = calclator.Start();

                if (successStarted)
                {
                    Console.WriteLine();
                }
            }
            Console.WriteLine("策略生成结束");
            Console.WriteLine();
        }
    }
}
