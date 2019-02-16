using Newtonsoft.Json;
using System.Collections.Generic;

namespace Lottery.Core.Data
{
    [JsonObject(MemberSerialization.OptIn)]
    public class LotteryMetaConfig
    {
        [JsonProperty("lotteries")]
        public IEnumerable<Lottery> Lotteries { get; set; }

        [JsonProperty("threeNumbers")]
        public IEnumerable<LotteryNumber> ThreeNumbers { get; set; }

        [JsonProperty("twoNumbers")]
        public IEnumerable<LotteryNumber> TwoNumbers { get; set; }
    }
}
