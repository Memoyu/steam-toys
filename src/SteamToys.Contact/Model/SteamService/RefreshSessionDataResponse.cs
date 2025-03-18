namespace SteamToys.Contact.Model.SteamService;

public class RefreshSessionDataResponse
{
    [JsonProperty("response")]
    public RefreshSessionDataInternalResponse Response { get; set; }

}

public class RefreshSessionDataInternalResponse
{
    [JsonProperty("token")]
    public string Token { get; set; }

    [JsonProperty("token_secure")]
    public string TokenSecure { get; set; }
}
