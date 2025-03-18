namespace SteamToys.Contact.Model.SteamService
{
    public class BoundDataExcelOutput
    {
        public SteamAccount Account { get; set; }

        public EmailboxOption EmailboxOption { get; set; }

        public SmsOption SmsOption { get; set; }

        public OtherOption OtherOption { get; set; }

        public AuthenticatorLinker AuthLinker { get; set; }

        public long Id { get; set; }

        /// <summary>
        /// 报价链接
        /// </summary>
        public string Tradeoffer { get; set; }

        public bool BoundSucceed { get; set; }

        public string BoundDate { get; set; }

        public string BoundMsg { get; set; }

        public BoundDataExcelOutput Filed(string msg)
        {
            BoundMsg = msg;
            BoundSucceed = false;
            BoundDate = DateTime.Now.ToString("F");
            return this;
        }
    }
}
