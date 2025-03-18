namespace SteamToys.Contact.Model.SteamService;

public class SendPhoneVerificationCodeResponse
{
    [JsonProperty("response")]
    public SendPhoneVerificationCodeInternalResponse Response { get; set; }
}

public class SendPhoneVerificationCodeInternalResponse
{
}
