namespace SteamToys.Contact.Model;

public class OtherOption
{
    public Proxy? Proxy { get; set; }

    public int RequestRetry { get; set; }

    public int AccountRetry { get; set; }

    public int WaitCodeTime { get; set; }

    public bool IsGetTradeoffers { get; set; }
}
