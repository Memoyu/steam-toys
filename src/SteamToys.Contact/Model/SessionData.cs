namespace SteamToys.Contact.Model;

/// <summary>
/// 会话状态实体
/// </summary>
public class SessionData
{
    public ulong SteamID { get; set; }

    public string AccessToken { get; set; }

    public string RefreshToken { get; set; }

    public string SessionID { get; set; }
}
