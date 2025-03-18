namespace SteamToys.Service.Sms.Configurations;

public class GetPhoneNumber
{
    public string Id { get; set; }

    public string PhoneNumber { get; set; }

    public DateTime GenerateTime { get; set; }

    public SmsPlatform Platform { get; set; }
}

public class GetPhoneNumberResponse
{
    public string Id { get; set; }

    public string PhoneNumber { get; set; }

    public bool IsRetry { get; set; }
}
