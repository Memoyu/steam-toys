namespace SteamToys.Contact.Model;

/// <summary>
/// 处理用户登录到移动Steam网站。生成OAuth令牌和会话cookie所必需的。
/// </summary>
public class UserLogin
{
    public long Id { get; set; }

    public ulong SteamID { get; set; }

    public bool RequiresCaptcha { get; set; }
    public string CaptchaGID { get; set; } = null;
    public string CaptchaText { get; set; } = null;

    public bool RequiresEmail { get; set; }

    public bool Requires2FA { get; set; }
    public string TwoFactorCode { get; set; } = null;
    public bool LoggedIn { get; set; } = false;

    public string OutputPath { get; set; }

    public SteamAccount Account { get; set; }

    public Privacy Privacy { get; set; }

    public SessionData Session { get; set; } = null;

    public EmailboxOption EmailboxOption { get; set; }

    public SmsOption SmsOption { get; set; }

    public OtherOption OtherOption { get; set; }

    public DateTime BeginBindTime { get; set; }

    public (bool Success, string Msg) VerifyReq()
    {
        if (string.IsNullOrEmpty(Account.Steam)) return (false, "用户名不能为空");
        if (string.IsNullOrEmpty(Account.SteamPassword)) return (false, "用户密码不能为空");
        if (string.IsNullOrEmpty(Account.Email)) return (false, "邮箱账号不能为空");
        if (string.IsNullOrEmpty(Account.EmailPassword)) return (false, "邮箱密码不能为空");

        return (true, "");
    }
}
