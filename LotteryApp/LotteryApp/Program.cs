﻿using LotteryApp.Algorithm;
using System;
using System.Linq;

namespace LotteryApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Run(60, "cqssc,xjssc,tjssc", "dynamic", "2");
            //Run(30, "all", "three");
            //Run(60, "jx115,gd115,sd115,gs115", "dynamic", "34");
            //Run(30, "jx115,gd115,sd115,gs115", "dynamic", "22");

            string commands = Console.ReadLine();
            while (commands != "exit")
            {
                string[] inputArgs = commands.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToArray();
                int number = inputArgs.Length > 1 ? int.Parse(inputArgs[1]) : 60;
                string name = inputArgs.Length > 2 ? inputArgs[2] : null;
                string type = inputArgs.Length > 3 ? inputArgs[3] : null;
                string algorArgs = inputArgs.Length > 4 ? inputArgs[4] : null;
                Run(number, name, type, algorArgs);
                commands = Console.ReadLine();
            }
        }

        static void Run(int? number = null, string lotterNames = null, string type = null, string algorArgs = null)
        {
            number = number.HasValue ? number.Value : 60;
            string[] names = lotterNames != null && lotterNames != "all" ? lotterNames.Split(',') : LotteryGenerator.GetConfig().Lotteries.Select(x => x.Key).ToArray();
            if (type == null)
            {
                type = "dynamic";
            }

            Calculator.ClearCache();

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
