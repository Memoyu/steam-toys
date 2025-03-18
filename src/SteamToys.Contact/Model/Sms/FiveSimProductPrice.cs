namespace SteamToys.Contact.Model.Sms
{
    public class FiveSimProductPrice
    {
        /// <summary>
        /// 操作名
        /// </summary>
        public string Operator { get; set; } = string.Empty;

        /// <summary>
        /// 价格
        /// </summary>
        public float Cost { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 交货率
        /// </summary>
        public float Rate { get; set; }
    }
}
