using Lottery.Core.Data;

namespace Lottery.Core.Algorithm
{
    public class InputOptions
    {
        public int Number { get; set; }

        public int RetrieveNumber { get; set; }

        public int? SkipCount { get; set;}

        public string LotteryName { get; set; }

        public string GameName { get; set; }

        public string GameArgs { get; set; }

        public int BetCycle { get; set; }

        public int BetIndex { get; set; }

        public bool EnableSinglePattern { get; set; } 

        public bool EnableContinuous { get; set; }

        public bool UseGeneralTrend { get; set; }

        public bool RespectRepeat { get; set; }

        public bool ChangeBetPerTime { get; set; }

        public int WaitInterval { get; set; }

        public int TupleLength { get; set; }

        public int SpanLength { get; set; }

        public InputOptions()
        {
            RetrieveNumber = 200;
        }
    }
}
