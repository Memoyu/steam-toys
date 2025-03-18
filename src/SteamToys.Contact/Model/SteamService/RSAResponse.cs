namespace SteamToys.Contact.Model.SteamService;

public class RSAResponse
{
    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("publickey_exp")]
    public string Exponent { get; set; }

    [JsonProperty("publickey_mod")]
    public string Modulus { get; set; }

    [JsonProperty("timestamp")]
    public string Timestamp { get; set; }

    [JsonProperty("steamid")]
    public ulong SteamID { get; set; }
}
