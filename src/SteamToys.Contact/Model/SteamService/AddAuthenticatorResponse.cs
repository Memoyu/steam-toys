namespace SteamToys.Contact.Model.SteamService;

public class AddAuthenticatorResponse
{
    [JsonProperty("response")]
    public SteamGuardAccount Response { get; set; }
}