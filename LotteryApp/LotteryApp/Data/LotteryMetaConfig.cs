using Newtonsoft.Json;
using System.Collections.Generic;

namespace LotteryApp.Data
{
    [JsonObject(MemberSerialization.OptIn)]
    public class LotteryMetaConfig
    {
        [JsonProperty("lotteries")]
        public IEnumerable<Lottery> Lotteries { get; set; }

        [JsonProperty("numbers")]
        public IEnumerable<LotteryNumber> Numbers { get; set; }
    }
}
