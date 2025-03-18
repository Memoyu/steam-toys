namespace SteamToys.Contact.Model.SteamService;

public class TimeQuery
{
    [JsonProperty("response")]
    public TimeQueryResponse Response { get; set; }
}

public class TimeQueryResponse
{
    [JsonProperty("server_time")]
    public long ServerTime { get; set; }
}
