using Lottery.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lottery.Core.Algorithm
{
    public class Validator
    {
        public static ValidationResult Validate(InputOptions[] options)
        {
            int count = options[0].RetrieveNumber;
            int skipCount = options[0].Number;
            int betCycle = 0;
            int failureCount = 0;
            double minAmount = 0;
            double maxAmount = 0;
            double betAmount = 0;

            Dictionary<int, int> cycleDic = null;
            Dictionary<int, int> hitDic = Enumerable.Range(0, 9).ToDictionary(x => x, x => 0);
            LotteryResult betResult = null;
            int allCount = 0;

            Calculator.ClearCache();

            while (skipCount < count)
            {
                betResult = Calculator.GetResults(options, false).SelectMany(t => t.Output).OrderByDescending(t => t.HitCount).ThenBy(t => t.MaxInterval).ThenBy(t => t.LastInterval).FirstOrDefault();
                int cycleType = betResult.GameName.StartsWith("dynamic34") ? 2 : (betResult.GameName.StartsWith("dynamic22") ? 3 : 1);
                cycleDic = CreateCycle(cycleType, 9);

                double baseAmount = cycleType == 2 ? 22.482 : 6.666;
                double baseBetAmount = cycleType == 2 ? 4 : 1;

                if (betResult != null)
                {
                    betCycle = 0;
                    bool ret = false;
                    string lottery = null;

                    while (skipCount < count && (betCycle < cycleDic.Count || ret))
                    {
                        betAmount = betAmount - cycleDic[betCycle] * baseBetAmount;
                        if (betAmount < minAmount)
                        {
                            minAmount = betAmount;
                        }

                        lottery = Calculator.GetCache()[betResult.LotteryName][skipCount];
                        ret = betResult.AnyFilters.Any(t => t.Values.All(q => lottery.Contains(q.ToString())));
                        bool isAll = cycleType == 2 && betResult.AnyFilters.All(t => t.Values.All(q => lottery.Contains(q.ToString())));
                        skipCount++;

                        if (ret)
                        {
                            hitDic[betCycle] = hitDic[betCycle] + 1;
                            betAmount = betAmount + cycleDic[betCycle] * baseAmount * (isAll ? 4 : 1);
                            if (isAll)
                            {
                                allCount++;
                            }
                            if (betAmount > maxAmount)
                            {
                                maxAmount = betAmount;
                            }
                            Console.WriteLine($"当前资金：{betAmount}, 开奖号：{lottery},  投注品种：{betResult.LotteryName} - {betResult.Filter}：最大中奖次数：{ betResult.HitCount} ，最大间隔：{betResult.MaxInterval}，最近间隔：{betResult.LastInterval}，间隔列表：{string.Join(",", betResult.HitIntervals)}");
                            betResult = null;
                            break;
                        }
                        betCycle++;
                    }

                    if (!ret)
                    {
                        failureCount++;
                    }
                }
                else
                {
                    skipCount++;
                }

                foreach (InputOptions o in options)
                {
                    o.SkipCount = skipCount;
                }
            }

            if (betResult == null)
            {
                betResult = Calculator.GetResults(options, false).SelectMany(t => t.Output).OrderByDescending(t => t.HitCount).ThenBy(t => t.MaxInterval).ThenBy(t => t.LastInterval).FirstOrDefault();
            }

            ValidationResult validation = new ValidationResult
            {
                Amount = betAmount,
                MaxAmount = maxAmount,
                MinAmount = minAmount,
                BetResult = betResult,
                HitAllNumber = allCount,
                HitDic = hitDic,
                LastLotteryNumber = Calculator.GetCache()[betResult.LotteryName].Last()
            };
            return validation;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">1：任二；2：三码不定位；3：二码不定位</param>
        /// <param name="cycleCount">计划期数</param>
        /// <returns></returns>
        public static Dictionary<int, int> CreateCycle(int type, int cycleCount)
        {
            Dictionary<int, int> cycleDic = null;
            int[] counter = Enumerable.Range(0, cycleCount).ToArray();
            switch (type)
            {
                case 1:
                    counter[0] = 1;
                    counter[1] = 1;
                    for (var i = 2; i < counter.Length; i++)
                    {
                        counter[i] = counter[i - 1] + counter[i - 2];
                    }
                    break;
                case 2:
                    counter[0] = counter[1] = counter[2] = 1;
                    counter[3] = counter[4] = 2;
                    counter[5] = 3;
                    counter[6] = 4;
                    counter[7] = 6;
                    counter[8] = 8;
                    break;
                case 3:
                    counter[0] = counter[1] = counter[2] = 4;
                    counter[3] = 4;
                    counter[4] = 5;
                    counter[5] = 7;
                    counter[6] = 9;
                    counter[7] = 11;
                    counter[8] = 14;
                    break;
            }

            cycleDic = counter.Select((x, i) => new { key = i, value = x }).ToDictionary(x => x.key, x => x.value);

            return cycleDic;
        }
    }
}
