namespace Lottery.Core.Data
{
    /// <summary>
    /// 任选过滤条件
    /// </summary>
    public class AnyFilter
    {
        /// <summary>
        /// 位置索引
        /// </summary>
        public int Pos { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public int[] Values { get; set; }
    }
}
