namespace SteamToys.Contact.Model.Client;

public class FinalizeLoginStatus
{
    [JsonProperty("steamID")]
    public string? SteamId { get; set; }

    [JsonProperty("redir")]
    public string? Redir { get; set; }

    [JsonProperty("transfer_info")]
    public List<TransferInfo>? TransferInfo { get; set; }

    [JsonProperty("primary_domain")]
    public string? PrimaryDomain { get; set; }
}

public class TransferInfo
{
    [JsonProperty("url")]
    public string? Url { get; set; }

    [JsonProperty("params")]
    public TransferInfoParams? Params { get; set; }
}

public class TransferInfoParams
{
    [JsonProperty("nonce")]
    public string? Nonce { get; set; }

    [JsonProperty("auth")]
    public string? Auth { get; set; }
}
