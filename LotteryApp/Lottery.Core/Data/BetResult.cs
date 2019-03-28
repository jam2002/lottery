namespace Lottery.Core.Data
{
    public class BetResult
    {
        public string Key { get; set; }

        public string GroupName { get; set; }

        public int Status { get; set; }

        public string Description { get; set; }

        public string Value { get; set; }

        public SimpleBet Bet { get; set; }
    }
}
