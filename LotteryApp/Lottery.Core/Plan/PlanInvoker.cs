using Hangfire;
using Hangfire.SQLite;
using Lottery.Core.Algorithm;
using Lottery.Core.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net;

namespace Lottery.Core.Plan
{
    public class PlanInvoker
    {
        private Dictionary<string, IPlan> planDic;
        private int takeNumber;

        public PlanInvoker(Dictionary<string, IPlan> plans)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "lottery.db");
            if (!File.Exists(path))
            {
                SQLiteConnection.CreateFile(path);
            }

            ServicePoint sp = ServicePointManager.FindServicePoint(new Uri("https://www.pp926.com/api/lastOpenedIssues.php"));
            sp.ConnectionLimit = 10;

            planDic = plans;
            takeNumber = int.Parse(ConfigurationManager.AppSettings["GameNumber"]);

            GlobalConfiguration.Configuration.UseSQLiteStorage("sqlite");
            BackgroundJobServer server = new BackgroundJobServer();
            RecurringJob.AddOrUpdate(() => StartBet(), "15 * * * *");

            //DateTime start = DateTime.Now;
            //timer = new Timer(StartBet, null, start.Second < 15 ? (15 - start.Second) * 1000 : (75 - start.Second) * 1000, int.Parse(ConfigurationManager.AppSettings["GameInterval"]));
        }

        private SimpleBet[] Invoke()
        {
            Calculator.ClearCache();
            InputOptions[] options = planDic.Values.Select(c => new InputOptions
            {
                Number = c.TakeNumber ?? takeNumber,
                LotteryName = c.LotteryName,
                GameName = c.GameName,
                GameArgs = c.GameArgs,
                BetCycle = c.BetCycle
            }).ToArray();
            OutputResult[] outputs = Calculator.GetResults(options, false);

            var query = outputs.Select(c =>
            {
                SimpleBet bet = new SimpleBet
                {
                    LastLotteryNumber = c.LastLotteryNumber,
                    BetAward = new int[] { },
                    Results = new[] { c }
                };
                if (c?.Output.Any() == true)
                {
                    IPlan plan = planDic[GetKey(c.Input)];
                    bet.BetAward = plan.GetBetAwards(c);
                }
                return bet;
            });

            return query.ToArray();
        }

        public void StartBet()
        {
            SimpleBet[] currentBets = Invoke();

            foreach (SimpleBet bet in currentBets)
            {
                if (bet.Results.Any())
                {
                    string key = GetKey(bet.Results[0].Input);
                    IPlan plan = planDic[key];
                    plan.Invoke(bet);
                }
            }
        }

        private string GetKey(InputOptions input)
        {
            return string.Concat(input.LotteryName, ".", input.GameName, ".", input.GameArgs ?? string.Empty);
        }
    }
}
