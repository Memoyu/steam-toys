namespace SteamToys.Service.Sms.Configurations;

public class DestroyPhoneNumberItem
{
    public SmsPlatform Platform { get; set; }

    public string Id { get; set; }

    public string PhoneNumber { get; set; }

    public DateTime GenerateTime { get; set; }

    public int RetryCount { get; set; }
}
