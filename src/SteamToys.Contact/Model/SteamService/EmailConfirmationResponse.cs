namespace SteamToys.Contact.Model.SteamService;

public class EmailConfirmationResponse : BaseResponse
{
    [JsonProperty("email_confirmation")]
    public bool EmailConfirmation { get; set; }
}
