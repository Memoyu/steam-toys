namespace SteamToys.Contact.Model.SteamService;

public class LoginResponse
{
    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("login_complete")]
    public bool LoginComplete { get; set; }

    [JsonProperty("transfer_urls")]
    public List<string> TransferUrls { get; set; }

    [JsonProperty("transfer_parameters")]
    public TransferParameters TransferParameters { get; set; }

    [JsonProperty("captcha_needed")]
    public bool CaptchaNeeded { get; set; }

    [JsonProperty("captcha_gid")]
    public string CaptchaGID { get; set; }

    [JsonProperty("emailsteamid")]
    public ulong EmailSteamID { get; set; }

    [JsonProperty("emailauth_needed")]
    public bool EmailAuthNeeded { get; set; }

    [JsonProperty("requires_twofactor")]
    public bool TwoFactorNeeded { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }
}

public class TransferParameters
{
    [JsonProperty("auth")]
    public string Auth { get; set; }

    [JsonProperty("remember_login")]
    public bool RememberLogin { get; set; }

    [JsonProperty("steamid")]
    public ulong SteamID { get; set; }

    [JsonProperty("token_secure")]
    public string SteamLoginSecure { get; set; }

    [JsonProperty("webcookie")]
    public string Webcookie { get; set; }
}
