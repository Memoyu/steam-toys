namespace SteamToys.Contact.Model.SteamService;

public class BaseResponse
{
    [JsonProperty("success")]
    public bool Success { get; set; }
}
