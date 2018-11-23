using Lottery.Core.Algorithm;
using Lottery.Core.Data;
using System;

namespace Lottery.Core.Plan
{
    public interface IPlan
    {
        SimpleBet LastBet { get; set; }

        string LotteryName { get; set; }

        string GameName { get; set; }

        string GameArgs { get; set; }

        int BetIndex { get; set; }

        int BetCycle { get; set; }

        int Number { get; set; }

        int? TakeNumber { get; set; }

        Action<string, string> Dispatcher { get; set; }

        bool EnableSinglePattern { get; set; }

        bool EnableContinuous { get; set; }

        bool UseGeneralTrend { get; set; }

        bool RespectRepeat { get; set; }

        int TupleLength { get; set; }

        void Invoke(SimpleBet currentBet);

        string GetKey();

        int[] GetBetAwards(OutputResult output);
    }
}
