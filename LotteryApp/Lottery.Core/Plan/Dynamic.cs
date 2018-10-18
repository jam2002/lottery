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

        public int BetCycle { get; set; }

        public int Number { get; set; }

        public int? TakeNumber { get; set; }

        public string GameName { get; set; }

        public string GameArgs { get; set; }

        public Action<string, string> Dispatcher { get; set; }

        public abstract string GetBetString(SimpleBet currentBet);

        public string GetKey()
        {
            return string.Concat(LotteryName, ".", GameName, ".", GameArgs ?? string.Empty);
        }

        public void Invoke(SimpleBet currentBet)
        {
            foreach (var p in currentBet.Results)
            {
                Dispatcher(p.ToReadString(), null);
            }

            Action<int> Reset = (s) =>
            {
                bool changed = currentBet.BetAward.Any() && (BetIndex == 0 || s == 3 || s == 1);
                if (changed)
                {
                    if (s != 1 || ChangeBetOnceSuccess)
                    {
                        LastBet = currentBet;
                    }
                    BetIndex = 1;
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
            if (BetIndex > 0)
            {
                string bet = GetChangedBetString(currentBet, status);
                Dispatcher(BuildInfo(LastBet.BetAward, status == 1 || status == 3 ? BetIndex : ++BetIndex, status), bet);
            }

            if (status == 1 || status == 3)
            {
                Reset(status);
            }
        }

        public virtual bool IsHit(SimpleBet currentBet)
        {
            int[] current = currentBet.LastLotteryNumber.Select(t => int.Parse(t.ToString())).ToArray();
            int[][] betValues = new int[][] { LastBet.BetAward };
            bool isHit = BetIndex > 0 && BetIndex <= BetCycle && betValues.Any(t => t.Intersect(current).Count() >= Number);
            return isHit;
        }

        public virtual string GetChangedBetString(SimpleBet currentBet, int status)
        {
            return null;
        }

        public virtual bool ChangeBetOnceSuccess => true;

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
                    ret = $"{betTime}，当前计划投注号：{betAwards}，已中奖，中奖轮次：{betIndex}";
                    break;
                case 2:
                    ret = $"{betTime}，当前计划投注号：{betAwards}，轮次：{betIndex}，计划中...";
                    break;
                case 3:
                    ret = $"{betTime}，当前计划投注号：{betAwards}，已失败";
                    break;
                case 4:
                    ret = $"{betTime}，当前计划没有投注号，等待中";
                    break;
            }
            return ret;
        }
    }
}
