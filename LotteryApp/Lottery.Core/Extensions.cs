using Lottery.Core.Algorithm;
using Lottery.Core.Data;
using System.Linq;
using System.Text;

namespace Lottery.Core
{
    public static class Extensions
    {
        public static string ToReadString(this OutputResult output, bool isHtml = false)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"{output.DisplayName} 最后一期分析奖号 {output.LastLotteryNumber}，分析期数：{output.Number}，分析结果：");
            if (isHtml)
            {
                builder.AppendLine("<br/>");
            }

            foreach (LotteryResult r in output.Output)
            {
                builder.AppendLine($"{r.Filter}：最大中奖次数：{ r.HitCount} ，最大间隔：{r.MaxInterval}，最近间隔：{r.LastInterval}，间隔列表：{string.Join(",", r.HitIntervals)}");
                if (isHtml)
                {
                    builder.AppendLine("<br/>");
                }
            }

            return builder.ToString();
        }

        public static string ToReadString(this ValidationResult valiation, bool isHtml = false)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"最后分析奖号：{valiation.LastLotteryNumber}， 最后投注策略：{valiation.BetResult.LotteryName} - {valiation.BetResult.Filter}：最大中奖次数：{ valiation.BetResult.HitCount} ，最大间隔：{valiation.BetResult.MaxInterval}，最近间隔：{valiation.BetResult.LastInterval}，间隔列表：{string.Join(",", valiation.BetResult.HitIntervals)}");
            if (isHtml)
            {
                builder.AppendLine("<br/>");
            }
            builder.AppendLine($"当前资金：{valiation.Amount}，最低：{valiation.MinAmount}，最高：{valiation.MaxAmount}, 四飞次数：{valiation.HitAllNumber}, 中奖统计：{string.Join(",", valiation.HitDic.Select(t => string.Concat(t.Key, "=", t.Value)))}");

            return builder.ToString();
        }

        public static LotteryResult ToResult(this ReferenceFactor factor)
        {
            return new LotteryResult
            {
                MaxInterval = factor.MaxInterval,
                LastInterval = factor.LastInterval,
                HitPositions = factor.OccurPositions,
                HitIntervals = factor.HitIntervals,
                HitCount = factor.OccurPositions.Length
            };
        }
    }
}
