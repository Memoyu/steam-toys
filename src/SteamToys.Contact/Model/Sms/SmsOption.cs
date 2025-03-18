namespace SteamToys.Contact.Model.Sms;

public class SmsOption
{
    /// <summary>
    /// 平台
    /// </summary>
    public SmsPlatform Platform { get; set; }

    /// <summary>
    /// 项目
    /// </summary>
    public string Service { get; set; }

    /// <summary>
    /// 国家信息
    /// </summary>
    public SmsCountryOption Country { get; set; }
}
