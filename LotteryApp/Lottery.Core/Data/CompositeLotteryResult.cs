using System.Collections.Generic;

namespace Lottery.Core.Data
{
    public class CompositeLotteryResult
    {
        /// <summary>
        /// 组三
        /// </summary>
        public LotteryResult GroupThree { get; set; }

        /// <summary>
        /// 组六
        /// </summary>
        public LotteryResult GroupSix { get; set; }

        /// <summary>
        ///  单式
        /// </summary>
        public LotteryResult Mix { get; set; }

        /// <summary>
        /// 复式
        /// </summary>
        public LotteryResult Compound { get; set; }

        /// <summary>
        /// 定位胆
        /// </summary>
        public LotteryResult Position { get; set; }

        /// <summary>
        /// 不定胆
        /// </summary>
        public LotteryResult DynamicPosition { get; set; }

        /// <summary>
        /// 直选
        /// </summary>
        public LotteryResult Duplicated { get; set; }

        /// <summary>
        /// 五星形态
        /// </summary>
        public Dictionary<FiveStarFormEnum, LotteryResult> FiveStar { get; set; }

        /// <summary>
        /// 任二直选
        /// </summary>
        public LotteryResult AnyTwo { get; set; }
    }
}
