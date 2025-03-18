namespace SteamToys.Contact.Model.Sms;

public class GetNumStateOnlineSimResp
{
    public string Response { get; set; }

    public string Tzid { get; set; }

    public string Service { get; set; }

    public string Number { get; set; }

    public string Msg { get; set; }

    public string Time { get; set; }

    public string Form { get; set; }

    [JsonProperty("forward_status")]
    public string ForwardStatus { get; set; }

    [JsonProperty("forward_number")]
    public string ForwardNumber { get; set; }

    public string Country { get; set; }
}
