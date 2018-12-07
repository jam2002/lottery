using Lottery.Core.Algorithm;
using Lottery.Core.Data;
using System;
using System.Linq;

namespace Lottery.Core.Plan
{
    public abstract class Dynamic : IPlan
    {
        public SimpleBet LastBet { get; set; }

        public string LotteryName { get; set; }

        public int BetIndex { get; set; }

        public int Number { get; set; }

        public int FailureCount { get; set; }

        public int SuccessCount { get; set; }

        public int TakeNumber { get; set; }

        public string GameName { get; set; }

        public string GameArgs { get; set; }

        public string Title { get; set; }

        public int GameInterval { get; set; }

        public Action<string, string> Dispatcher { get; set; }

        public abstract string GetBetString(SimpleBet currentBet);

        public string GetKey()
        {
            return string.Join(".", LotteryName, GameName, GameArgs ?? string.Empty, EnableSinglePattern ? "Single" : "Composite", RespectRepeat ? "RespectRepeat" : "WithouRespectRepeat", UseGeneralTrend ? "UseGeneralTrend" : "WithouUseGeneralTrend", TakeNumber, WaitInterval, BetCycle);
        }

        public bool EnableSinglePattern { get; set; }
        public bool EnableContinuous { get; set; }
        public bool UseGeneralTrend { get; set; }
        public bool RespectRepeat { get; set; }
        public bool ChangeBetPerTime { get; set; }
        public int BetCycle { get; set; }
        public int TupleLength { get; set; }
        public int WaitInterval { get; set; }
        public int SpanLength { get; set; }

        public void Invoke(SimpleBet currentBet)
        {
            foreach (var p in currentBet.Results)
            {
                Dispatcher(p.ToReadString(), null);
            }

            Action<int> Reset = (s) =>
            {
                bool changed = currentBet.BetAward.Any();
                if (changed)
                {
                    switch (s)
                    {
                        case 1:
                        case 3:
                        case 4:
                            if (s == 1 || s == 3)
                            {
                                Dispatcher(BuildInfo(LastBet.BetAward, BetIndex, s), null);
                            }
                            LastBet = currentBet;
                            BetIndex = 1;
                            break;
                        case 2:
                            BetIndex++;
                            if (ChangeBetPerTime)
                            {
                                LastBet = currentBet;
                            }
                            break;
                    }
                    Dispatcher(BuildInfo(LastBet.BetAward, BetIndex, 2), GetBetString(LastBet));
                }
                else
                {
                    int[] betAwards = LastBet?.BetAward ?? new int[] { };
                    BetIndex = 0;
                    Dispatcher(BuildInfo(betAwards, BetIndex, 4), string.Empty);
                }
            };

            if (BetIndex == 0)
            {
                Reset(4);
                return;
            }

            bool isHit = IsHit(currentBet);
            int status = isHit ? 1 : (BetIndex == BetCycle ? 3 : 2);
            Reset(status);
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
            switch (status)
            {
                case 1:
                    SuccessCount++;
                    ret = $"{betTime}，当前计划投注号：{betAwards}，失败：{FailureCount}，中奖：{SuccessCount}，已中奖，中奖轮次：{betIndex}";
                    break;
                case 2:
                    ret = $"{betTime}，当前计划投注号：{betAwards}，失败：{FailureCount}，中奖：{SuccessCount}，轮次：{betIndex}，计划中...";
                    break;
                case 3:
                    FailureCount++;
                    ret = $"{betTime}，当前计划投注号：{betAwards}，失败：{FailureCount}，中奖：{SuccessCount}，已失败";
                    break;
                case 4:
                    ret = $"{betTime}，当前计划没有投注号，失败：{FailureCount}，中奖：{SuccessCount}，等待中";
                    break;
            }
            return ret;
        }
    }
}
