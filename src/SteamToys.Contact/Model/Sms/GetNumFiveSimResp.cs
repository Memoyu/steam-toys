namespace SteamToys.Contact.Model.Sms;

public class GetNumFiveSimResp
{
    public long Id { get; set; }

    public string Phone { get; set; }

    public string Operator { get; set; }

    public string Product { get; set; }

    public decimal Price { get; set; }

    public string Status { get; set; }

    public string Expires { get; set; }

    public List<FiveSimSms>? Sms { get; set; }

    [JsonProperty("created_at")]
    public string CreatedAt { get; set; }

    public bool Forwarding { get; set; }

    [JsonProperty("forwarding_number")]
    public string ForwardingNumber { get; set; }

    public string Country { get; set; }
}


public class FiveSimSms
{
    [JsonProperty("created_at")]
    public string CreatedAt { get; set; }
    public DateTime Date { get; set; }
    public string Sender { get; set; }
    public string Text { get; set; }
    public string Code { get; set; }
}

