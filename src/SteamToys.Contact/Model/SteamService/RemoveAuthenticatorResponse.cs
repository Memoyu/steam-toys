namespace SteamToys.Contact.Model.SteamService;

public class RemoveAuthenticatorResponse
{
    [JsonProperty("response")]
    public RemoveAuthenticatorInternalResponse Response { get; set; }
}

public class RemoveAuthenticatorInternalResponse
{
    [JsonProperty("success")]
    public bool Success { get; set; }
}
