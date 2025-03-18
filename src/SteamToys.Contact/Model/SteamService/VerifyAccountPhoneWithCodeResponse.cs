namespace SteamToys.Contact.Model.SteamService;

public class VerifyAccountPhoneWithCodeResponse
{
    [JsonProperty("response")]
    public VerifyAccountPhoneWithCodeInternalResponse Response { get; set; }
}

public class VerifyAccountPhoneWithCodeInternalResponse
{
}
