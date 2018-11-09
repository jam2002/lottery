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

        public bool EnableSinglePattern { get; set; }

        public InputOptions()
        {
            RetrieveNumber = 200;
        }
    }
}
