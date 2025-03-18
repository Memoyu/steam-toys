namespace SteamToys.Contact.Model.SteamService;

public class FinalizeAuthenticatorResponse
{
    [JsonProperty("response")]
    public FinalizeAuthenticatorInternalResponse Response { get; set; }
}

public class FinalizeAuthenticatorInternalResponse
{

    [JsonProperty("status")]
    public int Status { get; set; }

    [JsonProperty("server_time")]
    public long ServerTime { get; set; }

    [JsonProperty("want_more")]
    public bool WantMore { get; set; }

    [JsonProperty("success")]
    public bool Success { get; set; }
}
