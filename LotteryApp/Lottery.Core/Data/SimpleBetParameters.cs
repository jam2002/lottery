using System;

namespace Lottery.Core.Data
{
    public class SimpleBetParameters
    {
        public SimpleBet LastBet { get; set; }

        public string GameArgs { get; set; }

        public int GameNumber { get; set; }

        public int BetIndex { get; set; }

        public int BetCycle { get; set; }

        public int ContinuousFailureCount { get; set; }

        public bool ChangeBetNumberOnceHit { get; set; }

        public bool BetRepeat { get; set; }

        public Action<string, string> Dispatcher { get; set; }
    }
}
