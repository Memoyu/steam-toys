namespace SteamToys.Contact.Model;

/// <summary>
/// 身份验证器的链接过程。
/// </summary>
public class AuthenticatorLinker
{

    public bool ConfirmationEmailSent { get; set; }

    /// <summary>
    /// 设置为在链接时注册一个新的电话号码。如果帐号上没有设置电话号码，则必须设置电话号码。如果在帐户上设置了电话号码，则该值必须为空。
    /// </summary>
    public string PhoneNumber { get; set; } = null;


    public DateTime BingPhoneDate { get; set; }

    /// <summary>
    /// 验证码
    /// </summary>
    public string SmsCode { get; set; } = null;


    public string TzId { get; set; }

    /// <summary>
    /// 随机生成的设备ID。每个链接只应该生成一次。
    /// </summary>
    public string DeviceID { get; set; }

    /// <summary>
    /// 在初始链接步骤之后，如果成功，这将是该帐户的SteamGuard数据。请在生成后将其保存在某个地方;这是至关重要的数据。
    /// </summary>
    public SteamGuardAccount LinkedAccount { get; set; }

    /// <summary>
    /// True if the authenticator has been fully finalized.
    /// </summary>
    public bool Finalized { get; set; } = false;

    public SessionData Session { get; set; }

    public AuthenticatorLinker()
    {
    }

    public AuthenticatorLinker(SessionData session)
    {
        Session = session;
        DeviceID = Util.GenerateDeviceID();
    }
}
