using System;

namespace Lottery.Core.Data
{
    public class SimpleBetParameters
    {
        public SimpleBet LastBet { get; set; }

        public int BetIndex { get; set; }

        public Action<string, string> Dispatcher { get; set; }
    }
}
