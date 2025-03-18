namespace SteamToys.Service.Sms.Configurations;

public class GetPhoneNumberStatusRequest
{
    public SmsPlatform Platform { get; set; }

    public string Id { get; set; }

    public int WaitTime { get; set; }
}
