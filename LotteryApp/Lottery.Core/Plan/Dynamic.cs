using Lottery.Core.Algorithm;
using Lottery.Core.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Lottery.Core.Plan
{
    public abstract class Dynamic
    {
        public SimpleBet LastBet { get; set; }

        public string LotteryName { get; set; }

        public int BetIndex { get; set; }

        public int Number { get; set; }

        public int FailureCount = 0;

        public int SuccessCount = 0;

        public int TakeNumber { get; set; }

        public string GroupName { get; set; }

        public string GameName { get; set; }

        public string GameArgs { get; set; }

        public string Title { get; set; }

        public int GameInterval { get; set; }

        public Action<string, string> Dispatcher { get; set; }

        public abstract string GetBetString(SimpleBet currentBet);

        public string GetKey()
        {
            return string.Join(".", LotteryName, GameName, GameArgs ?? string.Empty, EnableSinglePattern ? "S" : "C", RespectRepeat ? "R" : "WR", UseGeneralTrend ? "G" : "WG", ChangeBetPerTime ? "C" : "WC", EnableContinuous ? "E" : "WE", TakeNumber, WaitInterval, BetCycle, TupleLength, SpanLength, Rank, GeneralTrendInterval, NumberLength, StartSpan);
        }

        public bool EnableSinglePattern { get; set; }
        public bool EnableContinuous { get; set; }
        public bool UseGeneralTrend { get; set; }
        public bool RespectRepeat { get; set; }
        public bool DisableRepeat { get; set; }
        public bool ChangeBetPerTime { get; set; }
        public int BetCycle { get; set; }
        public int TupleLength { get; set; }
        public int WaitInterval { get; set; }
        public int GeneralTrendInterval { get; set; }

        public int StartSpan { get; set; }
        public int SpanLength { get; set; }
        public int Rank { get; set; }
        public int NumberLength { get; set; }

        private Dictionary<int, int> betCounters;
        protected bool isDistinct;

        public event PropertyChangedEventHandler PropertyChanged;
        protected internal virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private BetResult Reset(int s, SimpleBet currentBet)
        {
			BetResult ret = new BetResult { Status = s, Key = GetKey(), GroupName = GroupName };
            bool hasBet = currentBet.BetAward.Any();
            switch (s)
            {
                case 1:
                case 3:
                case 4:
                    if (s == 1 || s == 3)
                    {
                        ret.Description = BuildInfo(LastBet.BetAward, BetIndex, s);
                    }

                    if (hasBet)
                    {
                        LastBet = currentBet;
                        BetIndex = 1;
                        ret.Description = BuildInfo(LastBet.BetAward, BetIndex, 2);
                        ret.Value = GetBetString(LastBet);
                    }
                    else
                    {
                        int[] betAwards = LastBet?.BetAward ?? new int[] { };
                        BetIndex = 0;
                        ret.Description = BuildInfo(betAwards, BetIndex, 4);
                        ret.Value = string.Empty;
                    }
                    break;
                case 2:
                    BetIndex++;
                    if (ChangeBetPerTime)
                    {
                        LastBet = currentBet;
                    }
                    ret.Description = BuildInfo(LastBet.BetAward, BetIndex, 2);
                    ret.Value = GetBetString(LastBet);
                    break;
            }
			ret.Bet = LastBet;
            return ret;
        }

        public BetResult Invoke(SimpleBet currentBet)
        {
            if (betCounters == null)
            {
                betCounters = Enumerable.Range(1, BetCycle).ToDictionary(c => c, c => 0);
            }
            foreach (var p in currentBet.Results)
            {
                Dispatcher(p.ToReadString(), null);
            }

            if (BetIndex == 0)
            {
                return Reset(4, currentBet);
            }

            bool isHit = IsHit(currentBet);
            int status = isHit ? 1 : (BetIndex == BetCycle ? 3 : 2);
            if (status == 1)
            {
                betCounters[BetIndex] = betCounters[BetIndex] + 1;
                SuccessCount++;
            }
            if (status == 3)
            {
                FailureCount++;
            }
            return Reset(status, currentBet);
        }

        public virtual bool IsHit(SimpleBet currentBet)
        {
            int[] current = currentBet.LastLotteryNumber.Select(t => int.Parse(t.ToString())).ToArray();
            bool isHit = BetIndex > 0 && BetIndex <= BetCycle && LastBet.BetAward.Intersect(current).Count() >= Number;
            return isHit;
        }

        public virtual string GetChangedBetString(SimpleBet currentBet, int status)
        {
            return null;
        }

        public virtual int[] GetBetAwards(OutputResult output)
        {
            return output.Output[0].AnyFilters.SelectMany(t => t.Values).Distinct().ToArray();
        }

        /// <summary>
        /// 1: 已中奖；2：计划中；3：已失败；4：等待中
        /// </summary>
        /// <param name="award"></param>
        /// <param name="betIndex"></param>
        /// <param name="status"></param>
        protected string BuildInfo(int[] award, int betIndex, int status)
        {
            string ret = null;
            string betTime = DateTime.Now.ToString("HH:mm:ss");
            string betAwards = string.Join(",", award);
            string hitCounter = string.Join(",", betCounters.Select(c => $"{c.Key}={c.Value}"));
            switch (status)
            {
                case 1:
                    ret = $"{betTime}，投注：{betAwards}，{hitCounter}，失败：{FailureCount}，中奖：{SuccessCount}，轮次：{betIndex}，已中奖";
                    break;
                case 2:
                    ret = $"{betTime}，投注：{betAwards}，{hitCounter}，失败：{FailureCount}，中奖：{SuccessCount}，轮次：{betIndex}，计划中...";
                    break;
                case 3:
                    ret = $"{betTime}，投注：{betAwards}，{hitCounter}，失败：{FailureCount}，中奖：{SuccessCount}，已失败";
                    break;
                case 4:
                    ret = $"{betTime}，没有投注，{hitCounter}，失败：{FailureCount}，中奖：{SuccessCount}，等待中";
                    break;
            }
            return ret;
        }
    }
}
