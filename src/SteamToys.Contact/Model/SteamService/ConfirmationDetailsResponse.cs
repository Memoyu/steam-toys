namespace SteamToys.Contact.Model.SteamService;

public class ConfirmationDetailsResponse
{
    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("html")]
    public string HTML { get; set; }
}
