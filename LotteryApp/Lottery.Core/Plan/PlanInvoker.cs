using Lottery.Core.Algorithm;
using Lottery.Core.Data;
using Quartz;
using Quartz.Impl;
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
        private IScheduler scheduler;
        private ITrigger trigger;
        private Dictionary<string, Dynamic> planDic;
        private Dictionary<string, string> valueDic;
        private List<string> currentBetKeys;
        private int currentInterval;
        public int RunCounter { get; set; }

        public static readonly PlanInvoker Current = new PlanInvoker();

        public void Init(Dictionary<string, Dynamic> plans)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "lottery.db");
            if (!File.Exists(path))
            {
                SQLiteConnection.CreateFile(path);
            }

            ServicePoint sp = ServicePointManager.FindServicePoint(new Uri("https://www.pp926.com/api/lastOpenedIssues.php"));
            sp.ConnectionLimit = 10;

            planDic = plans;
            valueDic = plans.ToDictionary(x => x.Key, x => string.Empty);
            currentBetKeys = new List<string> { };

            scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            IJobDetail job = JobBuilder.Create<SimpleJob>().WithIdentity("job1", "group1").Build();
            DateTime start = DateTime.Now;
            start = start.AddSeconds(start.Second < 10 ? (10 - start.Second) : (70 - start.Second));

            string lotteryName = planDic.First().Value.LotteryName;
            currentInterval = planDic.First().Value.GameInterval;
            if (currentInterval >=20 && currentInterval <=30)
            {
                int minute = start.Minute / 10;
                int remainder = minute % 2;
                int nextPoint = minute * 10 + (lotteryName == "xjssc" ? (remainder == 1 ? currentInterval - 10 : currentInterval) : (remainder == 0 ? currentInterval - 10 : currentInterval));
                start = start.AddMinutes(nextPoint + 5 - start.Minute);
            }

            bool isDebug = bool.Parse(ConfigurationManager.AppSettings["debug"]);
            start = isDebug || (start - DateTime.Now).Minutes >= 10 ? DateTime.Now : start;

            trigger = TriggerBuilder.Create().WithIdentity("trigger1", "group1").StartAt(start).WithSimpleSchedule(x => (currentInterval>=30 ? x.WithIntervalInSeconds(currentInterval) : x.WithIntervalInMinutes(currentInterval)).RepeatForever()).Build();
            scheduler.ScheduleJob(job, trigger);
        }

        private SimpleBet[] Invoke()
        {
            RunCounter++;
            Calculator.ClearCache();
            InputOptions[] options = planDic.Values.Select(c => new InputOptions
            {
                Number = c.Number,
                TakeNumber = c.TakeNumber,
                LotteryName = c.LotteryName,
                GroupName = c.GroupName,
                GameName = c.GameName,
                GameArgs = c.GameArgs,
                BetCycle = c.BetCycle,
                BetIndex = c.BetIndex,
                EnableSinglePattern = c.EnableSinglePattern,
                EnableContinuous = c.EnableContinuous,
                UseGeneralTrend = c.UseGeneralTrend,
                RespectRepeat = c.RespectRepeat,
                DisableRepeat = c.DisableRepeat,
                ChangeBetPerTime = c.ChangeBetPerTime,
                TupleLength = c.TupleLength,
                WaitInterval = c.WaitInterval,
                GeneralTrendInterval = c.GeneralTrendInterval,
                StartSpan = c.StartSpan,
                SpanLength = c.SpanLength,
                Rank = c.Rank,
                NumberLength = c.NumberLength,
                RunCounter = RunCounter
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
                    Dynamic plan = planDic[GetKey(c.Input)];
                    bet.BetAward = plan.GetBetAwards(c);
                }
                return bet;
            });

            return query.ToArray();
        }

        public void Close()
        {
            scheduler.Shutdown();
        }

        public void StartBet()
        {
            SimpleBet[] currentBets = Invoke();
            List<BetResult> list = new List<BetResult> { };

            foreach (SimpleBet bet in currentBets)
            {
                if (bet.Results.Any())
                {
                    string key = GetKey(bet.Results[0].Input);
                    list.Add(planDic[key].Invoke(bet));
                }
            }

            //Update(list);

            foreach (BetResult br in list)
            {
                valueDic[br.Key] = br.Value;
                planDic[br.Key].Dispatcher(br.Description, br.Value);
            }
        }

        private void Update(List<BetResult> list)
        {
            Dictionary<string, BetResult[]> successGroupDic = list.Where(c => c.Bet.Results[0].Input.Rank > 0).GroupBy(c => c.Bet.Results[0].Input.GameName).Where(c => c.Any(t => t.Status == 3 || t.Status == 1)).ToDictionary(c => c.Key, c => c.ToArray());
            foreach (var pair in successGroupDic)
            {
                BetResult[] requireChanged = pair.Value.Where(c => c.Status == 3 || c.Status == 1).OrderBy(c => c.Bet.Results[0].Input.Rank).ToArray();

                string[] newBets = requireChanged[0].Bet.Results[0].Output.Select(c => GetValue(c.AnyFilters[0].Values)).ToArray();
                string[] unChanged = pair.Value.Where(c => c.Status == 2 || c.Status == 4).Select(c => valueDic[c.Key]).ToArray();
                string[] bets = newBets.Except(unChanged).ToArray();

                for (int i = 0; i < requireChanged.Length; i++)
                {
                    BetResult br = requireChanged[i];
                    br.Bet.BetAward = GetAward(bets[i]);
                    br.Value = bets[i];

                    planDic[br.Key].LastBet = br.Bet;
                }
            }
        }

        private string GetValue(int[] betAward)
        {
            string[] ret = (from x in betAward
                            from y in betAward
                            where x != y
                            select x.ToString() + y.ToString()).ToArray();
            return $"【{string.Join(" ", ret)}】";
        }

        private int[] GetAward(string value)
        {
            return new int[] { Convert.ToInt32(value[1].ToString()), Convert.ToInt32(value[2].ToString()) };
        }

        private string GetKey(InputOptions input)
        {
            return string.Join(".", input.LotteryName, input.GameName, input.GameArgs ?? string.Empty, input.GroupName ?? string.Empty, input.EnableSinglePattern ? "S" : "C", input.RespectRepeat ? "R" : "WR", input.UseGeneralTrend ? "G" : "WG", input.ChangeBetPerTime ? "C" : "WC", input.EnableContinuous ? "E" : "WE", input.TakeNumber, input.WaitInterval, input.BetCycle, input.TupleLength, input.SpanLength, input.Rank, input.GeneralTrendInterval, input.NumberLength, input.StartSpan);
        }
    }
}
