namespace SteamToys.Contact.Model.SteamService;

public class IsAccountWaitingForEmailConfirmationResponse
{
    [JsonProperty("response")]
    public IsAccountWaitingForEmailConfirmationInternalResponse Response { get; set; }
}

public class IsAccountWaitingForEmailConfirmationInternalResponse
{
    [JsonProperty("awaiting_email_confirmation")]
    public bool AwaitingEmailConfirmation { get; set; }

    [JsonProperty("seconds_to_wait")]
    public int SecondsToWait{ get; set; }
}
